using Godot;
using System;

public partial class SqlBlock : Button
{
	
	[Export] private string SQLText = "SELECT";

	public override void _Ready()
	{
		Text = SQLText;
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		Label preview = new Label();
		preview.Text = SQLText;

		SetDragPreview(preview);

		return SQLText;
	}


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
