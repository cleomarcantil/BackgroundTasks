namespace BackgroundWorksRunner.WorksRunner;

internal record WorkRunnerItem(Type WorkType, IWorkRunnerStarterCheck StarterCheck)
{
    public string Key { get; } = Guid.NewGuid().ToString();

    internal Task? RunningTask { get; set; }
    public DateTime? LastStartTime { get; set; }
    public DateTime? LastEndTime { get; set; }
}
