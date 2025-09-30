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
    public class UsuariosController : ControllerBase
    {
        private readonly LocadoraContext _ctx;
        private readonly LinkGenerator _linkGenerator;

        public UsuariosController(LocadoraContext ctx, LinkGenerator linkGenerator)
        {
            _ctx = ctx;
            _linkGenerator = linkGenerator;
        }

        /// <summary>
        /// Retorna lista paginada de usuários
        /// </summary>
        /// <param name="pageNumber">Número da página (padrão = 1)</param>
        /// <param name="pageSize">Itens por página (padrão = 5)</param>
        /// <returns>Lista de usuários</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
        {
            var totalItems = await _ctx.Usuarios.CountAsync();
            var usuarios = await _ctx.Usuarios
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var metadata = new
            {
                totalItems,
                pageSize,
                currentPage = pageNumber,
                totalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
            };

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(metadata));

            var usuariosComLinks = usuarios.Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Funcao,
                links = new List<object>
                {
                    new { rel = "self", method = "GET", href = _linkGenerator.GetPathByAction(HttpContext, "GetById", "Usuarios", new { id = u.Id }) },
                    new { rel = "update", method = "PUT", href = _linkGenerator.GetPathByAction(HttpContext, "Update", "Usuarios", new { id = u.Id }) },
                    new { rel = "delete", method = "DELETE", href = _linkGenerator.GetPathByAction(HttpContext, "Delete", "Usuarios", new { id = u.Id }) }
                }
            });

            return Ok(new { metadata, data = usuariosComLinks });
        }

        /// <summary>
        /// Retorna um usuário pelo Id
        /// </summary>
        /// <param name="id">Id do usuário</param>
        [HttpGet("{id}", Name = "GetUsuarioById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var usuario = await _ctx.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            var resp = new
            {
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.Funcao,
                links = new List<object>
                {
                    new { rel = "self", method = "GET", href = _linkGenerator.GetPathByAction(HttpContext, "GetById", "Usuarios", new { id = usuario.Id }) },
                    new { rel = "update", method = "PUT", href = _linkGenerator.GetPathByAction(HttpContext, "Update", "Usuarios", new { id = usuario.Id }) },
                    new { rel = "delete", method = "DELETE", href = _linkGenerator.GetPathByAction(HttpContext, "Delete", "Usuarios", new { id = usuario.Id }) }
                }
            };

            return Ok(resp);
        }

        /// <summary>
        /// Cria um novo usuário
        /// </summary>
        /// <param name="novo">Objeto usuário</param>
        /// <returns>Usuário criado</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Usuario novo)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _ctx.Usuarios.Add(novo);
            await _ctx.SaveChangesAsync();

            var usuarioComLinks = new
            {
                novo.Id,
                novo.Nome,
                novo.Email,
                novo.Funcao,
                links = new List<object>
                {
                    new { rel = "self", method = "GET", href = _linkGenerator.GetPathByAction(HttpContext, "GetById", "Usuarios", new { id = novo.Id }) },
                    new { rel = "update", method = "PUT", href = _linkGenerator.GetPathByAction(HttpContext, "Update", "Usuarios", new { id = novo.Id }) },
                    new { rel = "delete", method = "DELETE", href = _linkGenerator.GetPathByAction(HttpContext, "Delete", "Usuarios", new { id = novo.Id }) }
                }
            };

            return CreatedAtRoute("GetUsuarioById", new { id = novo.Id }, usuarioComLinks);
        }

        /// <summary>
        /// Atualiza um usuário existente
        /// </summary>
        /// <param name="id">Id do usuário</param>
        /// <param name="edit">Objeto usuário</param>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Usuario edit)
        {
            if (id != edit.Id) return BadRequest("Id inconsistente");

            var existe = await _ctx.Usuarios.AnyAsync(u => u.Id == id);
            if (!existe) return NotFound();

            _ctx.Entry(edit).State = EntityState.Modified;
            await _ctx.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Remove um usuário
        /// </summary>
        /// <param name="id">Id do usuário</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var ex = await _ctx.Usuarios.FindAsync(id);
            if (ex == null) return NotFound();

            _ctx.Usuarios.Remove(ex);
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
