using Application.Interfaces;
using Domain;
using Domain.Entities;
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
    //public class CrearOrdenCommand : IRequest<int>
    //{
        public record CrearOrdenCommand(OrdenPayload Orden) : IRequest<MessageResult<object>>;
        public class CrearOrdenHandler : IRequestHandler<CrearOrdenCommand, MessageResult<object>>
        {
            private readonly IUnitOfWork _unitOfWork;
            public CrearOrdenHandler(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

            public async Task<MessageResult<object>> Handle(CrearOrdenCommand request, CancellationToken cancellationToken)
            {
                
                var (status, data, message) = await _unitOfWork.Ordenes.CrearOrdenAsync(request.Orden);
                if (status != ServiceStatus.Ok)
                    throw new ErrorHandler(
                            status == ServiceStatus.FailedValidation
                            ? HttpStatusCode.BadRequest
                            : HttpStatusCode.InternalServerError
                        , "Error al crear", message);

            await _unitOfWork.CommitAsync();
                return MessageResult<object>.Of(message, data);
            }
        }
}
