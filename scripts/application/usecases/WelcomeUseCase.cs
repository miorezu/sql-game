public static class WelcomeUseCase
{
    public static bool ShouldShowWelcomeScreen()
    {
        if (SaveManager.Instance?.Data == null)
            return true;

        return !SaveManager.Instance.Data.HasSeenWelcomeScreen;
    }

    public static bool TryCompleteWelcomeScreen(string playerName, out string errorMessage)
    {
        playerName = playerName?.Trim() ?? "";
        errorMessage = "";

        if (string.IsNullOrWhiteSpace(playerName))
        {
            errorMessage = "Введіть ім'я користувача";
            return false;
        }

        if (playerName.Length > 24)
        {
            errorMessage = "Ім'я не повинно перевищувати 24 символи";
            return false;
        }

        if (playerName.Length < 2)
        {
            errorMessage = "Ім'я повинно мати хоча б 2 символи";
            return false;
        }

        SaveManager.Instance.CompleteWelcomeScreen(playerName);
        return true;
    }
}
