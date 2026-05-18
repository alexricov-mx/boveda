using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Core.Interfaces;

/// <summary>
/// Contrato para gestión de transacciones explícitas sobre Dapper.
/// Aplica en operaciones de escritura que involucren múltiples pasos atómicos
/// (firma, notificación, actualización de estado).
/// Patrón de uso async: BeginAsync() → operaciones Dapper → CommitAsync() o RollbackAsync()
/// </summary>
public interface IUnitOfWork : IAsyncDisposable, IDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction Transaction { get; }

    /// <summary>Abre la conexión e inicia la transacción de forma asíncrona.</summary>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>Confirma todos los cambios de la transacción activa de forma asíncrona.</summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>Revierte todos los cambios de la transacción activa de forma asíncrona.</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>Compatibilidad temporal — preferir BeginAsync().</summary>
    [Obsolete("Usa BeginAsync(). Mantenido por compatibilidad temporal.")]
    void Begin();

    /// <summary>Compatibilidad temporal — preferir CommitAsync().</summary>
    [Obsolete("Usa CommitAsync(). Mantenido por compatibilidad temporal.")]
    void Commit();

    /// <summary>Compatibilidad temporal — preferir RollbackAsync().</summary>
    [Obsolete("Usa RollbackAsync(). Mantenido por compatibilidad temporal.")]
    void Rollback();
}
