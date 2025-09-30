using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChallengeYardFlow.Data;
using ChallengeYardFlow.Modelo;

namespace ChallengeYardFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocacoesController : ControllerBase
    {
        private readonly LocadoraContext _ctx;
        public LocacoesController(LocadoraContext ctx) => _ctx = ctx;

        [HttpPost("calcular")]
        public async Task<IActionResult> Calcular([FromBody] Locacao req)
        {
            var Moto = await _ctx.Motos.FindAsync(req.MotoId);
            if (Moto == null) return NotFound($"Moto {req.MotoId} não encontrado.");

            var dias = (req.DataFinal - req.DataInicial).Days + 1;
            var subtotal = dias * Moto.ValorDiaria;

            var resp = new
            {
                Moto = Moto.Modelo,
                DataInicial = req.DataInicial,
                DataFinal = req.DataFinal,
                ValorDiaria = Moto.ValorDiaria,
                ValorFinal = subtotal
            };

            return Ok(resp);
        }
    }
}