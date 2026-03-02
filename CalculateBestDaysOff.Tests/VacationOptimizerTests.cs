using CalculateBestDaysOff.Domain;

namespace CalculateBestDaysOff.Tests;

public class VacationOptimizerTests
{
    [Fact]
    public void FindBest_ReturnsNonEmptyList()
    {
        var results = VacationOptimizer.FindBest(2025, 5);
        Assert.NotEmpty(results);
    }

    [Fact]
    public void FindBest_ResultsAreSortedByRatioDescending()
    {
        var results = VacationOptimizer.FindBest(2025, 5);
        for (int i = 0; i < results.Count - 1; i++)
            Assert.True(results[i].Ratio >= results[i + 1].Ratio,
                $"Résultat {i} (ratio {results[i].Ratio}) < résultat {i + 1} (ratio {results[i + 1].Ratio})");
    }

    [Fact]
    public void FindBest_NoDaysTakenExceedsMax()
    {
        const int max = 3;
        var results = VacationOptimizer.FindBest(2025, max);
        Assert.All(results, o => Assert.True(o.DaysTaken <= max));
    }

    [Fact]
    public void FindBest_RatioEqualsTotal_DividedBy_Taken()
    {
        var results = VacationOptimizer.FindBest(2025, 5);
        Assert.All(results, o =>
            Assert.Equal((double)o.TotalDaysOff / o.DaysTaken, o.Ratio, precision: 10));
    }

    [Fact]
    public void FindBest_NoWindowWithZeroDaysTaken()
    {
        var results = VacationOptimizer.FindBest(2025, 5);
        Assert.All(results, o => Assert.True(o.DaysTaken >= 1));
    }

    [Fact]
    public void FindBest_2025_BestOpportunityAroundMay1()
    {
        // 1er mai 2025 = jeudi (férié), 2 mai = vendredi (ouvré), 3-4 mai = week-end
        // Poser 1 jour (2 mai) → 4 jours off (1-4 mai) → ratio 4
        // Pâques 2025 = 20 avril (dim), Lundi de Pâques = 21 avril
        // Le week-end 19-20 + lundi férié 21 = 3 jours free
        // Si on ajoute 22 avril (mardi, ouvré) → 4 jours (19-22), ratio 4
        var results = VacationOptimizer.FindBest(2025, 1);
        Assert.True(results[0].Ratio >= 4.0,
            $"Meilleur ratio attendu >= 4, obtenu {results[0].Ratio}");
    }

    [Fact]
    public void FindBest_WindowEndIsAlwaysAfterOrEqualStart()
    {
        var results = VacationOptimizer.FindBest(2025, 10);
        Assert.All(results, o => Assert.True(o.End >= o.Start));
    }

    [Fact]
    public void FindBest_TotalDaysOffMatchesDateRange()
    {
        var results = VacationOptimizer.FindBest(2025, 5);
        Assert.All(results, o =>
            Assert.Equal(o.End.DayNumber - o.Start.DayNumber + 1, o.TotalDaysOff));
    }
}
