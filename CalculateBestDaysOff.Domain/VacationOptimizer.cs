namespace CalculateBestDaysOff.Domain;

public static class VacationOptimizer
{
    /// <summary>
    /// Trouve toutes les opportunités de congés pour l'année donnée.
    /// Un fenêtre [start, end] est valide si elle contient entre 1 et maxDaysTaken jours ouvrés
    /// et est maximale (ne peut pas être étendue sans ajouter un jour ouvré).
    /// </summary>
    public static IReadOnlyList<VacationOpportunity> FindBest(int year, int maxDaysTaken)
    {
        var holidays = FrenchPublicHolidays.For(year);
        var jan1 = new DateOnly(year, 1, 1);
        var dec31 = new DateOnly(year, 12, 31);

        bool IsFree(DateOnly d) =>
            d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
            || holidays.ContainsKey(d);

        var opportunities = new List<VacationOpportunity>();

        for (var start = jan1; start <= dec31; start = start.AddDays(1))
        {
            // On ne démarre une fenêtre qu'au début d'un bloc libre (ou en Jan 1)
            // pour garantir que la fenêtre est maximale à gauche.
            if (start != jan1 && IsFree(start.AddDays(-1)))
                continue;

            int workCount = 0;
            for (var end = start; end <= dec31; end = end.AddDays(1))
            {
                if (!IsFree(end)) workCount++;
                if (workCount > maxDaysTaken) break;

                // La fenêtre est maximale à droite si le lendemain est ouvré (ou fin d'année)
                bool rightBlocked = end == dec31 || !IsFree(end.AddDays(1));
                if (rightBlocked && workCount > 0)
                {
                    int total = end.DayNumber - start.DayNumber + 1;
                    var holidaysInWindow = EnumerateDays(start, end)
                        .Where(holidays.ContainsKey)
                        .Select(d => holidays[d])
                        .ToList();

                    var workDaysInWindow = EnumerateDays(start, end)
                        .Where(d => !IsFree(d))
                        .ToList();

                    opportunities.Add(new VacationOpportunity(
                        start, end, total, workCount,
                        (double)total / workCount,
                        holidaysInWindow,
                        workDaysInWindow));
                }
            }
        }

        return [.. opportunities
            .OrderByDescending(o => o.Ratio)
            .ThenByDescending(o => o.TotalDaysOff)];
    }

    private static IEnumerable<DateOnly> EnumerateDays(DateOnly start, DateOnly end)
    {
        for (var d = start; d <= end; d = d.AddDays(1))
            yield return d;
    }
}
