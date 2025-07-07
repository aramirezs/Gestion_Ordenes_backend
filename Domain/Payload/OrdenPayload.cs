using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Payload
{
    public class OrdenPayload
    {
        public string Cliente { get; set; } = string.Empty;
        //public decimal Total { get; set; }
        public List<OrdenDetallePayload> Detalles { get; set; } = new();
    }
}
