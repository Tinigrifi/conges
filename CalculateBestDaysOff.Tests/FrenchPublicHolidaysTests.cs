using CalculateBestDaysOff.Domain;

namespace CalculateBestDaysOff.Tests;

public class FrenchPublicHolidaysTests
{
    [Theory]
    [InlineData(2024, 3, 31)] // Pâques 2024
    [InlineData(2025, 4, 20)] // Pâques 2025
    [InlineData(2026, 4, 5)]  // Pâques 2026
    public void ComputeEaster_ReturnsKnownDate(int year, int month, int day)
    {
        var easter = FrenchPublicHolidays.ComputeEaster(year);
        Assert.Equal(new DateOnly(year, month, day), easter);
    }

    [Fact]
    public void For_Returns11Holidays()
    {
        var holidays = FrenchPublicHolidays.For(2025);
        Assert.Equal(11, holidays.Count);
    }

    [Theory]
    [InlineData(2025, 1, 1,  "Jour de l'An")]
    [InlineData(2025, 5, 1,  "Fête du Travail")]
    [InlineData(2025, 5, 8,  "Victoire 1945")]
    [InlineData(2025, 7, 14, "Fête Nationale")]
    [InlineData(2025, 8, 15, "Assomption")]
    [InlineData(2025, 11, 1, "Toussaint")]
    [InlineData(2025, 11, 11,"Armistice")]
    [InlineData(2025, 12, 25,"Noël")]
    public void For_ContainsFixedHolidays(int year, int month, int day, string name)
    {
        var holidays = FrenchPublicHolidays.For(year);
        var date = new DateOnly(year, month, day);
        Assert.True(holidays.ContainsKey(date));
        Assert.Equal(name, holidays[date]);
    }

    [Fact]
    public void For_2025_EasterBasedHolidaysAreCorrect()
    {
        // Pâques 2025 = 20 avril
        var holidays = FrenchPublicHolidays.For(2025);
        Assert.True(holidays.ContainsKey(new DateOnly(2025, 4, 21)));  // Lundi de Pâques
        Assert.True(holidays.ContainsKey(new DateOnly(2025, 5, 29)));  // Ascension (+39)
        Assert.True(holidays.ContainsKey(new DateOnly(2025, 6, 9)));   // Lundi de Pentecôte (+50)
    }
}
