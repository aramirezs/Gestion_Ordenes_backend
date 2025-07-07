using Application.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Queries
{
    public record ObtenerOrdenPorIdQuery(int Id) : IRequest<MessageResult<object>>;

    public class ObtenerOrdenPorIdHandler : IRequestHandler<ObtenerOrdenPorIdQuery, MessageResult<object>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public ObtenerOrdenPorIdHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MessageResult<object>> Handle(ObtenerOrdenPorIdQuery request, CancellationToken cancellationToken)
        {
            var (status, data, message) = await _unitOfWork.OrdenesRead.ObtenerPorIdAsync(request.Id);

            if (status != ServiceStatus.Ok)
                throw new ErrorHandler(
                        status == ServiceStatus.FailedValidation
                        ? HttpStatusCode.BadRequest
                        : HttpStatusCode.InternalServerError
                    , "Error al Actualizar", message);

            return MessageResult<object>.Of(message, data);

        }
    }
}