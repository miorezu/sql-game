using System;
using Godot;

public partial class WelcomeScreen : Control
{
    [Export] private LineEdit _nameInput;
    [Export] private BaseButton _confirmButton;
    [Export] private Label _errorLabel;

    public event Action WelcomeCompleted;

    public override void _Ready()
    {
        Visible = false;
        MouseFilter = MouseFilterEnum.Stop;

        if (_confirmButton != null)
            _confirmButton.Pressed += OnConfirmPressed;

        if (_nameInput != null)
            _nameInput.TextSubmitted += _ => OnConfirmPressed();

        if (_errorLabel != null)
            _errorLabel.Visible = false;
    }

    public void TryShow()
    {
        if (SaveManager.Instance == null)
        {
            GD.PrintErr("[WELCOME] SaveManager.Instance не знайдено!");
            ShowScreen();
            return;
        }

        if (SaveManager.Instance.Data == null)
        {
            GD.PrintErr("[WELCOME] SaveData не завантажено!");
            ShowScreen();
            return;
        }

        if (SaveManager.Instance.Data.HasSeenWelcomeScreen)
        {
            Visible = false;
            return;
        }

        ShowScreen();
    }

    private void ShowScreen()
    {
        Visible = true;
        MoveToFront();

        if (_errorLabel != null)
            _errorLabel.Visible = false;

        if (_nameInput != null)
            _nameInput.GrabFocus();
    }

    private void OnConfirmPressed()
    {
        string playerName = _nameInput?.Text.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(playerName))
        {
            ShowError("Введіть ім'я користувача");
            return;
        }

        if (playerName.Length > 24)
        {
            ShowError("Ім'я не повинно перевищувати 24 символи");
            return;
        }
        if (playerName.Length < 2)
        {
            ShowError("Ім'я повинно мати хоча б 2 символи");
            return;
        }

        SaveManager.Instance.CompleteWelcomeScreen(playerName);

        Visible = false;
        WelcomeCompleted?.Invoke();
    }

    private void ShowError(string message)
    {
        if (_errorLabel == null)
            return;

        _errorLabel.Text = message;
        _errorLabel.Visible = true;
    }
}