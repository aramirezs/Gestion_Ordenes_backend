using Application.Interfaces;
using Domain;
using Domain.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands
{
    public record EliminarOrdenCommand(int OrdenId) : IRequest<MessageResult<object>>;

    public class EliminarOrdenHandler : IRequestHandler<EliminarOrdenCommand, MessageResult<object>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public EliminarOrdenHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MessageResult<object>> Handle(EliminarOrdenCommand request, CancellationToken cancellationToken)
        {
            var (status, message) = await _unitOfWork.Ordenes.EliminarOrdenAsync(request.OrdenId);
            if (status != ServiceStatus.Ok)
                throw new ErrorHandler(
                        status == ServiceStatus.FailedValidation
                        ? HttpStatusCode.BadRequest
                        : HttpStatusCode.InternalServerError
                    , "Error al eliminar", message);

            await _unitOfWork.CommitAsync();

            return MessageResult<object>.Of(message, null);
        }
    }
}