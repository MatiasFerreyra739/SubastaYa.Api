using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AuditoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuditorias()
        {
            var auditorias = await _context.Auditorias_Log
                .ToListAsync();

            return Ok(auditorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditoria(int id)
        {
            var auditoria = await _context.Auditorias_Log
                .FindAsync(id);

            if (auditoria == null)
                return NotFound();

            return Ok(auditoria);
        }
    }
}