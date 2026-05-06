using System;
using Godot;

public partial class TopBarUi : PanelContainer
{
	public enum TopBarMode
	{
		MainMenu,
		Level
	}

	[Export] public TopBarMode Mode { get; set; } = TopBarMode.MainMenu;

	[Export] private TextureButton _homeButton;
	//[Export] private Label _titleLabel;
	[Export] private Label _timerLabel;
	[Export] private TextureButton _restartButton;
	[Export] private TextureButton _profileButton;
	[Export] private TextureButton _settingsButton;

	public event  Action HomePressed;
	public event  Action RestartPressed;
	public event  Action ProfilePressed;
	public event  Action SettingsPressed;

	public override void _Ready()
	{
		ApplyMode();
		ConnectButtons();
	}
	
	public void SetMode(TopBarMode mode)
	{
		Mode = mode;
		ApplyMode();
	}
	
	public void ApplyMode()
	{
		bool isLevel = Mode == TopBarMode.Level;
		_homeButton.Visible = isLevel;
		_timerLabel.Visible = isLevel;
		_restartButton.Visible = isLevel;

		_profileButton.Visible = !isLevel;
		_settingsButton.Visible = true;
		if (!isLevel)
		{
			LayoutDirection = LayoutDirectionEnum.Rtl;
		}
	}
	
	private void ConnectButtons()
	{
		if (_homeButton != null)
			_homeButton.Pressed += () => HomePressed?.Invoke();

		if (_restartButton != null)
			_restartButton.Pressed += () => RestartPressed?.Invoke();

		if (_profileButton != null)
			_profileButton.Pressed += () => ProfilePressed?.Invoke();

		if (_settingsButton != null)
			_settingsButton.Pressed += () => SettingsPressed?.Invoke();
	}
	
	public void SetTime(float seconds)
	{
		int minutes = (int)seconds / 60;
		int secs = (int)seconds % 60;

		_timerLabel.Text = $"{minutes:00}:{secs:00}";
	}
}
