namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkerManager
{
    /// <summary>
    /// Adiciona uma tarefa de um tipo para executar em background
    /// </summary>
    /// <typeparam name="T">Tipo que implementa IWorkRunner</typeparam>
    /// <param name="startDelay">Tempo de atraso até o início</param>
    /// <param name="repeatInterval">Intervalo para repetição</param>
    void AddToRun<T>(int startDelay = 0, int? repeatInterval = null) where T : IWorkRunner;

    /// <summary>
    /// Adiciona uma instância de um tipo para executar em background
    /// </summary>
    /// <param name="instance">Instância</param>
    /// <param name="startDelay">Tempo de atraso até o início</param>
    /// <param name="repeatInterval">Intervalo para repetição</param>
    void AddToRun(IWorkRunner instance, int startDelay = 0, int? repeatInterval = null);

    Task CaptureWorkersRunnerStatus(WorkRunnerStatusChanged callback, CancellationToken cancellationToken);
}

public delegate Task WorkRunnerStatusChanged(IEnumerable<(string Name, WorkRunnerStatusInfo Status)> statusChanges);
