using System.Threading.Tasks;
using Godot;

public partial class TutorialController : Node
{
    [Export] private TutorialOverlay _tutorialOverlay;
    private bool _isTutorialRunning;

    public async Task<bool> ShowForLevel(
        LevelData levelData,
        Control levelView,
        bool forceShow = false
    )
    {
        if (_isTutorialRunning)
            return false;

        if (_tutorialOverlay == null)
        {
            GD.PrintErr("[Tutorial] TutorialOverlay не заданий.");
            return false;
        }

        if (levelData == null)
        {
            GD.PrintErr("[Tutorial] LevelData is null.");
            return false;
        }

        if (levelView == null)
        {
            GD.PrintErr("[Tutorial] LevelView is null.");
            return false;
        }

        if (!TutorialProgressUseCase.ShouldShowTutorial(levelData.LevelType, forceShow))
            return false;

        _isTutorialRunning = true;

        try
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            bool tutorialWasShown;

            switch (levelData.LevelType)
            {
                case "table":
                    tutorialWasShown = await ShowTableTutorial(levelView);
                    break;

                case "builder":
                    tutorialWasShown = await ShowBuilderTutorial(levelView);
                    break;

                case "match":
                    tutorialWasShown = await ShowMatchTutorial(levelView);
                    break;

                default:
                    GD.PrintErr($"[Tutorial] Невідомий тип рівня: {levelData.LevelType}");
                    return false;
            }

            if (tutorialWasShown && !forceShow)
                TutorialProgressUseCase.MarkTutorialShown(levelData.LevelType);

            return tutorialWasShown;
        }
        finally
        {
            _isTutorialRunning = false;
        }
    }

    private async Task<bool> ShowTableTutorial(Control levelView)
    {
        if (levelView is not TableLevel tableLevel)
        {
            GD.PrintErr("[Tutorial] Поточний рівень не є TableLevel.");
            return false;
        }

        var blocksContainer = tableLevel.BlocksContainer;
        var targetArea = tableLevel.QueryDropArea;

        if (blocksContainer == null || targetArea == null)
        {
            GD.PrintErr("[Tutorial] Table tutorial: BlocksContainer або QueryDropArea не задані.");
            return false;
        }

        if (blocksContainer.GetChildCount() == 0)
        {
            GD.PrintErr("[Tutorial] Table tutorial: немає блоків для демонстрації.");
            return false;
        }

        var firstBlock = blocksContainer.GetChildOrNull<Control>(0);

        if (firstBlock == null)
        {
            GD.PrintErr("[Tutorial] Table tutorial: перший блок не є Control.");
            return false;
        }

        await _tutorialOverlay.ShowTableTutorial(firstBlock, targetArea);

        return true;
    }

    private async Task<bool> ShowBuilderTutorial(Control levelView)
    {
        if (levelView is not BuilderLevel builderLevel)
        {
            GD.PrintErr("[Tutorial] Поточний рівень не є BuilderLevel.");
            return false;
        }

        var blocksContainer = builderLevel.BlocksContainer;
        var queryBuilder = builderLevel.QueryBuilder;

        if (blocksContainer == null || queryBuilder == null)
        {
            GD.PrintErr("[Tutorial] Builder tutorial: BlocksContainer або QueryBuilder не задані.");
            return false;
        }

        if (blocksContainer.GetChildCount() == 0)
        {
            GD.PrintErr("[Tutorial] Builder tutorial: немає блоків для демонстрації.");
            return false;
        }

        var firstBlock = blocksContainer.GetChildOrNull<Control>(0);

        if (firstBlock == null)
        {
            GD.PrintErr("[Tutorial] Builder tutorial: перший блок не є Control.");
            return false;
        }

        await _tutorialOverlay.ShowBuilderTutorial(firstBlock, queryBuilder);

        return true;
    }

    private async Task<bool> ShowMatchTutorial(Control levelView)
    {
        if (levelView is not MatchLevel matchLevel)
        {
            GD.PrintErr("[Tutorial] Поточний рівень не є MatchLevel.");
            return false;
        }

        var leftContainer = matchLevel.LeftBlocksContainer;
        var rightContainer = matchLevel.RightBlocksContainer;
        var targetSlot = matchLevel.TargetSlot;

        if (leftContainer == null || rightContainer == null)
        {
            GD.PrintErr("[Tutorial] Match tutorial: LeftBlocksContainer або RightBlocksContainer не задані.");
            return false;
        }

        if (targetSlot == null)
        {
            GD.PrintErr("[Tutorial] Match tutorial: TargetSlot не заданий.");
            return false;
        }

        var leftBlock = GetFirstDirectControl(leftContainer)
                        ?? FindMatchBlockInTree(targetSlot, MatchSide.Left);

        var rightBlock = GetFirstDirectControl(rightContainer)
                         ?? FindMatchBlockInTree(targetSlot, MatchSide.Right);

        if (leftBlock == null || rightBlock == null)
        {
            GD.PrintErr("[Tutorial] Match tutorial: немає лівого або правого блока для демонстрації.");
            return false;
        }

        return await _tutorialOverlay.ShowMatchTutorial(
            leftBlock,
            rightBlock,
            targetSlot
        );
    }

    private Control GetFirstDirectControl(Control container)
    {
        if (container == null)
            return null;

        foreach (Node child in container.GetChildren())
        {
            if (child is Control control)
                return control;
        }

        return null;
    }

    private SqlBlock FindMatchBlockInTree(Control root, MatchSide matchSide)
    {
        if (root == null)
            return null;

        foreach (Node child in root.GetChildren())
        {
            if (child is SqlBlock block && block.MatchSide == matchSide)
                return block;

            if (child is Control control)
            {
                var nestedBlock = FindMatchBlockInTree(control, matchSide);

                if (nestedBlock != null)
                    return nestedBlock;
            }
        }

        return null;
    }
}
