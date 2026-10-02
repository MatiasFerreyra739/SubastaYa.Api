using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Servicios;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriasController : ControllerBase
    {
        private readonly AuditoriaService _auditoriaService;

        public AuditoriasController(
            AuditoriaService auditoriaService)
        {
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditorias()
        {
            var auditorias =
                await _auditoriaService
                    .GetAuditoriasAsync();

            return Ok(auditorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditoria(int id)
        {
            var auditoria =
                await _auditoriaService
                    .GetAuditoriaAsync(id);

            if (auditoria == null)
            {
                return NotFound();
            }

            return Ok(auditoria);
        }
    }
}