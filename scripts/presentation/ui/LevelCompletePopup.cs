using System;
using Godot;

public partial class LevelCompletePopup : Control
{
    [Export] private Label _titleLabel;
    [Export] private  Label _xpLabel;
    [Export] private  Label _timeLabel;
    [Export] private  Label _errorsLabel;

    [Export] private TextureButton _selectLevelButton;
    [Export] private TextureButton _nextLevelButton;

    public event Action NextLevelPressed;
    public event Action SelectLevelPressed;

    private bool _isButtonPressed;

    public override void _Ready()
    {
        Visible = false;

        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Stop;

        if (_nextLevelButton != null)
            _nextLevelButton.Pressed += OnNextLevelPressed;

        if (_selectLevelButton != null)
            _selectLevelButton.Pressed += OnSelectLevelPressed;
    }

    public void ShowPopup(LevelCompleteResult result)
    {
        _isButtonPressed = false;
        SetButtonsDisabled(false);

        _xpLabel.Text = $"Отримано XP: {result.RewardXp}";
        _timeLabel.Text = $"Пройден за: {SaveManager.FormatTime(result.ElapsedSeconds)}";
        _errorsLabel.Text = $"Зроблено помилок: {result.WrongAttempts}";

        Visible = true;
        MoveToFront();

        if (_nextLevelButton != null)
            _nextLevelButton.Visible = result.HasNextLevel;
    }


    private void OnNextLevelPressed()
    {
        if (_isButtonPressed)
            return;

        _isButtonPressed = true;
        SetButtonsDisabled(true);

        Visible = false;
        GetTree().Paused = false;

        GD.Print("[POPUP] Натиснули Next");

        NextLevelPressed?.Invoke();
    }

    private void OnSelectLevelPressed()
    {
        if (_isButtonPressed)
            return;

        _isButtonPressed = true;
        SetButtonsDisabled(true);

        Visible = false;
        GetTree().Paused = false;

        GD.Print("[POPUP] Натиснули Select Level(menu)");

        SelectLevelPressed?.Invoke();
    }

    private void SetButtonsDisabled(bool disabled)
    {
        if (_nextLevelButton != null)
            _nextLevelButton.Disabled = disabled;

        if (_selectLevelButton != null)
            _selectLevelButton.Disabled = disabled;
    }
}