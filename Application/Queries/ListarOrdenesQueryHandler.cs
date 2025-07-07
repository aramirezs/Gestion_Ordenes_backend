using Application.Interfaces;
using Domain;
using Domain.Dtos;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    public record ListarOrdenesQuery(
      string? Cliente,
      DateTime? Desde,
      DateTime? Hasta,
      int Page,
      int PageSize,
      string? OrdenarPor
  ) : IRequest<MessageResult<object>>;
    public class ListarOrdenesQueryHandler : IRequestHandler<ListarOrdenesQuery, MessageResult<object>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListarOrdenesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MessageResult<object>> Handle(ListarOrdenesQuery request, CancellationToken cancellationToken)
        {
            var(status, data, message) = await _unitOfWork.OrdenesRead.ListarOrdenesQuery(
                cliente: request.Cliente,
                desde: request.Desde,
                hasta: request.Hasta,
                page: request.Page,
                pageSize: request.PageSize,
                ordenarPor: request.OrdenarPor
            );

            if (status != ServiceStatus.Ok)
                throw new ErrorHandler(
                        status == ServiceStatus.NotFound
                        ? HttpStatusCode.BadRequest
                        : HttpStatusCode.InternalServerError
                    , "No se obtuvo información", message);

            return MessageResult<object>.Of(message, data);
        }
    }
}