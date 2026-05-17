namespace EBL.FIG.Process.Identity.Infra.Job.Interfaces;

public interface IJob
{
    /// <summary>
    /// Executa o job
    /// </summary>
    Task Execute(CancellationToken cancellationToken = default);
}
