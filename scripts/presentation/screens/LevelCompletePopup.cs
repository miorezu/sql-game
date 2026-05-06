using System;
using Godot;



public partial class LevelCompletePopup : Control
{
    [Export] private Label _titleLabel;
    
    [Export] private TextureButton _selectLevelButton;
    [Export] private TextureButton _nextLevelButton;

    public event  Action NextLevelPressed;
    public event  Action SelectLevelPressed;
    
    public override void _Ready()
    {
        Visible = false;
        
        if (_nextLevelButton != null)
            _nextLevelButton.Pressed += OnNextLevelPressed;
        
        if (_selectLevelButton != null)
            _selectLevelButton.Pressed += OnSelectLevelPressed;
    }

    public void ShowPopup()
    {
        Visible = true;
        MoveToFront();
    }

    private void OnNextLevelPressed()
    {
        Visible = false;
        GD.Print("[POPUP] Натиснули Next");
        NextLevelPressed?.Invoke();
    }

    private void OnSelectLevelPressed()
    {
        Visible = false;
        GD.Print("[POPUP] Натиснули Select Level(menu)");
        SelectLevelPressed?.Invoke();
    }
}