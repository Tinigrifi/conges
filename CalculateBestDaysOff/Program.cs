using System.Globalization;
using CalculateBestDaysOff.Domain;
using Spectre.Console;

// --- Parsing des arguments ---
int year = DateTime.Now.Year;
int maxDays = 10;
int top = 10;
int weeks = 0;        // --weeks N : filtre min N semaines consécutives off
bool maxDaysExplicit = false;

if (args.Contains("--help") || args.Contains("-h"))
{
    AnsiConsole.Write(new FigletText("Conges").Color(Color.Orange1));

    var help = new Table().NoBorder().HideHeaders()
        .AddColumn(new TableColumn("").PadRight(2))
        .AddColumn("");

    help.AddRow("[bold yellow]USAGE[/]", "");
    help.AddRow("",  "[grey]dotnet run --[/] [cyan][[année]][/] [grey][[options]][/]");
    help.AddRow("", "");
    help.AddRow("[bold yellow]ARGUMENT[/]", "");
    help.AddRow("[cyan]année[/]",       $"Année à analyser [grey](défaut : {DateTime.Now.Year})[/]");
    help.AddRow("", "");
    help.AddRow("[bold yellow]OPTIONS[/]", "");
    help.AddRow("[cyan]--weeks [/][grey]<n>[/]",    "Affiche uniquement les blocs d'au moins [bold]n semaines[/] consécutives\n[grey]Ajuste automatiquement --max-days si nécessaire[/]");
    help.AddRow("[cyan]--max-days[/] [grey]<n>[/]", $"Nombre maximum de jours de congé posés par opportunité [grey](défaut : 10)[/]");
    help.AddRow("[cyan]--top[/] [grey]<n>[/]",      $"Nombre de résultats à afficher [grey](défaut : 10)[/]");
    help.AddRow("[cyan]--help[/][grey], -h[/]",     "Affiche cette aide");
    help.AddRow("", "");
    help.AddRow("[bold yellow]EXEMPLES[/]", "");
    help.AddRow("", "[grey]dotnet run --[/] [cyan]2025[/]");
    help.AddRow("", "[grey]dotnet run --[/] [cyan]2026 --top 5[/]");
    help.AddRow("", "[grey]dotnet run --[/] [cyan]2025 --weeks 2[/]             [dim]# meilleurs blocs de 2 semaines[/]");
    help.AddRow("", "[grey]dotnet run --[/] [cyan]2025 --weeks 2 --max-days 7  [dim]# idem, max 7 jours posés[/][/]");

    AnsiConsole.Write(new Panel(help)
        .Header("[bold]Aide[/]")
        .BorderColor(Color.Orange1));
    return;
}

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--max-days" && i + 1 < args.Length && int.TryParse(args[i + 1], out int md))
    {
        maxDays = md; maxDaysExplicit = true; i++;
    }
    else if (args[i] == "--top" && i + 1 < args.Length && int.TryParse(args[i + 1], out int t))
    {
        top = t; i++;
    }
    else if (args[i] == "--weeks" && i + 1 < args.Length && int.TryParse(args[i + 1], out int w))
    {
        weeks = w; i++;
    }
    else if (int.TryParse(args[i], out int y) && y is >= 1900 and <= 2100)
    {
        year = y;
    }
}

// Pour N semaines on a besoin d'au moins N*5 jours posables (cas sans aucun férié)
if (weeks > 0 && !maxDaysExplicit)
    maxDays = Math.Max(maxDays, weeks * 5);

int minDays = weeks > 0 ? weeks * 7 : 0;

// --- En-tête ---
var fr = CultureInfo.GetCultureInfo("fr-FR");

AnsiConsole.Write(new FigletText("Conges").Color(Color.Orange1));
AnsiConsole.MarkupLine($"[grey]Année :[/] [bold yellow]{year}[/]   " +
                       $"[grey]Max jours posés :[/] [bold yellow]{maxDays}[/]   " +
                       $"[grey]Top :[/] [bold yellow]{top}[/]" +
                       (weeks > 0 ? $"   [grey]Filtre :[/] [bold cyan]≥ {weeks} sem. ({minDays} j)[/]" : ""));
AnsiConsole.WriteLine();

// --- Jours fériés ---
var holidaysPanel = new Table().NoBorder().HideHeaders();
holidaysPanel.AddColumn(new TableColumn("").Padding(0, 0));
holidaysPanel.AddColumn(new TableColumn("").Padding(0, 0));

var holidays = FrenchPublicHolidays.For(year);
foreach (var (date, name) in holidays.OrderBy(kv => kv.Key))
    holidaysPanel.AddRow(
        $"[dim]{date.ToString("ddd d MMM", fr)}[/]",
        $"[dim italic]{Markup.Escape(name)}[/]");

AnsiConsole.Write(new Panel(holidaysPanel)
    .Header("[bold]Jours fériés[/]")
    .BorderColor(Color.Grey));
AnsiConsole.WriteLine();

// --- Calcul ---
var results = AnsiConsole
    .Status()
    .Spinner(Spinner.Known.Dots)
    .Start("[yellow]Calcul en cours…[/]", _ =>
        VacationOptimizer.FindBest(year, maxDays)
            .Where(o => minDays == 0 || o.TotalDaysOff >= minDays)
            .Take(top)
            .ToList());

if (results.Count == 0)
{
    AnsiConsole.MarkupLine($"[red]Aucune opportunité trouvée avec ≥ {minDays} jours consécutifs (max {maxDays} jours posés).[/]");
    AnsiConsole.MarkupLine("[dim]Essayez d'augmenter --max-days.[/]");
    return;
}

// --- Tableau des résultats ---
string tableTitle = weeks > 0
    ? $"[bold gold1]🏖  Top {results.Count} blocs de {weeks} semaine(s) — {year}[/]"
    : $"[bold gold1]🏖  Top {results.Count} opportunités de congés {year}[/]";
var table = new Table()
    .Border(TableBorder.Rounded)
    .BorderColor(Color.Grey)
    .Title(tableTitle)
    .AddColumn(new TableColumn("[bold]#[/]").RightAligned())
    .AddColumn("[bold]Période[/]")
    .AddColumn(new TableColumn("[bold]Durée[/]").RightAligned())
    .AddColumn(new TableColumn("[bold]Posés[/]").RightAligned())
    .AddColumn(new TableColumn("[bold]Ratio[/]").RightAligned())
    .AddColumn("[bold]Fériés inclus[/]")
    .AddColumn("[bold]Jours à poser[/]");

for (int rank = 0; rank < results.Count; rank++)
{
    var o = results[rank];

    var (ratioColor, ratioEmoji) = o.Ratio switch
    {
        >= 7 => ("bold gold1", "✨"),
        >= 5 => ("bold green", "🌟"),
        >= 3 => ("green", "👍"),
        >= 2 => ("yellow", ""),
        _    => ("white", ""),
    };

    string rankStr   = rank == 0 ? "[bold gold1]🥇 1[/]"
                     : rank == 1 ? "[bold silver]🥈 2[/]"
                     : rank == 2 ? "[bold orange1]🥉 3[/]"
                     : $"[dim]{rank + 1}[/]";

    string period = $"{o.Start.ToString("ddd d MMM", fr)} [dim]→[/] {o.End.ToString("ddd d MMM", fr)}";
    string total  = $"[bold]{o.TotalDaysOff} j[/]";
    string taken  = $"{o.DaysTaken} j";
    string ratio  = $"[{ratioColor}]{ratioEmoji} {o.Ratio:F1}[/]";
    string feries = o.HolidaysIncluded.Count > 0
        ? string.Join(", ", o.HolidaysIncluded.Select(Markup.Escape))
        : "[dim]-[/]";
    string workDays = string.Join(", ", o.WorkDaysTaken.Select(d => d.ToString("dd/MM", fr)));

    table.AddRow(rankStr, period, total, taken, ratio, feries, workDays);
}

AnsiConsole.Write(table);
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[dim]Ratio = jours off consécutifs obtenus / jours de congé posés[/]");

