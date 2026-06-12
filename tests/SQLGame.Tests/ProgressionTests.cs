using System.Collections.Generic;

public class ProgressionTests
{
    [Fact]
    public void CalculateLevelXp_ReturnsBaseXp_WhenThereAreNoWrongAttempts()
    {
        int xp = XpCalculator.CalculateLevelXp(baseXp: 100, wrongAttempts: 0);

        Assert.Equal(100, xp);
    }

    [Fact]
    public void CalculateLevelXp_AppliesTenPercentPenalty_ForEachWrongAttempt()
    {
        int xp = XpCalculator.CalculateLevelXp(baseXp: 100, wrongAttempts: 2);

        Assert.Equal(80, xp);
    }

    [Fact]
    public void CalculateLevelXp_DoesNotReturnNegativeXp()
    {
        int xp = XpCalculator.CalculateLevelXp(baseXp: 100, wrongAttempts: 20);

        Assert.Equal(0, xp);
    }

    [Fact]
    public void CalculateRewardXp_ReturnsOnlyImprovementOverPreviousBest()
    {
        int reward = XpCalculator.CalculateRewardXp(
            previousBestXp: 70,
            baseXp: 100,
            wrongAttempts: 1
        );

        Assert.Equal(20, reward);
    }

    [Fact]
    public void CalculateRewardXp_UsesWrongAttempts_WhenCalculatingImprovement()
    {
        int rewardWithoutErrors = XpCalculator.CalculateRewardXp(
            previousBestXp: 70,
            baseXp: 100,
            wrongAttempts: 0
        );

        int rewardWithOneError = XpCalculator.CalculateRewardXp(
            previousBestXp: 70,
            baseXp: 100,
            wrongAttempts: 1
        );

        Assert.Equal(30, rewardWithoutErrors);
        Assert.Equal(20, rewardWithOneError);
    }

    [Fact]
    public void CalculateRewardXp_ReturnsZero_WhenResultDoesNotImprovePreviousBest()
    {
        int reward = XpCalculator.CalculateRewardXp(
            previousBestXp: 90,
            baseXp: 100,
            wrongAttempts: 3
        );

        Assert.Equal(0, reward);
    }

    [Fact]
    public void CalculateRewardXp_ReturnsReplayBonus_WhenPerfectLevelIsRepeated()
    {
        int reward = XpCalculator.CalculateRewardXp(
            previousBestXp: 100,
            baseXp: 100,
            wrongAttempts: 0
        );

        Assert.Equal(5, reward);
    }

    [Fact]
    public void CalculateRewardXp_DoesNotReturnReplayBonus_WhenRepeatedWithWrongAttempts()
    {
        int reward = XpCalculator.CalculateRewardXp(
            previousBestXp: 100,
            baseXp: 100,
            wrongAttempts: 1
        );

        Assert.Equal(0, reward);
    }

    [Fact]
    public void CalculateScore_ReturnsZero_WhenXpIsZero()
    {
        int score = LeaderboardScoreCalculator.CalculateScore(
            xp: 0,
            bestTimeToTargetTimeRatios: new[] { 0.5 }
        );

        Assert.Equal(0, score);
    }

    [Fact]
    public void CalculateScore_ReturnsXp_WhenNoValidTimeRatiosAreAvailable()
    {
        int score = LeaderboardScoreCalculator.CalculateScore(
            xp: 1000,
            bestTimeToTargetTimeRatios: new List<double>()
        );

        Assert.Equal(1000, score);
    }

    [Fact]
    public void CalculateScore_IncreasesRating_WhenBestTimeIsFasterThanTarget()
    {
        int score = LeaderboardScoreCalculator.CalculateScore(
            xp: 1000,
            bestTimeToTargetTimeRatios: new[] { 0.5 }
        );

        Assert.Equal(1175, score);
    }

    [Fact]
    public void CalculateScore_DecreasesRating_WhenBestTimeIsSlowerThanTarget()
    {
        int score = LeaderboardScoreCalculator.CalculateScore(
            xp: 1000,
            bestTimeToTargetTimeRatios: new[] { 1.5 }
        );

        Assert.Equal(825, score);
    }
}
