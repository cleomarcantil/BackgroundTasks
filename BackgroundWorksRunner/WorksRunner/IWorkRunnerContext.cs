namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkRunnerContext : IDisposable
{
    IWorkRunner GetInstance();
}