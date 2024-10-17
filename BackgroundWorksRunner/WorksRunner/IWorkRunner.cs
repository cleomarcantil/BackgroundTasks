using System.ComponentModel;
using System.Reflection;

namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkRunner
{
    Task Execute(IWorkRunnerStatus s);
}

public interface IWorkRunnerStatus
{
    public string Name { get; }

    void UpdateStatusInfo(string info, int? progress = null);

    (string Info, int? Progress) GetStatusInfo();
}
