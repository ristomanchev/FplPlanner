# FPL Planner

Backend за Fantasy Premier League 2026/27. Ги презема податоците од јавното FPL API, ги чува во SQLite база и за секој FPL менаџер нуди:
- валидација на тимот според FPL правилата,
- очекувани поени по играч и коло (Poisson модел),
- најдобра постава и капитен,
- предлози за трансфери.

Проект по предметот Интегрирани системи.

## Стартување

Потребни локални сервиси, еднократно преку Homebrew:

```bash
brew install rabbitmq mailpit
brew services start rabbitmq         # AMQP :5672, UI http://localhost:15672 (guest/guest)
brew services start mailpit          # SMTP :1025, inbox http://localhost:8025
```

Стартување:

```bash
cd ProektIntegrirani.Repository && dotnet ef database update && cd ..
dotnet run --project ProektIntegrirani.Web --launch-profile http
```

Во Development мејловите одат во локалниот Mailpit (`appsettings.Development.json`). За вистински мејлови се пополнуваат Gmail поставките во `EmailSettings` во `appsettings.json` (App Password).

- API UI (Scalar): http://localhost:5092/scalar
- При старт апликацијата сама ги повлекува податоците од FPL (ETL). Потоа преку RabbitMQ ги пресметува предвидувањата.
- Тестови: `dotnet test`

Ако RabbitMQ е недостапен, ETL-от сепак ги зачувува податоците, а предвидувањата се пресметуваат рачно со `POST /api/playerpredictions/recalculate`. Без SMTP сервер не се праќаат мејлови.

Брз пример, од нула до предлог за трансфер:

```
POST /api/etl/run                                   # клубови, кола, играчи, натпревари
POST /api/managers/import  {"fplEntryId": 1, "email": "me@example.com"}
POST /api/playerpredictions/recalculate?horizon=6
GET  /api/squads/{managerId}/lineup                 # постава + капитен
GET  /api/squads/{managerId}/transfer-suggestions?horizon=5&maxTransfers=2
POST /api/reports/{managerId}/weekly/send           # мејл → http://localhost:8025
GET  /api/export/predictions                        # .xlsx
```

## Архитектура (Onion)

```
        ┌──────────────────────────────────────────┐
        │ Web: Controllers → Mapper → Service      │  HTTP, Request/Response, DI, exception handler
        │  ┌────────────────────────────────────┐  │
        │  │ Service: бизнис логика, интеграции │  │  сервиси, Logic (чисти алгоритми), Jobs
        │  │  ┌──────────────────────────────┐  │  │
        │  │  │ Repository: EF Core, SQLite  │  │  │  ApplicationDbContext, IRepository<T>
        │  │  │  ┌────────────────────────┐  │  │  │
        │  │  │  │ Domain                 │  │  │  │  модели, DTO, enum, правила, исклучоци
        │  │  │  └────────────────────────┘  │  │  │
        │  │  └──────────────────────────────┘  │  │
        │  └────────────────────────────────────┘  │
        └──────────────────────────────────────────┘
```

Зависностите одат само кон внатре: Repository → Domain, Service → Domain + Repository, Web → сите три. Domain нема ниту еден NuGet пакет.

| Проект | Содржина |
|---|---|
| `Domain` | `Common/BaseEntity`, `Models`, `ValueObjects` (owned types), `Dto`, `Enums`, `ExternalModels` (FPL API), `Messages`, `Configuration`, `Rules` (FPL правила и бодување), `Exceptions` |
| `Repository` | `ApplicationDbContext` (Fluent API), генерички `IRepository<T>`/`Repository<T>`, `Converters`, `Migrations` |
| `Service` | `Interface`/`Implementation` сервиси, `Logic` (валидатор, Poisson модел, оптимизатор; без I/O), `Jobs` (background services) |
| `Web` | `Controllers` (`api/[controller]`), `Mapper` (еден по entity/функционалност), `Request`, `Response`, `Extensions` (`ToResponse`/`ToDto`, DI), `Middlewares` |
| `Tests` | xUnit тестови за чистата логика |

Тек на едно барање: `Controller` → `Mapper` (Request → DTO) → `Service` (бизнис правила) → `IRepository<T>` → EF Core. Одговорот оди обратно: `Mapper` (entity → Response).

## Модели

| Entity | Опис |
|---|---|
| `Club` | клуб: FplId, име, кратенка, FPL јачина (home/away) |
| `Player` | играч: позиција, цена, статус, шанса за играње, вести; `Stats` (owned) = сезонска статистика |
| `Gameweek` | коло 1–38: deadline, завршено |
| `Fixture` | натпревар: **две FK кон Club** (домаќин/гостин, без cascade), опционално коло |
| `Manager` | FPL менаџер: entry id, тим, email, банка, слободни трансфери |
| `ApiClient` | надворешен систем што смее да го повикува `/api/external`: име, SHA-256 hash на API клучот, активен, лимит барања/мин |
| `InboundSquadEntry` | тим пратен од надворешен систем: суров payload, статус Pending/Processing/Completed/Failed, грешка, FK кон `ApiClient` |
| `SquadPick` | **тернарна релација Manager × Gameweek × Player**: позиција 1–15 (12–15 клупа), капитен, вице. Unique index на (ManagerId, GameweekId, PlayerId) |
| `PlayerPrediction` | Player × Gameweek × ModelType: очекувани поени + `Breakdown` (owned) по категорија |

Правила за бришење:
- Менаџер → неговите picks се бришат (cascade).
- Играч или коло што е во нечиј тим не може да се избрише (restrict). Сервисот го проверува ова пред бришење и враќа 400 со јасна порака.
- Предвидувањата се бришат со играчот или колото, бидејќи се изведени податоци.

## Бизнис логика

- **Валидација на тим** (`SquadValidator`):
  - 15 играчи во составот 2 GK / 5 DEF / 5 MID / 3 FWD,
  - најмногу 3 од ист клуб,
  - формација на првите 11: 1 GK, 3–5 DEF, 2–5 MID, 1–3 FWD,
  - точно еден капитен и еден вице, и двајцата во првите 11,
  - уникатни играчи и позиции.

  Се враќаат **сите** прекршувања одеднаш (`ProblemDetails.errors`).
- **Poisson модел** (`PoissonPredictionModel`):
  1. Рејтинг за напад/одбрана на тимот од голови и xG, „смалени“ кон приор од FPL јачината.
  2. λ за постигнати/примени голови по натпревар, со домашна предност.
  3. Очекувани минути од стартови и статус на повреда.
  4. Стапки на 90 минути (xG, xA, одбранбени придонеси, одбрани, бонус, картони), смалени кон приор по позиција и цена.
  5. Поени според FPL бодувањето.
- **Оптимизатор** (`SquadOptimizer`):
  - најдобри 11 + капитен (двојно) + 0.1 × клупа,
  - сите поединечни трансфери што го подобруваат тимот,
  - greedy планови со 1..N трансфери и −4 за секој трансфер над бесплатните.

  Дополнителен трансфер со −4 се препорачува само ако нето добивката е поголема за барем 2 поени, бидејќи моделот има шум.

## Интеграции

1. **Надворешно API + ETL**:
   - Extract: `FplApiClient` (typed `HttpClient`) ги влече `bootstrap-static` и `fixtures`.
   - Transform: `FplTransformations` ги претвора во доменски ентитети: `element_type` → `Position`, `a/d/i/s/u/n` → `PlayerStatus`, `now_cost/10` → цена во милиони. Id-то се добива со `GuidHelper.FromExternalId("Player", fplId)`, па истиот FPL запис секогаш добива исто Id, а FK-ите (`Player.ClubId`, `Fixture.GameweekId`) се пресметуваат без пребарување.
   - Load: `IFplDataRepository` со `BulkInsertOrUpdate` (EFCore.BulkExtensions), по една операција за секоја табела.
   - Секое извршување се запишува во `EtlSyncLog` (почеток, крај, успех, грешка, број на записи) во `try/catch/finally`. Дневникот е достапен на `GET /api/etl/logs`.

   Се извршува при старт и на секои 6 часа (`FplEtlBackgroundService`), или рачно со `POST /api/etl/run` (502 ако FPL API не одговори).
   Увозот на менаџер ги користи `entry/{id}` и `picks`. Одговорите се кешираат во `IMemoryCache` (`CacheExpirationMinutes`).
2. **RabbitMQ**: по успешен ETL се праќа `FplDataSyncedMessage` на durable queue. `PredictionRecalculationConsumer` ги пресметува предвидувањата асинхроно. Поставки: prefetch 1, рачен ack, nack без requeue при грешка, една заедничка конекција со automatic recovery.
3. **Email**: `WeeklyReportService` го составува извештајот (капитен, трансфери, повредени/сомнителни играчи, постава) и го става `EmailMessage` во `IEmailQueue` (`Channel<EmailMessage>`). `EmailBackgroundService` ја чита редицата и праќа преку `SmtpEmailService` (MailKit, `EmailSettings`). Така HTTP барањето не чека SMTP. Извештајот носи и Excel прилог со предвидувањата (`EmailAttachment`). `QuartzWeeklyReportJob` (Quartz, cron на секој час) го става извештајот во редицата 24 ч пред deadline, еднаш по коло (`Manager.LastReportedGameweek`).
4. **Excel** (ClosedXML):
   - `ExportController` + `IExcelExportService` (враќа `byte[]`): предвидувања (xP по коло + детален пресмет) и тимот на менаџерот во форматот за увоз.
   - `ImportController` + `IExcelImportService`: проверка на фајлот (празен, `.xlsx`, до 5 MB), задолжителни колони, проверка на секој ред → `ImportResult<T>` со `ImportError` (ред, колона, порака). Шаблон: `GET /api/import/squads/get-import-template`.
   - Ако сите редови се валидни, тимот се зачувува преку `ISquadService`, со истите FPL правила како и API-то.

5. **Inbound REST API**:
   - Надворешен систем праќа тим со `POST /api/external/squads` и header `X-Api-Key`. Тимот се идентификува со FPL id-ја: `fplEntryId`, `gameweekNumber`, `picks[fplId, squadPosition, isCaptain, isViceCaptain]`.
   - `ApiKeyAuthMiddleware` го проверува клучот (401 ако недостига или е невалиден). Rate limiter-от `external-api` има посебен прозорец за секој клуч, со лимит од `ApiClient.RequestsPerMinute`. Над лимитот враќа 429.
   - Барањето само го зачувува payload-от како `InboundSquadEntry` (`Pending`) и враќа **202 Accepted** со id.
   - `InboundSquadProcessingBackgroundService` на секои 10 секунди го повикува `InboundSquadEntryProcessor`. Тој зема најмногу 10 записи со статус `Pending`, најстарите прво, го наоѓа менаџерот и играчите и го зачувува тимот преку `ISquadService` (истите FPL правила). Резултатот е `Completed` со `ManagerId`, или `Failed` со сите прекршени правила во `ErrorMessage`.
   - `GET /api/external/squads/{id}/status` го враќа статусот. Клиентот ги гледа само своите записи.
   - Администрација: CRUD на `/api/apiclients` (клучот се прикажува само при креирање или `regenerate-key`), и `/api/inboundsquadentries` (листа по статус, детали, `retry` за Failed, бришење).

## Одлуки при дизајнот

| Одлука | Зошто |
|---|---|
| Domain без EF/Identity пакети | Onion: јадрото не зависи од базата. Целата конфигурација е Fluent API во `ApplicationDbContext`. |
| `ExistsAsync`, `UpdateManyAsync`, `DeleteManyAsync`, `GetAsync` (наместо `Get`) | `EXISTS` во SQL наместо вчитување на цел ентитет; batch upsert за ETL; конзистентен `Async` суфикс. |
| `NotFoundException`/`BusinessRuleException` + `IExceptionHandler` | Контролерите немаат `try/catch`. 404/400 се враќаат во стандарден ProblemDetails формат, наместо 500. |
| Insert/Update повторно го вчитуваат ентитетот | Навигациите (`Club.ShortName`...) се пополнети за одговорот. |
| `decimal` → `double` во SQLite, UTC конвертор за `DateTime` | SQLite нема decimal (не може `ORDER BY`) и не чува временска зона. |
| Enums како string (во базата и во JSON) | Читливо, не зависи од редоследот во enum-от; невалидна вредност → 400. |
| `BaseAuditableEntity` + `AuditInterceptor` + `ICurrentUser` за `Manager`, `SquadPick`, `ApiClient` | Без кориснички сметки. „Корисникот“ е `api-client:<име>` за надворешни системи, `api` за обични HTTP барања и `system` за background job-ови. `CurrentUser` е во Web, бидејќи го чита `HttpContext`, па Service не зависи од ASP.NET. |
| Надворешниот клуч (FplId, број на коло, FplEntryId) не може да се менува | Id-то се пресметува од него (`GuidHelper`); ако се смени, CRUD записот и ETL записот би се разминале. |
| Quartz за неделниот извештај, BackgroundService за ETL/inbound/queue | Quartz кога е потребен cron распоред; `BackgroundService` за континуирани циклуси. |
| Owned types (`PlayerSeasonStats`, `PointsBreakdown`) | Групирани поврзани полиња без дополнителни табели и JOIN-ови. |
| `Service/Logic` (статички, без I/O) | Алгоритмите се тестираат без база и HTTP. Сервисите само вчитуваат/зачувуваат околу нив. |
| API клучевите се чуваат како SHA-256 hash | Како лозинките: ако базата протече, клучевите не се употребливи. Клучот се прикажува само еднаш. |
| Middleware прави `return` по секој 401 и го користи `IApiClientService` | Невалидно барање никогаш не стига до контролерот. Web слојот не пристапува директно до `DbContext`. |
| Rate limit по клиент (`RequestsPerMinute`) | Различни партнери можат да имаат различен лимит, наместо еден фиксен број за сите. |
| Web е чист API | Frontend не е задолжителен. Scalar UI за тестирање. |

## Познати ограничувања

- FPL цената за продажба (купувачка цена + половина од растот) не е јавна, па за трансферите се користи тековната цена.
- Бројот на слободни трансфери не е во јавното API. Се чува во `Manager.FreeTransfers` (стандардно 1, може да се промени со CRUD).
- Нема JWT/Identity, бидејќи доменот нема кориснички сметки (менаџерите се FPL тимови). Надворешниот API (`/api/external`) е заштитен со API клуч. Останатите endpoints се за локална администрација и демонстрација.
