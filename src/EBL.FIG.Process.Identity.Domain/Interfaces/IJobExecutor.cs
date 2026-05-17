using EBL.FIG.Process.Identity.Domain.Entities;

namespace EBL.FIG.Process.Identity.Domain.Interfaces;

/// <summary>
/// Interface para execução e gerenciamento de jobs no Hangfire
/// </summary>
public interface IJobExecutor
{
    /// <summary>
    /// Registra um job recorrente no Hangfire
    /// </summary>
    Task<bool> RegisterRecurringJobAsync(JobDefinitionEntity jobDefinition);

    /// <summary>
    /// Remove um job recorrente do Hangfire
    /// </summary>
    Task<bool> RemoveRecurringJobAsync(string jobName);

    /// <summary>
    /// Enfileira um job para execução imediata
    /// </summary>
    Task<string> EnqueueJobAsync(JobDefinitionEntity jobDefinition);
}
