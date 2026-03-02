namespace CalculateBestDaysOff.Domain;

public static class FrenchPublicHolidays
{
    public static IReadOnlyDictionary<DateOnly, string> For(int year)
    {
        var easter = ComputeEaster(year);
        return new Dictionary<DateOnly, string>
        {
            [new(year, 1, 1)]      = "Jour de l'An",
            [easter.AddDays(1)]    = "Lundi de Pâques",
            [new(year, 5, 1)]      = "Fête du Travail",
            [new(year, 5, 8)]      = "Victoire 1945",
            [easter.AddDays(39)]   = "Ascension",
            [easter.AddDays(50)]   = "Lundi de Pentecôte",
            [new(year, 7, 14)]     = "Fête Nationale",
            [new(year, 8, 15)]     = "Assomption",
            [new(year, 11, 1)]     = "Toussaint",
            [new(year, 11, 11)]    = "Armistice",
            [new(year, 12, 25)]    = "Noël",
        };
    }

    // Algorithme de Meeus/Jones/Butcher
    public static DateOnly ComputeEaster(int year)
    {
        int a = year % 19;
        int b = year / 100;
        int c = year % 100;
        int d = b / 4;
        int e = b % 4;
        int f = (b + 8) / 25;
        int g = (b - f + 1) / 3;
        int h = (19 * a + b - d - g + 15) % 30;
        int i = c / 4;
        int k = c % 4;
        int l = (32 + 2 * e + 2 * i - h - k) % 7;
        int m = (a + 11 * h + 22 * l) / 451;
        int month = (h + l - 7 * m + 114) / 31;
        int day = ((h + l - 7 * m + 114) % 31) + 1;
        return new DateOnly(year, month, day);
    }
}
