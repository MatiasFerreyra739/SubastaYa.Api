using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class AuditoriaService
    {
        private readonly ApplicationDbContext _context;

        public AuditoriaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Auditoria_Log>> GetAuditoriasAsync()
        {
            return await _context.Auditorias_Log
                .ToListAsync();
        }

        public async Task<Auditoria_Log?> GetAuditoriaAsync(int id)
        {
            return await _context.Auditorias_Log
                .FindAsync(id);
        }

        public async Task RegistrarAsync(
            string entidad,
            int entidadId,
            string accion,
            int? usuarioId,
            string detalleJson)
        {
            var auditoria = new Auditoria_Log
            {
                entidad = entidad,
                entidad_id = entidadId,
                accion = accion,
                usuario_id = usuarioId,
                detalle_json = detalleJson,
                fecha = DateTime.Now
            };

            _context.Auditorias_Log.Add(auditoria);

            await _context.SaveChangesAsync();
        }
    }
}