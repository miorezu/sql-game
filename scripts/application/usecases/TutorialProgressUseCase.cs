public static class TutorialProgressUseCase
{
    public static bool ShouldShowTutorial(string levelType, bool forceShow)
    {
        if (forceShow)
            return true;

        if (SaveManager.Instance?.Data == null)
            return false;

        return levelType switch
        {
            "table" => !SaveManager.Instance.Data.TableTutorialShown,
            "builder" => !SaveManager.Instance.Data.BuilderTutorialShown,
            "match" => !SaveManager.Instance.Data.MatchTutorialShown,
            _ => false
        };
    }

    public static void MarkTutorialShown(string levelType)
    {
        if (SaveManager.Instance?.Data == null)
            return;

        bool changed = levelType switch
        {
            "table" when !SaveManager.Instance.Data.TableTutorialShown => SaveManager.Instance.Data.TableTutorialShown = true,
            "builder" when !SaveManager.Instance.Data.BuilderTutorialShown => SaveManager.Instance.Data.BuilderTutorialShown = true,
            "match" when !SaveManager.Instance.Data.MatchTutorialShown => SaveManager.Instance.Data.MatchTutorialShown = true,
            _ => false
        };

        if (changed)
            SaveManager.Instance.Save();
    }
}
