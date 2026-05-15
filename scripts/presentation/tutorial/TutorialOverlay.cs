using Godot;
using System.Threading.Tasks;

public partial class TutorialOverlay : CanvasLayer
{
    [Export] private ColorRect _darkBackground;

    [Export] private SqlBlock _ghostBlock;
    [Export] private SqlBlock _ghostLeftBlock;
    [Export] private SqlBlock _ghostRightBlock;

    [Export] private Label _hintLabel;

    [Export] private float _moveDuration = 1.8f;
    [Export] private float _returnDuration = 0.35f;
    [Export] private float _pauseDuration = 0.7f;

    private Tween _tween;
    private TaskCompletionSource<bool> _hideCompletionSource;

    public override void _Ready()
    {
        HideTutorial();
    }

    public async Task ShowTableTutorial(Control fromBlock, Control targetArea)
    {
        await ShowDragTutorial(
            fromBlock,
            targetArea,
            _ghostBlock,
            "UPDATE",
            "Перетягни SQL-запит у поле виконання. Твоя мета — змінити ліву таблицю так, щоб вона стала як права."
        );
    }

    public async Task ShowBuilderTutorial(Control fromBlock, Control queryBuilder)
    {
        await ShowDragTutorial(
            fromBlock,
            queryBuilder,
            _ghostBlock,
            "SELECT",
            "Перетягни блоки в поле запиту так, щоб сформувати правильний SQL-запит.\n" +
            "Для перевірки натисни кнопку.\n\n" +
            "Зелений блок — правильний.\n" +
            "Жовтий блок — правильний, але не на своєму місці.\n" +
            "Червоний блок — неправильний."
        );
    }

    public async Task<bool> ShowMatchTutorial(
        Control leftBlock,
        Control rightBlock,
        Control targetSlot
    )
    {
        return await ShowTwoBlocksMatchTutorial(
            leftBlock,
            rightBlock,
            targetSlot,
            "SELECT",
            "Вибірка",
            "Перетягни блок зліва та відповідний блок справа у центральний слот.\n" +
            "Після натискання кнопки правильна пара стане зеленою, неправильна — червоною."
        );
    }

    private async Task<bool> ShowTwoBlocksMatchTutorial(
        Control leftBlock,
        Control rightBlock,
        Control targetSlot,
        string leftText,
        string rightText,
        string hintText
    )
    {
        if (!IsValidControl(leftBlock))
        {
            GD.PrintErr("[TutorialOverlay] leftBlock невалідний або вже видалений.");
            return false;
        }

        if (!IsValidControl(rightBlock))
        {
            GD.PrintErr("[TutorialOverlay] rightBlock невалідний або вже видалений.");
            return false;
        }

        if (!IsValidControl(targetSlot))
        {
            GD.PrintErr("[TutorialOverlay] targetSlot невалідний або вже видалений.");
            return false;
        }

        if (_ghostLeftBlock == null || _ghostRightBlock == null)
        {
            GD.PrintErr("[TutorialOverlay] Ghost-блоки для match tutorial не прив'язані.");
            return false;
        }

        ShowBaseOverlay(hintText);

        SetupGhostBlock(_ghostLeftBlock, leftText);
        SetupGhostBlock(_ghostRightBlock, rightText);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (!IsValidControl(leftBlock) ||
            !IsValidControl(rightBlock) ||
            !IsValidControl(targetSlot))
        {
            GD.PrintErr("[TutorialOverlay] Один із блоків або TargetSlot був видалений після ProcessFrame.");
            HideTutorial();
            return false;
        }

        Vector2 leftStart = GetCenteredPosition(leftBlock, _ghostLeftBlock);
        Vector2 rightStart = GetCenteredPosition(rightBlock, _ghostRightBlock);

        Vector2 targetCenter = targetSlot.GlobalPosition + targetSlot.Size / 2.0f;

        Vector2 leftEnd = targetCenter
                          - _ghostLeftBlock.Size / 2.0f
                          + new Vector2(-_ghostLeftBlock.Size.X / 2.0f - 8.0f, 0);

        Vector2 rightEnd = targetCenter
                           - _ghostRightBlock.Size / 2.0f
                           + new Vector2(_ghostRightBlock.Size.X / 2.0f + 8.0f, 0);

        await AnimateTwoGhostBlocks(
            leftStart,
            rightStart,
            leftEnd,
            rightEnd
        );

        return true;
    }

    private async Task ShowDragTutorial(
        Control fromBlock,
        Control target,
        SqlBlock ghostBlock,
        string blockText,
        string hintText
    )
    {
        if (!IsValidControl(fromBlock) || !IsValidControl(target) || ghostBlock == null)
        {
            GD.PrintErr("[TutorialOverlay] ShowDragTutorial: fromBlock, target або ghostBlock невалідний.");
            return;
        }

        ShowBaseOverlay(hintText);

        SetupGhostBlock(ghostBlock, blockText);

        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (!IsValidControl(fromBlock) || !IsValidControl(target))
        {
            GD.PrintErr("[TutorialOverlay] fromBlock або target був видалений після ProcessFrame.");
            HideTutorial();
            return;
        }

        Vector2 startPosition = GetCenteredPosition(fromBlock, ghostBlock);
        Vector2 endPosition = GetCenteredPosition(target, ghostBlock);

        await AnimateOneGhostBlock(ghostBlock, startPosition, endPosition);
    }

    private bool IsValidControl(Control control)
    {
        return control != null
               && GodotObject.IsInstanceValid(control)
               && control.IsInsideTree();
    }

    private void ShowBaseOverlay(string hintText)
    {
        _hideCompletionSource = new TaskCompletionSource<bool>();

        Visible = true;

        if (_darkBackground != null)
        {
            _darkBackground.Visible = true;
            _darkBackground.ZIndex = 0;

            // Краще Stop, щоб кліки не проходили на блоки під tutorial.
            _darkBackground.MouseFilter = Control.MouseFilterEnum.Stop;
        }

        if (_ghostBlock != null)
        {
            _ghostBlock.Visible = false;
            _ghostBlock.ZIndex = 10;
        }

        if (_ghostLeftBlock != null)
        {
            _ghostLeftBlock.Visible = false;
            _ghostLeftBlock.ZIndex = 10;
        }

        if (_ghostRightBlock != null)
        {
            _ghostRightBlock.Visible = false;
            _ghostRightBlock.ZIndex = 10;
        }

        if (_hintLabel != null)
        {
            _hintLabel.Visible = true;
            _hintLabel.Text = hintText;
            _hintLabel.Modulate = new Color(1, 1, 1, 1);
            _hintLabel.ZIndex = 20;
        }
    }

    private void SetupGhostBlock(SqlBlock block, string text)
    {
        block.Visible = true;
        block.BlockValue = text;
        block.Text = text;

        block.Disabled = false;
        block.MouseFilter = Control.MouseFilterEnum.Ignore;

        block.Modulate = new Color(1, 1, 1, 1f);
        block.SelfModulate = new Color(1, 1, 1, 1f);

        block.ZIndex = 10;
    }

    private Vector2 GetCenteredPosition(Control target, Control ghostBlock)
    {
        return target.GlobalPosition + target.Size / 2.0f - ghostBlock.Size / 2.0f;
    }

    private async Task AnimateOneGhostBlock(
        Control ghostBlock,
        Vector2 startPosition,
        Vector2 endPosition
    )
    {
        _tween?.Kill();

        ghostBlock.GlobalPosition = startPosition;

        _tween = CreateTween();
        _tween.SetLoops();

        _tween.TweenProperty(ghostBlock, "global_position", endPosition, _moveDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _tween.TweenInterval(_pauseDuration);

        _tween.TweenProperty(ghostBlock, "global_position", startPosition, _returnDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        await WaitUntilTutorialClosed();
    }

    private async Task AnimateTwoGhostBlocks(
        Vector2 leftStart,
        Vector2 rightStart,
        Vector2 leftEnd,
        Vector2 rightEnd
    )
    {
        _tween?.Kill();

        _ghostLeftBlock.GlobalPosition = leftStart;
        _ghostRightBlock.GlobalPosition = rightStart;

        _tween = CreateTween();
        _tween.SetLoops();

        _tween.SetParallel(true);

        _tween.TweenProperty(_ghostLeftBlock, "global_position", leftEnd, _moveDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _tween.TweenProperty(_ghostRightBlock, "global_position", rightEnd, _moveDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _tween.SetParallel(false);
        _tween.TweenInterval(_pauseDuration);

        _tween.SetParallel(true);

        _tween.TweenProperty(_ghostLeftBlock, "global_position", leftStart, _returnDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        _tween.TweenProperty(_ghostRightBlock, "global_position", rightStart, _returnDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        await WaitUntilTutorialClosed();
    }

    private async Task WaitUntilTutorialClosed()
    {
        if (_hideCompletionSource == null)
            return;

        await _hideCompletionSource.Task;
    }

    public void HideTutorial()
    {
        _tween?.Kill();
        _tween = null;

        Visible = false;

        if (_darkBackground != null)
            _darkBackground.Visible = false;

        if (_ghostBlock != null)
            _ghostBlock.Visible = false;

        if (_ghostLeftBlock != null)
            _ghostLeftBlock.Visible = false;

        if (_ghostRightBlock != null)
            _ghostRightBlock.Visible = false;

        if (_hintLabel != null)
            _hintLabel.Visible = false;

        _hideCompletionSource?.TrySetResult(true);
    }

    public override void _Input(InputEvent @event)
    {
        if (!Visible)
            return;

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            HideTutorial();
            GetViewport().SetInputAsHandled();
        }
    }
}