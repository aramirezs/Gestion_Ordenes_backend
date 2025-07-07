using Application.Commands;
using Application.Interfaces;
using Domain;
using Domain.Dtos;
using Domain.Entities;
using Domain.Payload;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ordenes.Tests.Application.Commands
{
    public class CrearOrdenHandlerTests
    {
        [Fact]
        public async Task Handle_ShouldReturnOrdenId_WhenOrdenIsValid()
        {
            // Arrange
            var ordenDto = new OrdenPayload
            {
                Cliente = "Cliente Test",
                Detalles = new List<OrdenDetallePayload>
                {
                    new OrdenDetallePayload { Producto = "Producto A", Cantidad = 2, PrecioUnitario = 100 }
                }
            };

            var ordenIdEsperado = 200;

            var mockUnitOfWork = new Mock<IUnitOfWork>();
            mockUnitOfWork.Setup(x => x.Ordenes.CrearOrdenAsync(It.IsAny<OrdenPayload>()))
                .ReturnsAsync((ServiceStatus.Ok, new OrdenDto { /* propiedades */ }, "Éxito"));


            mockUnitOfWork.Setup(u => u.CommitAsync()).Returns(Task.CompletedTask);

            var handler = new CrearOrdenHandler(mockUnitOfWork.Object);

            var command = new CrearOrdenCommand(ordenDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(ordenIdEsperado, result.Status);
        }
    }
}