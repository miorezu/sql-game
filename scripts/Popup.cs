using Godot;
using System;

public partial class Popup : Control
{
    [Export] private Label _titleLabel;
    
    [Export] private TextureButton _allLevelsButton;
    [Export] private TextureButton _nextLevelButton;

    public event  Action NextLevelPressed;
    
    public override void _Ready()
    {
        Visible = false;
        
        if (_nextLevelButton != null)
            _nextLevelButton.Pressed += OnNextLevelPressed;
    }

    public void ShowPopup()
    {
        Visible = true;
    }

    private void OnNextLevelPressed()
    {
        Visible = false;
        GD.Print("[POPUP] Натиснули Next");
        NextLevelPressed?.Invoke();
    }
}
