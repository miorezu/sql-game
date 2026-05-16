public class LevelCompleteResult
{
	public LevelData LevelData { get; set; }
	public int CalculatedXp { get; set; }
	public int RewardXp { get; set; }
	public float ElapsedSeconds { get; set; }
	public int WrongAttempts { get; set; }
	public bool HasNextLevel { get; set; }
}
