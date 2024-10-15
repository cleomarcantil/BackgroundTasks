namespace BackgroundWorksRunner.WorksRunner;

internal record WorkRunnerItem(Type WorkType, int StartDelay, int? RepeatInterval)
{
    public string Key { get; } = Guid.NewGuid().ToString();

    public DateTime? NextStartTime { get; set; } = DateTime.Now.AddMilliseconds(StartDelay);

    public Task? RunningTask { get; set; }
    public (DateTime Start, DateTime? End)? LastExecutionTime { get; set; }
}
