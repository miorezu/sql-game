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
        if (!WelcomeUseCase.ShouldShowWelcomeScreen())
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

        if (!WelcomeUseCase.TryCompleteWelcomeScreen(playerName, out string errorMessage))
        {
            ShowError(errorMessage);
            return;
        }

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
