using Application.Queries;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators
{
    public class ListarOrdenesQueryValidator : AbstractValidator<ListarOrdenesQuery>
    {
        public ListarOrdenesQueryValidator()
        {
            RuleFor(x => x.Page).GreaterThan(0).WithMessage("La página debe ser mayor a 0");
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("El tamaño de página debe estar entre 1 y 100");

            When(x => x.Desde.HasValue && x.Hasta.HasValue, () =>
            {
                RuleFor(x => x).Must(x => x.Desde <= x.Hasta)
                    .WithMessage("La fecha de inicio no puede ser mayor que la fecha final");
            });
        }
    }
}