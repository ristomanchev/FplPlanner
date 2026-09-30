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
GET  /api/excel/predictions                         # .xlsx
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

1. **Надворешно API + ETL**: `FplApiClient` е typed `HttpClient`. `FplEtlService` извршува Extract (`bootstrap-static`, `fixtures`), Transform и Load:
   - Transform: `element_type` → `Position`, `a/d/i/s/u/n` → `PlayerStatus`, `now_cost/10` → цена во милиони.
   - Load: upsert по `FplId`.

   Се извршува при старт и на секои 6 часа (`FplSyncBackgroundService`), или рачно со `POST /api/etl/run`. Увозот на менаџер го користи `entry/{id}` и `picks`.
2. **RabbitMQ**: по успешен ETL се праќа `FplDataSyncedMessage` на durable queue. `PredictionRecalculationConsumer` ги пресметува предвидувањата асинхроно. Поставки: prefetch 1, рачен ack, nack без requeue при грешка, една заедничка конекција со automatic recovery.
3. **Email**: `WeeklyReportService` го составува извештајот (капитен, трансфери, повредени/сомнителни играчи, постава) и го става `EmailMessage` во `IEmailQueue` (`Channel<EmailMessage>`). `EmailBackgroundService` ја чита редицата и праќа преку `SmtpEmailService` (MailKit, `EmailSettings`). Така HTTP барањето не чека SMTP. `WeeklyReportBackgroundService` го става извештајот во редицата 24 ч пред deadline, еднаш по коло (`Manager.LastReportedGameweek`).
4. **Excel** (ClosedXML):
   - извоз на предвидувања (xP по коло + детален пресмет),
   - извоз на тимот на менаџерот, кој служи и како шаблон,
   - увоз на тим: секој ред се проверува, па тимот минува низ истите FPL правила како и API-то.

## Одлуки при дизајнот

| Одлука | Зошто |
|---|---|
| Domain без EF/Identity пакети | Onion: јадрото не зависи од базата. Целата конфигурација е Fluent API во `ApplicationDbContext`. |
| `ExistsAsync`, `UpdateManyAsync`, `DeleteManyAsync`, `GetAsync` (наместо `Get`) | `EXISTS` во SQL наместо вчитување на цел ентитет; batch upsert за ETL; конзистентен `Async` суфикс. |
| `NotFoundException`/`BusinessRuleException` + `IExceptionHandler` | Контролерите немаат `try/catch`. 404/400 се враќаат во стандарден ProblemDetails формат, наместо 500. |
| Insert/Update повторно го вчитуваат ентитетот | Навигациите (`Club.ShortName`...) се пополнети за одговорот. |
| `decimal` → `double` во SQLite, UTC конвертор за `DateTime` | SQLite нема decimal (не може `ORDER BY`) и не чува временска зона. |
| Enums како string (во базата и во JSON) | Читливо, не зависи од редоследот во enum-от; невалидна вредност → 400. |
| Owned types (`PlayerSeasonStats`, `PointsBreakdown`) | Групирани поврзани полиња без дополнителни табели и JOIN-ови. |
| `Service/Logic` (статички, без I/O) | Алгоритмите се тестираат без база и HTTP. Сервисите само вчитуваат/зачувуваат околу нив. |
| Web е чист API | Frontend не е задолжителен. Scalar UI за тестирање. |

## Познати ограничувања

- FPL цената за продажба (купувачка цена + половина од растот) не е јавна, па за трансферите се користи тековната цена.
- Бројот на слободни трансфери не е во јавното API. Се чува во `Manager.FreeTransfers` (стандардно 1, може да се промени со CRUD).
- Нема автентикација. Сите endpoints се јавни, што е во ред за локална демонстрација.
