using Domain.Dtos;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class ActualizarOrdenValidator : AbstractValidator<OrdenDto>
    {
        public ActualizarOrdenValidator()
        {
            RuleFor(x => x.OrdenId).GreaterThan(0);
            RuleFor(o => o.Cliente).NotEmpty();
            RuleFor(o => o.Detalles).NotEmpty();

            RuleForEach(o => o.Detalles).ChildRules(detalle =>
            {
                detalle.RuleFor(d => d.Cantidad).GreaterThan(0);
                detalle.RuleFor(d => d.PrecioUnitario).GreaterThan(0);
                detalle.RuleFor(d => d.Producto).NotEmpty();
            });
        }
    }
}