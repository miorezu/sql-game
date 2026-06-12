using System;

public static class TimeFormatter
{
    public static string Format(double totalSeconds)
    {
        if (totalSeconds <= 0)
            return "--:--";

        var time = TimeSpan.FromSeconds(totalSeconds);

        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";

        return $"{time.Minutes:00}:{time.Seconds:00}";
    }
}
