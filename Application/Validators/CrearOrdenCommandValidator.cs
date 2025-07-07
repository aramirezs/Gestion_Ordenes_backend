using Application.Commands;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class CrearOrdenCommandValidator : AbstractValidator<CrearOrdenCommand>
    {
        public CrearOrdenCommandValidator()
        {

            RuleFor(x => x.Orden.Cliente)
                .NotEmpty().WithMessage("El cliente es obligatorio");

            RuleFor(x => x.Orden.Detalles)
                .NotEmpty().WithMessage("Debe registrar al menos un detalle");

            RuleForEach(x => x.Orden.Detalles).ChildRules(detalle =>
            {
                detalle.RuleFor(d => d.Producto)
                    .NotEmpty().WithMessage("El producto es obligatorio");

                detalle.RuleFor(d => d.Cantidad)
                    .GreaterThan(0).WithMessage("La cantidad debe ser mayor a cero");

                detalle.RuleFor(d => d.PrecioUnitario)
                    .GreaterThan(0).WithMessage("El precio unitario debe ser mayor a cero");

                //extras
                 detalle.RuleFor(d => d.Producto)
                .NotEmpty()
                .MaximumLength(100)
                .Matches("^[a-zA-Z0-9 .,'-]*$")
                .WithMessage("Nombre de producto contiene caracteres inválidos.");
            });
        }
    }
}