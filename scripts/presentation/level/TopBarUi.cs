using System;
using System.Collections.Generic;
using Godot;

public partial class TopBarUi : PanelContainer
{
    public enum TopBarMode { MainMenu, Level, Profile }

    [Export] public TopBarMode Mode { get; set; } = TopBarMode.MainMenu;

    [Export] private TextureButton _homeButton;
    [Export] private Label _timerLabel;
    [Export] private TextureButton _restartButton;
    [Export] private TextureButton _profileButton;
    [Export] private TextureButton _settingsButton;
    [Export] private TextureButton _hintButton;

    public event Action HomePressed;
    public event Action RestartPressed;
    public event Action ProfilePressed;
    public event Action SettingsPressed;
    public event Action HintPressed;
    
    private static readonly Dictionary<TopBarMode, ModeConfig> ModeConfigs = new()
    {
        [TopBarMode.MainMenu] = new(
            Layout: LayoutDirectionEnum.Rtl,
            Home: false, Timer: false, Restart: false,
            Profile: true, Settings: true, Hint: false
        ),
        [TopBarMode.Level] = new(
            Layout: LayoutDirectionEnum.Ltr,
            Home: true, Timer: true, Restart: true,
            Profile: false, Settings: true, Hint: true
        ),
        [TopBarMode.Profile] = new(
            Layout: LayoutDirectionEnum.Ltr,
            Home: true, Timer: false, Restart: false,
            Profile: false, Settings: false, Hint: false
        ),
    };

    public override void _Ready()
    {
        ConnectButtons();
        ApplyMode();
    }

    public void SetMode(TopBarMode mode)
    {
        Mode = mode;
        ApplyMode();
    }

    public void ApplyMode()
    {
        if (!ModeConfigs.TryGetValue(Mode, out var cfg)) return;

        LayoutDirection = cfg.Layout;

        _homeButton?.SetVisibility(cfg.Home);
        _timerLabel?.SetVisibility(cfg.Timer);
        _restartButton?.SetVisibility(cfg.Restart);
        _profileButton?.SetVisibility(cfg.Profile);
        _settingsButton?.SetVisibility(cfg.Settings);
        _hintButton?.SetVisibility(cfg.Hint);
    }

    private void ConnectButtons()
    {
        _homeButton?.Connect(() => HomePressed?.Invoke());
        _restartButton?.Connect(() => RestartPressed?.Invoke());
        _profileButton?.Connect(() => ProfilePressed?.Invoke());
        _settingsButton?.Connect(() => SettingsPressed?.Invoke());
        _hintButton?.Connect(() => HintPressed?.Invoke());
    }

    public void SetTime(float seconds)
    {
        if (_timerLabel == null) return;

        int totalSeconds = (int)seconds;
        _timerLabel.Text = $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }

    private readonly record struct ModeConfig(
        LayoutDirectionEnum Layout,
        bool Home, bool Timer, bool Restart,
        bool Profile, bool Settings, bool Hint
    );
}

public static class GodotUiExtensions
{
    public static void SetVisibility(this CanvasItem node, bool visible) =>
        node.Visible = visible;

    public static void Connect(this TextureButton button, Action handler) =>
        button.Pressed += handler;
}