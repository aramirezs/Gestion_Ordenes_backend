using Domain;
using Domain.Common;
using Domain.Dtos;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrdenReadRepository
    {
        Task<(ServiceStatus, OrdenDto?, string)> ObtenerPorIdAsync(int id);
        Task<(ServiceStatus, PaginationResult<OrdenDto>?, string)> ListarOrdenesQuery(
            string? cliente,
            DateTime? desde,
            DateTime? hasta,
            int page,
            int pageSize,
            string? ordenarPor);
        //Task<(ServiceStatus, IEnumerable<OrdenDto>?, string)> ListarOrdenesQuery(string? cliente, DateTime? desde, DateTime? hasta, int page, int pageSize, string? ordenarPor);
    }
}
