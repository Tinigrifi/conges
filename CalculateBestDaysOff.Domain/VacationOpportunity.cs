namespace CalculateBestDaysOff.Domain;

public sealed record VacationOpportunity(
    DateOnly Start,
    DateOnly End,
    int TotalDaysOff,
    int DaysTaken,
    double Ratio,
    IReadOnlyList<string> HolidaysIncluded,
    IReadOnlyList<DateOnly> WorkDaysTaken);
