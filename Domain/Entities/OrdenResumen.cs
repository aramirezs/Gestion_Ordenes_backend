using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class OrdenResumen
    {
        public int Id { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }
}
