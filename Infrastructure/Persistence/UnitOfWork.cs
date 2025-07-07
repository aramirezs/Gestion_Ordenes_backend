using Application.Interfaces;
using Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;

        public IOrdenWriteRepository Ordenes { get; }   // escritura
        public IOrdenReadRepository OrdenesRead { get; } // lectura

        public UnitOfWork(string connectionString)
        {
            _connection = new NpgsqlConnection(connectionString);
            _connection.Open();
            _transaction = _connection.BeginTransaction();

            Ordenes = new OrdenWriteRepository(_connection, _transaction);
            OrdenesRead = new OrdenReadRepository(_connection); // 
        }

        public async Task CommitAsync()
        {
            await _transaction.CommitAsync();
            await _connection.CloseAsync();
        }

        public async Task RollbackAsync()
        {
            await _transaction.RollbackAsync();
            await _connection.CloseAsync();
        }

        public void Dispose()
        {
            _transaction.Dispose();
            _connection.Dispose();
        }
    }
}