using Application.Interfaces;
using Domain;
using Domain.Common;
using Domain.Dtos;
using Domain.Models;
using Domain.Payload;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    public record ActualizarOrdenCommand(int Id, OrdenPayload Orden) : IRequest<MessageResult<object>>;
    public class ActualizarOrdenHandler : IRequestHandler<ActualizarOrdenCommand, MessageResult<object>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public ActualizarOrdenHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MessageResult<object>> Handle(ActualizarOrdenCommand request, CancellationToken cancellationToken)
        {
            var (status, data, message) = await _unitOfWork.Ordenes.ActualizarOrdenAsync(request.Id, request.Orden);
            if (status != ServiceStatus.Ok)
                throw new ErrorHandler(
                        status == ServiceStatus.FailedValidation
                        ? HttpStatusCode.BadRequest
                        : HttpStatusCode.InternalServerError
                    , "Error al actualizar", message);

            await _unitOfWork.CommitAsync();

            return MessageResult<object>.Of(message, data);
        }
    }
}
