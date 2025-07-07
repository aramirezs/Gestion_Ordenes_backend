using Application.Interfaces;
using Domain;
using Domain.Common;
using Domain.Dtos;
using Domain.Entities;
using Domain.Payload;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class OrdenWriteRepository : IOrdenWriteRepository
    {
        private readonly NpgsqlConnection connection;
        private readonly NpgsqlTransaction _tx;
        public OrdenWriteRepository(NpgsqlConnection connection, NpgsqlTransaction tx)
        {
            this.connection = connection;
            _tx = tx;
        }
        public async Task<bool> ExisteOrdenAsync(string cliente, DateTime fecha)
        {
            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM ordenes WHERE cliente = @Cliente AND fechacreacion = @Fecha", connection, _tx);
            cmd.Parameters.AddWithValue("@Cliente", cliente);
            cmd.Parameters.AddWithValue("@Fecha", fecha.Date);
            var count = (long)(await cmd.ExecuteScalarAsync())!;
            return count > 0;
        }
        public async Task<(ServiceStatus, OrdenDto?, string)> CrearOrdenAsync(OrdenPayload orden)
        {

            //var detallesJson = JsonSerializer.Serialize(orden.Detalles);

            using var cmd = new NpgsqlCommand("SELECT * FROM ordenes.func_crear_orden(@Cliente, @p_detalles_json)", connection, _tx);
            cmd.Parameters.AddWithValue("@Cliente", orden.Cliente);
            //cmd.Parameters.AddWithValue("@DetallesJson", detallesJson);

            var detallesJson = JsonSerializer.Serialize(orden.Detalles);
            var paramDetalles = new NpgsqlParameter("@p_detalles_json", NpgsqlTypes.NpgsqlDbType.Json);
            paramDetalles.Value = detallesJson;
            cmd.Parameters.Add(paramDetalles);


            try
            {
                var ordenId = (int)(await cmd.ExecuteScalarAsync())!;
                
                return (ServiceStatus.Ok, null, "Success");

            }
            catch (PostgresException ex)  
            {
                return (ServiceStatus.FailedValidation, null, $"Error Orden -> {ex.InnerException?.Message ?? ex.Message}");

            }
        }
        public async Task<(ServiceStatus, OrdenDto?, string)> ActualizarOrdenAsync(int Id, OrdenPayload payload)
        {
            
            //var detallesJson = JsonSerializer.Serialize(payload.Detalles);

            using var cmd = new NpgsqlCommand("SELECT * FROM ordenes.func_actualizar_orden(@Id, @Cliente, @p_detalles_json)", connection, _tx);
            cmd.Parameters.AddWithValue("@Id", Id);
            cmd.Parameters.AddWithValue("@Cliente", payload.Cliente);
            //cmd.Parameters.AddWithValue("@DetallesJson", detallesJson);
            var detallesJson = JsonSerializer.Serialize(payload.Detalles);
            var paramDetalles = new NpgsqlParameter("@p_detalles_json", NpgsqlTypes.NpgsqlDbType.Json);
            paramDetalles.Value = detallesJson;
            cmd.Parameters.Add(paramDetalles);

            var resultado = (string)(await cmd.ExecuteScalarAsync())!;

            return resultado switch
            {
                "OK" => (ServiceStatus.Ok, null, "Orden actualizada correctamente."),
                "NO_EXISTE" => (ServiceStatus.NotFound, null, "La orden no existe."),
                "DUPLICADO" => (ServiceStatus.FailedValidation, null, "Ya existe una orden para este cliente en esa fecha."),
                _ => (ServiceStatus.InternalError, null, "Error al actualizar la orden.")
            };

        }

        public async Task<(ServiceStatus, string)> EliminarOrdenAsync(int ordenId)
        {
            using var cmd = new NpgsqlCommand("SELECT ordenes.func_eliminar_orden(@Id)", connection, _tx);
            cmd.Parameters.AddWithValue("@Id", ordenId);

            var result = (string)(await cmd.ExecuteScalarAsync())!;

            return result switch
            {
                "OK" => (ServiceStatus.Ok, "Orden eliminada correctamente."),
                "NO_EXISTE" => (ServiceStatus.NotFound, "La orden no existe."),
                _ => (ServiceStatus.InternalError, "Error al eliminar la orden.")
            };
        }
    }
}
