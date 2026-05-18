using BERecepcion.Core.Interfaces;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace BERecepcion.Infraestructura.Repositories;

/// <summary>
/// Implementación de IUnitOfWork sobre Dapper con gestión explícita de conexión y transacción.
/// Ciclo de vida esperado (Scoped): se crea por request, BeginAsync() abre conexión,
/// CommitAsync()/RollbackAsync() cierra la transacción, DisposeAsync() libera recursos.
/// </summary>
public sealed class DapperUnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _factory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public DapperUnitOfWork(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public IDbConnection Connection => _connection
        ?? throw new InvalidOperationException("La transacción no ha sido iniciada. Llama a BeginAsync() primero.");

    public IDbTransaction Transaction => _transaction
        ?? throw new InvalidOperationException("La transacción no ha sido iniciada. Llama a BeginAsync() primero.");

    /// <inheritdoc/>
    public Task BeginAsync(CancellationToken cancellationToken = default)
    {
        _connection = _factory.CreateConnection();
        _connection.Open();
        _transaction = _connection.BeginTransaction();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Commit();
        Reset();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Rollback();
        Reset();
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    [Obsolete("Usa BeginAsync(). Mantenido por compatibilidad temporal.")]
    public void Begin()
    {
        _connection = _factory.CreateConnection();
        _connection.Open();
        _transaction = _connection.BeginTransaction();
    }

    /// <inheritdoc/>
    [Obsolete("Usa CommitAsync(). Mantenido por compatibilidad temporal.")]
    public void Commit()
    {
        _transaction?.Commit();
        Reset();
    }

    /// <inheritdoc/>
    [Obsolete("Usa RollbackAsync(). Mantenido por compatibilidad temporal.")]
    public void Rollback()
    {
        _transaction?.Rollback();
        Reset();
    }

    /// <inheritdoc/>
    public void Dispose() => Reset();

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        Reset();
        return ValueTask.CompletedTask;
    }

    private void Reset()
    {
        _transaction?.Dispose();
        _connection?.Dispose();
        _transaction = null;
        _connection = null;
    }
}
