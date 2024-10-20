namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkerContext : IDisposable
{
    IWorkRunner GetInstance();
}