using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChallengeYardFlow.Data;
using ChallengeYardFlow.Modelo;
using System.Text.Json;

namespace ChallengeYardFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class MotoController : ControllerBase
    {
        private readonly LocadoraContext _ctx;
        private readonly LinkGenerator _linkGenerator;

        public MotoController(LocadoraContext ctx, LinkGenerator linkGenerator)
        {
            _ctx = ctx;
            _linkGenerator = linkGenerator;
        }

        /// <summary>
        /// Retorna lista paginada de motos
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão = 1)</param>
        /// <param name="pageSize">Quantidade de itens por página (padrão = 5)</param>
        /// <returns>Lista de motos</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var totalItems = await _ctx.Motos.CountAsync();
            var motos = await _ctx.Motos
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var paginationMetadata = new
            {
                totalItems,
                pageSize,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(paginationMetadata));

            var motosComLinks = motos.Select(m => CriarLinksMoto(m));

            return Ok(new
            {
                metadata = paginationMetadata,
                data = motosComLinks
            });
        }

        /// <summary>
        /// Retorna uma moto pelo Id
        /// </summary>
        [HttpGet("{id}", Name = "GetMotoById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            var moto = await _ctx.Motos.FindAsync(id);
            if (moto == null) return NotFound();

            return Ok(CriarLinksMoto(moto));
        }

        /// <summary>
        /// Cria uma nova moto
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Moto novo)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _ctx.Motos.Add(novo);
            await _ctx.SaveChangesAsync();

            var motoComLinks = CriarLinksMoto(novo);

            return CreatedAtRoute("GetMotoById", new { id = novo.Id }, motoComLinks);
        }

        /// <summary>
        /// Atualiza uma moto existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Moto edit)
        {
            if (id != edit.Id) return BadRequest("Id inconsistente");

            var existe = await _ctx.Motos.AnyAsync(m => m.Id == id);
            if (!existe) return NotFound();

            _ctx.Entry(edit).State = EntityState.Modified;
            await _ctx.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Remove uma moto
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var ex = await _ctx.Motos.FindAsync(id);
            if (ex == null) return NotFound();

            _ctx.Motos.Remove(ex);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }

        
        private object CriarLinksMoto(Moto moto)
        {
            var links = new List<object>
            {
                new { rel = "self", method = "GET", href = _linkGenerator.GetPathByAction(HttpContext, "Get", "Moto", new { id = moto.Id }) },
                new { rel = "update", method = "PUT", href = _linkGenerator.GetPathByAction(HttpContext, "Update", "Moto", new { id = moto.Id }) },
                new { rel = "delete", method = "DELETE", href = _linkGenerator.GetPathByAction(HttpContext, "Delete", "Moto", new { id = moto.Id }) }
            };

            return new { moto, links };
        }
    }
}
