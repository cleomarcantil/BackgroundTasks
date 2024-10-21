using System.Text;

namespace SharedHelpers.BackgroundTasks;

public record BackgroundTaskStatus(
    bool Running,
    string Info,
    int? Progress,
    DateTime? LastExecutionStartTime,
    DateTime? LastExecutionEndTime,
    DateTime? NextExecutionStartTime,
    int ExecutionCount)
{
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(Running ? "Executando" : "Parado");

        if (!string.IsNullOrEmpty(Info))
        {
            sb.Append(", '");
            sb.Append(Info);
            sb.Append("'");
        }

        if (Progress != null)
        {
            sb.Append(", ");
            sb.Append(Progress);
            sb.Append("%");
        }

        if (LastExecutionStartTime != null)
        {
            sb.Append(", última execução: ");
            sb.Append(LastExecutionStartTime?.ToString("dd/MM/yyy HH:mm:ss"));
            sb.Append(" .. ");
            sb.Append(LastExecutionEndTime?.ToString("dd/MM/yyy HH:mm:ss"));
        }

        if (NextExecutionStartTime != null)
        {
            sb.Append(", próxima execução: ");
            sb.Append(NextExecutionStartTime?.ToString("dd/MM/yyy HH:mm:ss"));
        }

        if (ExecutionCount > 0)
        {
            sb.Append(", executado ");
            sb.Append(ExecutionCount);
            sb.Append((ExecutionCount == 1) ? " vez" : " vezes");
        }

        return sb.ToString();
    }
}
