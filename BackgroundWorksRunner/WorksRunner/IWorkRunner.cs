namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkRunner
{
    static abstract string Name { get; }

    Task Execute(IWorkRunnerStatus s, CancellationToken cancellationToken);
}
