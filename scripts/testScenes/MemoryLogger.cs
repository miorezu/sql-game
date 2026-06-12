using Godot;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

public partial class MemoryLogger : Node
{
    public static MemoryLogger Instance { get; private set; }

    [Export] public bool Enabled { get; set; } = true;
    [Export] public float LogIntervalSeconds { get; set; } = 1.0f;
    [Export] public bool PrintLogPathOnStart { get; set; } = true;

    private const string LogDirectory = "user://performance_logs";

    private readonly CultureInfo _culture = CultureInfo.InvariantCulture;
    private readonly Process _process = Process.GetCurrentProcess();

    private string _csvPath;
    private string _metricsCsvPath;
    private float _elapsedSinceLastLog;
    private int _sampleIndex;
    private int _metricIndex;

    public override void _Ready()
    {
        Instance = this;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", _culture);
        string godotDirectory = LogDirectory;
        string globalDirectory = ProjectSettings.GlobalizePath(godotDirectory);

        Directory.CreateDirectory(globalDirectory);

        _csvPath = Path.Combine(globalDirectory, $"memory_log_{timestamp}.csv");
        _metricsCsvPath = Path.Combine(globalDirectory, $"custom_metrics_{timestamp}.csv");

        File.WriteAllText(
            _csvPath,
            "sample,time_sec,scene,fps,godot_static_memory_mb,process_working_set_mb,managed_heap_mb,node_count,object_count,resource_count,orphan_node_count\n"
        );

        File.WriteAllText(
            _metricsCsvPath,
            "metric,time_sec,event_name,current_scene,from_scene,to_scene,scene_path,level_order,level_type,level_title,duration_ms,details\n"
        );

        if (PrintLogPathOnStart)
        {
            GD.Print($"[MemoryLogger] CSV log path: {_csvPath}");
            GD.Print($"[MemoryLogger] Custom metrics path: {_metricsCsvPath}");
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    public override void _Process(double delta)
    {
        if (!Enabled)
            return;

        _elapsedSinceLastLog += (float)delta;

        if (_elapsedSinceLastLog < LogIntervalSeconds)
            return;

        _elapsedSinceLastLog = 0f;
        WriteSample();
    }

    public void WriteSample()
    {
        _sampleIndex++;
        _process.Refresh();

        double timeSec = Time.GetTicksMsec() / 1000.0;
        string sceneName = GetTree().CurrentScene?.Name ?? "Unknown";

        double fps = Performance.GetMonitor(Performance.Monitor.TimeFps);
        double godotStaticMemoryMb = BytesToMegabytes(Performance.GetMonitor(Performance.Monitor.MemoryStatic));
        double processWorkingSetMb = BytesToMegabytes(_process.WorkingSet64);
        double managedHeapMb = BytesToMegabytes(GC.GetTotalMemory(forceFullCollection: false));

        double nodeCount = Performance.GetMonitor(Performance.Monitor.ObjectNodeCount);
        double objectCount = Performance.GetMonitor(Performance.Monitor.ObjectCount);
        double resourceCount = Performance.GetMonitor(Performance.Monitor.ObjectResourceCount);
        double orphanNodeCount = Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount);

        string line = string.Join(
            ",",
            _sampleIndex.ToString(_culture),
            timeSec.ToString("F2", _culture),
            EscapeCsv(sceneName),
            fps.ToString("F0", _culture),
            godotStaticMemoryMb.ToString("F2", _culture),
            processWorkingSetMb.ToString("F2", _culture),
            managedHeapMb.ToString("F2", _culture),
            nodeCount.ToString("F0", _culture),
            objectCount.ToString("F0", _culture),
            resourceCount.ToString("F0", _culture),
            orphanNodeCount.ToString("F0", _culture)
        );

        File.AppendAllText(_csvPath, line + System.Environment.NewLine);
    }

    public void RecordSceneTransition(string fromScene, string toScene, string scenePath, double durationMs)
    {
        WriteMetric(
            eventName: "scene_transition_time_ms",
            fromScene: fromScene,
            toScene: toScene,
            scenePath: scenePath,
            levelOrder: string.Empty,
            levelType: string.Empty,
            levelTitle: string.Empty,
            durationMs: durationMs,
            details: string.Empty
        );
    }

    public void RecordLevelLoad(LevelData levelData, double durationMs)
    {
        string levelOrder = levelData?.LevelOrder.ToString(_culture) ?? string.Empty;
        string levelType = levelData?.LevelType ?? string.Empty;
        string levelTitle = levelData?.Title ?? string.Empty;

        WriteMetric(
            eventName: "level_load_time_ms",
            fromScene: string.Empty,
            toScene: string.Empty,
            scenePath: string.Empty,
            levelOrder: levelOrder,
            levelType: levelType,
            levelTitle: levelTitle,
            durationMs: durationMs,
            details: string.Empty
        );
    }

    public void RecordCustomMetric(string eventName, double durationMs, string details = "")
    {
        WriteMetric(
            eventName: eventName,
            fromScene: string.Empty,
            toScene: string.Empty,
            scenePath: string.Empty,
            levelOrder: string.Empty,
            levelType: string.Empty,
            levelTitle: string.Empty,
            durationMs: durationMs,
            details: details
        );
    }

    private void WriteMetric(
        string eventName,
        string fromScene,
        string toScene,
        string scenePath,
        string levelOrder,
        string levelType,
        string levelTitle,
        double durationMs,
        string details)
    {
        if (!Enabled || string.IsNullOrEmpty(_metricsCsvPath))
            return;

        _metricIndex++;

        double timeSec = Time.GetTicksMsec() / 1000.0;
        string currentScene = GetTree().CurrentScene?.Name ?? "Unknown";

        string line = string.Join(
            ",",
            _metricIndex.ToString(_culture),
            timeSec.ToString("F2", _culture),
            EscapeCsv(eventName),
            EscapeCsv(currentScene),
            EscapeCsv(fromScene),
            EscapeCsv(toScene),
            EscapeCsv(scenePath),
            EscapeCsv(levelOrder),
            EscapeCsv(levelType),
            EscapeCsv(levelTitle),
            durationMs.ToString("F2", _culture),
            EscapeCsv(details)
        );

        File.AppendAllText(_metricsCsvPath, line + System.Environment.NewLine);
    }

    private static double BytesToMegabytes(double bytes)
    {
        return bytes / 1024.0 / 1024.0;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        if (!value.Contains(',') && !value.Contains('"') && !value.Contains('\n'))
            return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
