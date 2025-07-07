using Domain;
using Domain.Common;
using Domain.Dtos;
using Domain.Entities;
using Domain.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IOrdenWriteRepository
    {
        Task<(ServiceStatus, OrdenDto?, string)> CrearOrdenAsync(OrdenPayload payload);
        Task<(ServiceStatus, OrdenDto?, string)> ActualizarOrdenAsync(int Id, OrdenPayload payload);
        Task<(ServiceStatus, string)> EliminarOrdenAsync(int ordenId);
    }
}
