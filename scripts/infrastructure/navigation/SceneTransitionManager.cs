using System;
using Godot;
using System.Threading.Tasks;



public partial class SceneTransitionManager : CanvasLayer
{
    public static SceneTransitionManager Instance { get; private set; }

    [Export] private ColorRect _fadeRect;
    [Export] private Label _loadingLabel;

    [Export] private float _fadeDuration = 0.5f;

    private bool _isTransitioning;

    public override void _Ready()
    {
        Instance = this;
        Layer = 100;
        if (_fadeRect == null || _loadingLabel == null)
        {
            GD.PrintErr("[SceneTransition] FadeRect або LoadingLabel не ініціалізовані.");
            return;
        }

        _fadeRect.Visible = false;
        _fadeRect.Modulate = new Color(1, 1, 1, 0);
        _loadingLabel.Visible = false;
    }

    public async void FadeWithChangeScene(string scenePath, bool showLoadingText = true)
    {
        if (_isTransitioning)
            return;

        if (_fadeRect == null || _loadingLabel == null)
        {
            GD.PrintErr("[SceneTransition] FadeRect або LoadingLabel не ініціалізовані.");

            var tree = Engine.GetMainLoop() as SceneTree;
            tree?.ChangeSceneToFile(scenePath);
            return;
        }

        _isTransitioning = true;

        _fadeRect.Visible = true;
        _loadingLabel.Visible = true;

        await FadeTo(1f);

        if (showLoadingText)
        {
            _loadingLabel.Text = "Loading...";
            _loadingLabel.Visible = true;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        Error error = GetTree().ChangeSceneToFile(scenePath);

        if (error != Error.Ok)
        {
            GD.PrintErr($"[SceneTransition] Failed to load scene: {scenePath}. Error: {error}");
            _loadingLabel.Visible = false;
            await FadeTo(0f);
            _fadeRect.Visible = false;
            _isTransitioning = false;
            return;
        }

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        _loadingLabel.Visible = false;

        await FadeTo(0f);

        _fadeRect.Visible = false;
        _isTransitioning = false;
    }

    public async Task FadeWithoutChangeScene(Func<Task> action, bool showLoadingText = true)
    {
        if (_isTransitioning)
            return;

        if (_fadeRect == null || _loadingLabel == null)
        {
            GD.PrintErr("[SceneTransition] FadeRect або LoadingLabel не ініціалізовані.");
            await action();
            return;
        }

        _isTransitioning = true;

        _fadeRect.Visible = true;
        _fadeRect.Modulate = new Color(1, 1, 1, 0);
        _loadingLabel.Visible = false;

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        await FadeTo(1f);

        if (showLoadingText)
        {
            _loadingLabel.Text = "Loading...";
            _loadingLabel.Visible = true;
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        await action();

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        _loadingLabel.Visible = false;

        await FadeTo(0f);

        _fadeRect.Visible = false;
        _isTransitioning = false;
    }
    
    private async Task FadeTo(float targetAlpha)
    {
        Tween tween = CreateTween();

        tween.TweenProperty(
            _fadeRect,
            "modulate:a",
            targetAlpha,
            _fadeDuration
        );

        await ToSignal(tween, Tween.SignalName.Finished);
    }
}