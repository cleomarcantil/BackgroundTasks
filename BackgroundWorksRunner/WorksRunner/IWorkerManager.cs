namespace BackgroundWorksRunner.WorksRunner;

public interface IWorkerManager
{
    /// <summary>
    /// Adiciona uma tarefa para executar em background
    /// </summary>
    /// <typeparam name="T">Tipo que implementa IWorkRunner</typeparam>
    /// <param name="startDelay">Tempo de atraso até o início</param>
    /// <param name="repeatInterval">Intervalo para repetição</param>
    void AddToRun<T>(int startDelay = 0, int? repeatInterval = null) where T : IWorkRunner;

    (bool Running, string Info, int? Progress) GetStatusInfo(string key);

    IEnumerable<(string Key, string Name)> GetWorkers();
}
