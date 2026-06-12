using System;
using System.Collections.Generic;
using System.Linq;

public static class LeaderboardScoreCalculator
{
    private const double TimeImpactAlpha = 0.35;

    public static int CalculateScore(int xp, IEnumerable<double> bestTimeToTargetTimeRatios)
    {
        if (xp <= 0)
            return 0;

        double timeFactor = CalculateTimeFactor(bestTimeToTargetTimeRatios);
        double score = xp * (1.0 + TimeImpactAlpha * timeFactor);

        return Math.Max(0, (int)Math.Round(score));
    }

    private static double CalculateTimeFactor(IEnumerable<double> bestTimeToTargetTimeRatios)
    {
        if (bestTimeToTargetTimeRatios == null)
            return 0;

        List<double> ratios = bestTimeToTargetTimeRatios
            .Where(ratio => ratio > 0 && !double.IsNaN(ratio) && !double.IsInfinity(ratio))
            .ToList();

        if (ratios.Count == 0)
            return 0;

        double averageRatio = ratios.Average();

        return Math.Clamp(1.0 - averageRatio, -1.0, 1.0);
    }
}
