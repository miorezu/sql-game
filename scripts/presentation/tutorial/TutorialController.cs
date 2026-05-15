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

        if (SaveManager.Instance?.Data == null)
        {
            GD.PrintErr("[Tutorial] SaveManager або SaveData не ініціалізовані.");
            return false;
        }

        _isTutorialRunning = true;

        try
        {
            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            var tutorialWasShown = false;
            var shouldSave = false;

            switch (levelData.LevelType)
            {
                case "table":
                    if (!forceShow && SaveManager.Instance.Data.TableTutorialShown)
                        return false;

                    tutorialWasShown = await ShowTableTutorial(levelView);

                    if (tutorialWasShown && !SaveManager.Instance.Data.TableTutorialShown)
                    {
                        SaveManager.Instance.Data.TableTutorialShown = true;
                        shouldSave = true;
                    }

                    break;

                case "builder":
                    if (!forceShow && SaveManager.Instance.Data.BuilderTutorialShown)
                        return false;

                    tutorialWasShown = await ShowBuilderTutorial(levelView);

                    if (tutorialWasShown && !SaveManager.Instance.Data.BuilderTutorialShown)
                    {
                        SaveManager.Instance.Data.BuilderTutorialShown = true;
                        shouldSave = true;
                    }

                    break;

                case "match":
                    if (!forceShow && SaveManager.Instance.Data.MatchTutorialShown)
                        return false;

                    tutorialWasShown = await ShowMatchTutorial(levelView);

                    if (tutorialWasShown && !SaveManager.Instance.Data.MatchTutorialShown)
                    {
                        SaveManager.Instance.Data.MatchTutorialShown = true;
                        shouldSave = true;
                    }

                    break;

                default:
                    GD.PrintErr($"[Tutorial] Невідомий тип рівня: {levelData.LevelType}");
                    return false;
            }

            if (shouldSave)
                SaveManager.Instance.Save();

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

        if (leftContainer.GetChildCount() == 0 || rightContainer.GetChildCount() == 0)
        {
            GD.PrintErr("[Tutorial] Match tutorial: немає блоків для демонстрації.");
            return false;
        }

        var leftBlock = leftContainer.GetChildOrNull<Control>(0);
        var rightBlock = rightContainer.GetChildOrNull<Control>(0);

        if (leftBlock == null || rightBlock == null)
        {
            GD.PrintErr("[Tutorial] Match tutorial: лівий або правий блок не є Control.");
            return false;
        }

        return await _tutorialOverlay.ShowMatchTutorial(
            leftBlock,
            rightBlock,
            targetSlot
        );
    }
}