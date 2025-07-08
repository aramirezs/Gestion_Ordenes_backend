using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos
{
    public class OrdenDto
    {
        public int OrdenId { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string FechaCreacion { get; set; }
        public decimal Total { get; set; }
        public List<OrdenDetalleDto> Detalles { get; set; } = new();
    }
}
