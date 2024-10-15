namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkRunner
{
    Task Execute();

    //string Status { get; }

    string Name => GetType().Name;
}
