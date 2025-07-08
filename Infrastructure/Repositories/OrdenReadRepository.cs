using Application.Interfaces;
using Domain;
using Domain.Common;
using Domain.Dtos;
using Domain.Entities;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class OrdenReadRepository : IOrdenReadRepository
    {
        private readonly NpgsqlConnection connection;
        public OrdenReadRepository(NpgsqlConnection connection)
        {
            this.connection = connection;
        }

        public async Task<(ServiceStatus, OrdenDto?, string)> ObtenerPorIdAsync(int id)
        {
            await connection.OpenAsync();

            var ordenCmd = new NpgsqlCommand("SELECT * FROM ordenes.func_get_orden_by_id(@Id)", connection);
            ordenCmd.Parameters.AddWithValue("@Id", id);
            using var reader = await ordenCmd.ExecuteReaderAsync();
            if (!reader.Read()) return (ServiceStatus.NotFound, null, "No hay registros para mostrar");

            var orden = new OrdenDto
            {
                OrdenId = reader.GetFieldValue<int>(reader.GetOrdinal("id")),
                FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fechacreacion")).ToString("yyyy-MM-dd HH:mm:ss"),
                Cliente = reader.GetFieldValue<string>(reader.GetOrdinal("cliente")),
                Total = reader.GetFieldValue<decimal>(reader.GetOrdinal("total")),
                Detalles = new List<OrdenDetalleDto>()
            };
            await reader.CloseAsync();

            var detallesCmd = new NpgsqlCommand("SELECT * FROM ordenes.func_get_orden_detalles_by_id(@Id)", connection);
            detallesCmd.Parameters.AddWithValue("@Id", id);
            using var detallesReader = await detallesCmd.ExecuteReaderAsync();

            if (!detallesReader.Read()) return (ServiceStatus.NotFound, null, "No hay registros para mostrar");

            while (await detallesReader.ReadAsync())
            {
                orden.Detalles.Add(new OrdenDetalleDto
                {
                    Id = detallesReader.GetFieldValue<int>(detallesReader.GetOrdinal("id")),
                    OrdenId = detallesReader.GetFieldValue<int>(detallesReader.GetOrdinal("ordenid")),
                    Producto = detallesReader.GetFieldValue<string>(detallesReader.GetOrdinal("producto")),
                    Cantidad = detallesReader.GetFieldValue<int>(detallesReader.GetOrdinal("cantidad")),
                    PrecioUnitario = detallesReader.GetFieldValue<decimal>(detallesReader.GetOrdinal("preciounitario")),
                    SubTotal = detallesReader.GetFieldValue<decimal>(detallesReader.GetOrdinal("subtotal"))
                });
            }
            return (ServiceStatus.Ok, orden, "Succeeded");
        }

        public async Task<(ServiceStatus, PaginationResult<OrdenDto>?, string)> ListarOrdenesQuery(
    string? cliente,
    DateTime? desde,
    DateTime? hasta,
    int page,
    int pageSize,
    string? ordenarPor)
        {
            try
            {
                var ordenColumn = ordenarPor?.ToLower() switch
                {
                    "cliente" => "cliente",
                    "total" => "total",
                    _ => "fechacreacion"
                };

                var parameters = new List<NpgsqlParameter>
        {
            new("@Cliente", (object?)cliente ?? DBNull.Value),
            new("@Desde", (object?)desde ?? DBNull.Value),
            new("@Hasta", hasta.HasValue ? (object)hasta.Value.AddDays(1) : DBNull.Value),
            new("@Page", page),
            new("@PageSize", pageSize),
            new("@OrdenarPor", ordenColumn)
        };

                var ordenes = new List<OrdenDto>();
                int totalPages = 1;
                int totalItems = 0;

                using var cmd = new NpgsqlCommand("SELECT * FROM ordenes.func_listar_ordenes(@Cliente, @Desde, @Hasta, @Page, @PageSize, @OrdenarPor)", connection);
                cmd.Parameters.AddRange(parameters.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    // Solo leer total una vez (en la primera fila)
                    if (totalItems == 0 && totalPages == 1)
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("total_items")))
                            totalItems = reader.GetInt32(reader.GetOrdinal("total_items"));
                        if (!reader.IsDBNull(reader.GetOrdinal("total_pages")))
                            totalPages = reader.GetInt32(reader.GetOrdinal("total_pages"));
                    }

                    var orden = new OrdenDto
                    {
                        OrdenId = reader.GetInt32(reader.GetOrdinal("id")),
                        Cliente = reader.GetString(reader.GetOrdinal("cliente")),
                        FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fechacreacion")).ToString("yyyy-MM-dd HH:mm:ss"),
                        Total = reader.GetDecimal(reader.GetOrdinal("total"))
                    };

                    ordenes.Add(orden);
                }

                if (!ordenes.Any())
                    return (ServiceStatus.NotFound, null, "No hay registros para mostrar");

                await reader.CloseAsync();

                // Obtener detalles de cada orden (secuencialmente)
                foreach (var orden in ordenes)
                {
                    var detallesCmd = new NpgsqlCommand("SELECT * FROM ordenes.func_get_orden_detalles_by_id(@p_ordenid)", connection);
                    detallesCmd.Parameters.AddWithValue("@p_ordenid", orden.OrdenId);

                    using var detallesReader = await detallesCmd.ExecuteReaderAsync();
                    while (await detallesReader.ReadAsync())
                    {
                        orden.Detalles.Add(new OrdenDetalleDto
                        {
                            Id = detallesReader.GetInt32(detallesReader.GetOrdinal("id")),
                            OrdenId = detallesReader.GetInt32(detallesReader.GetOrdinal("ordenid")),
                            Producto = detallesReader.GetString(detallesReader.GetOrdinal("producto")),
                            Cantidad = detallesReader.GetInt32(detallesReader.GetOrdinal("cantidad")),
                            PrecioUnitario = detallesReader.GetDecimal(detallesReader.GetOrdinal("preciounitario")),
                            SubTotal = detallesReader.GetDecimal(detallesReader.GetOrdinal("subtotal"))
                        });
                    }
                }

                var paginationResult = new PaginationResult<OrdenDto>
                {
                    Items = ordenes,
                    Page = page,
                    Pages = totalPages,
                    Total = totalItems
                };

                return (ServiceStatus.Ok, paginationResult, "Succeeded");
            }
            catch (Exception ex)
            {
                return (ServiceStatus.InternalError, null, $"Error al consultar -> {ex.InnerException?.Message ?? ex.Message}");
            }
        }



        public async Task<(ServiceStatus, IEnumerable<OrdenDto>?, string)> ListarOrdenesQueryV1(string? cliente, DateTime? desde, DateTime? hasta, int page, int pageSize, string? ordenarPor)
        {
            try
            {
                var ordenColumn = ordenarPor?.ToLower() switch
                {
                    "cliente" => "cliente",
                    "total" => "total",
                    _ => "fechacreacion"
                };

                var parameters = new List<NpgsqlParameter>
            {
                new("@Cliente", (object?)cliente ?? DBNull.Value),
                new("@Desde", (object?)desde ?? DBNull.Value),
                new("@Hasta", (object?)hasta.Value.AddDays(1) ?? DBNull.Value),
                new("@Page", page),
                new("@PageSize", pageSize),
                        new("@OrdenarPor", ordenColumn)
             };



                using var cmd = new NpgsqlCommand("SELECT * FROM ordenes.func_listar_ordenes(@Cliente, @Desde, @Hasta, @Page, @PageSize, @OrdenarPor)", connection);
                cmd.Parameters.AddRange(parameters.ToArray());

                using var reader = await cmd.ExecuteReaderAsync();

                var ordenes = new List<OrdenDto>();
                while (await reader.ReadAsync())
                {
                    ordenes.Add(new OrdenDto
                    {
                        OrdenId = reader.GetInt32(reader.GetOrdinal("id")),
                        Cliente = reader.GetString(reader.GetOrdinal("cliente")),
                        FechaCreacion = reader.GetDateTime(reader.GetOrdinal("fechacreacion")).ToString("yyyy-MM-dd HH:mm:ss"),
                        Total = reader.GetDecimal(reader.GetOrdinal("total"))
                    });
                }
                if (!ordenes.Any()) return (ServiceStatus.NotFound, null, "No hay registros para mostrar");

                await reader.CloseAsync();

                foreach (var orden in ordenes)
                {
                    #region consultar detalle
                    var detallesCmd = new NpgsqlCommand("SELECT * FROM ordenes.func_get_orden_detalles_by_id(@p_ordenid)", connection);
                    detallesCmd.Parameters.AddWithValue("@p_ordenid", orden.OrdenId);
                    using var detallesReader = await detallesCmd.ExecuteReaderAsync();

                    //if (!detallesReader.Read()) return (ServiceStatus.NotFound, null, "No existen detalles de la orden");

                    while (await detallesReader.ReadAsync())
                    {
                        orden.Detalles.Add(new OrdenDetalleDto
                        {
                            Id = detallesReader.GetFieldValue<int>(detallesReader.GetOrdinal("id")),
                            OrdenId = detallesReader.GetFieldValue<int>(detallesReader.GetOrdinal("ordenid")),
                            Producto = detallesReader.GetFieldValue<string>(detallesReader.GetOrdinal("producto")),
                            Cantidad = detallesReader.GetFieldValue<int>(detallesReader.GetOrdinal("cantidad")),
                            PrecioUnitario = detallesReader.GetFieldValue<decimal>(detallesReader.GetOrdinal("preciounitario")),
                            SubTotal = detallesReader.GetFieldValue<decimal>(detallesReader.GetOrdinal("subtotal"))
                        });
                    }

                    #endregion
                }

                return (ServiceStatus.Ok, ordenes, "Succeeded");
            }
            catch (Exception ex)
            {

                return (ServiceStatus.InternalError, null, $"Error al consultar -> {ex.InnerException?.Message ?? ex.Message}");

            }
            
        }

    }
}