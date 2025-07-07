using Application.Commands;
using Application.Queries;
using Domain;
using Domain.Dtos;
using Domain.Entities;
using Domain.Payload;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Api_Ordenes.Controllers
{
    //[Authorize]
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OrdenesController(IMediator mediator) => _mediator = mediator;

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var orden = await _mediator.Send(new ObtenerOrdenPorIdQuery(id));
            return orden is not null ? Ok(orden) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CrearOrdenCommand command) => Ok(await _mediator.Send(command));

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] OrdenPayload dto) => Ok(await _mediator.Send(new ActualizarOrdenCommand(id, dto) ));
    
        [HttpGet("listado")]
        public async Task<ActionResult> List([FromQuery] string? cliente, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] string ordenarPor = "FechaCreacion", [FromQuery] int page = 1, [FromQuery] int pageSize = 10) => Ok(await _mediator.Send(new ListarOrdenesQuery(cliente, desde, hasta, page, pageSize, ordenarPor)));
                        
        [HttpDelete("{id:int}")]
       
        public async Task<IActionResult> Delete(int id) => Ok( await _mediator.Send( new EliminarOrdenCommand(id)) );
        
    }
}
