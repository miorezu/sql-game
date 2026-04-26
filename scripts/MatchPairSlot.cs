using Godot;

public partial class MatchPairSlot : PanelContainer
{
	[Export] private MatchDropArea _leftDropArea;
	[Export] private MatchDropArea _rightDropArea;

	public void CheckPair()
	{
		if (_leftDropArea == null || _rightDropArea == null)
		{
			GD.PrintErr("[MatchPairSlot] DropArea не задані в Inspector.");
			return;
		}

		var leftBlock = _leftDropArea.GetBlock();
		var rightBlock = _rightDropArea.GetBlock();

		if (leftBlock == null || rightBlock == null)
			return;

		if (leftBlock.PairId == rightBlock.PairId)
		{
			GD.Print("[MATCH] Правильна пара");
			SelfModulate = Colors.LightGreen;

			leftBlock.Disabled = true;
			rightBlock.Disabled = true;
		}
		else
		{
			GD.Print("[MATCH] Неправильна пара");
			SelfModulate = Colors.IndianRed;
		}
	}
	
	public SqlBlock GetLeftBlock()
	{
		return _leftDropArea.GetBlock();
	}

	public SqlBlock GetRightBlock()
	{
		return _rightDropArea.GetBlock();
	}
	
}
