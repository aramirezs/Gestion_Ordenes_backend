using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dtos
{
    public class OrdenDetalleDto
    {
        public int Id { get; set; }
        public int OrdenId { get; set; }
        public string Producto { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal SubTotal { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
