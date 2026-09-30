using System.Globalization;
using System.Net;
using System.Text;
using FplPlanner.Domain.Dto;

namespace FplPlanner.Service.Logic;

// Renders the weekly report as a simple, e-mail-client-safe HTML document (inline styles, tables).
public static class WeeklyReportHtmlBuilder
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    public static string Build(WeeklyReportDto report)
    {
        var html = new StringBuilder();
        html.Append("<html><body style=\"font-family:Arial,sans-serif;color:#222;max-width:640px\">");
        html.Append($"<h2 style=\"color:#37003c\">{E(report.TeamName)} — Gameweek {report.GameweekNumber}</h2>");
        html.Append($"<p>Hi {E(report.ManagerName)}, the deadline is <b>{report.Deadline.ToString("ddd d MMM, HH:mm", Culture)} UTC</b>.</p>");

        AppendCaptaincy(html, report.Lineup);
        AppendTransfers(html, report.TransferAdvice);
        AppendFlaggedPlayers(html, report.FlaggedPlayers);
        AppendLineup(html, report.Lineup);

        html.Append("<p style=\"color:#888;font-size:12px\">Expected points come from FPL Planner's Poisson model.</p>");
        html.Append("</body></html>");
        return html.ToString();
    }

    private static void AppendCaptaincy(StringBuilder html, LineupDto lineup)
    {
        html.Append("<h3>Captain</h3>");
        if (lineup.Captain == null)
        {
            html.Append("<p>No captain suggestion available.</p>");
            return;
        }

        html.Append($"<p>Captain <b>{E(lineup.Captain.WebName)}</b> ({P(lineup.Captain.ExpectedPoints)} xP)");
        if (lineup.ViceCaptain != null)
        {
            html.Append($", vice-captain <b>{E(lineup.ViceCaptain.WebName)}</b> ({P(lineup.ViceCaptain.ExpectedPoints)} xP)");
        }

        html.Append(".</p>");
    }

    private static void AppendTransfers(StringBuilder html, TransferAdviceDto advice)
    {
        var plan = advice.RecommendedPlan;
        html.Append($"<h3>Transfers (next {advice.GameweekNumbers.Count} gameweeks)</h3>");

        if (plan.Transfers.Count == 0)
        {
            html.Append("<p>Roll the transfer — no move clearly improves the squad.</p>");
            return;
        }

        html.Append("<table cellpadding=\"4\" style=\"border-collapse:collapse\">");
        html.Append("<tr style=\"background:#eee\"><th align=\"left\">Out</th><th align=\"left\">In</th><th>Price</th></tr>");
        foreach (var transfer in plan.Transfers)
        {
            html.Append($"<tr><td>{E(transfer.PlayerOut.WebName)} ({E(transfer.PlayerOut.ClubShortName)})</td>" +
                        $"<td>{E(transfer.PlayerIn.WebName)} ({E(transfer.PlayerIn.ClubShortName)})</td>" +
                        $"<td>£{P(transfer.PlayerIn.Price)}m</td></tr>");
        }

        html.Append("</table>");
        html.Append($"<p>Expected gain {P(plan.Gain)} pts");
        if (plan.HitCost > 0)
        {
            html.Append($", minus a {plan.HitCost}-point hit = <b>{P(plan.NetGain)}</b> net");
        }

        html.Append($". Money in the bank afterwards: £{P(plan.BankAfter)}m.</p>");
    }

    private static void AppendFlaggedPlayers(StringBuilder html, List<SquadMemberDto> flagged)
    {
        html.Append("<h3>Injuries and doubts</h3>");
        if (flagged.Count == 0)
        {
            html.Append("<p>Everyone in the squad is available.</p>");
            return;
        }

        html.Append("<ul>");
        foreach (var player in flagged)
        {
            var chance = player.ChanceOfPlaying is { } c ? $" — {c}% chance of playing" : string.Empty;
            var news = string.IsNullOrWhiteSpace(player.News) ? string.Empty : $": {E(player.News)}";
            html.Append($"<li><b>{E(player.WebName)}</b> ({player.Status}{chance}){news}</li>");
        }

        html.Append("</ul>");
    }

    private static void AppendLineup(StringBuilder html, LineupDto lineup)
    {
        html.Append($"<h3>Suggested starting XI ({P(lineup.ExpectedPoints)} xP)</h3><ol>");
        foreach (var player in lineup.StartingEleven)
        {
            html.Append($"<li>{E(player.WebName)} — {player.Position}, {P(player.ExpectedPoints)} xP</li>");
        }

        html.Append("</ol><p>Bench: ");
        html.Append(string.Join(", ", lineup.Bench.Select(p => E(p.WebName))));
        html.Append("</p>");
    }

    private static string E(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

    private static string P(decimal value) => value.ToString("0.0", Culture);
}
