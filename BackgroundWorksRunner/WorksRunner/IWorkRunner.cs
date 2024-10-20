namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkRunner
{
    Task Execute(IWorkRunnerStatus s, CancellationToken cancellationToken);
}
