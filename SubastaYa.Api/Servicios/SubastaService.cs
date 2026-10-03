using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Dtos;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Servicios
{
    public class SubastaService
    {
        private readonly ApplicationDbContext _context;

        public SubastaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // CRUD EXISTENTE
        // ============================================================

        public async Task<List<Subasta>> GetSubastasAsync()
        {
            return await _context.Subastas
                .ToListAsync();
        }

        public async Task<Subasta?> GetSubastaAsync(int id)
        {
            return await _context.Subastas
                .FindAsync(id);
        }

        public async Task<Subasta> CrearSubastaAsync(
            Subasta subasta)
        {
            _context.Subastas.Add(subasta);

            await _context.SaveChangesAsync();

            return subasta;
        }

        public async Task<Subasta?> ActualizarSubastaAsync(
            int id,
            Subasta subasta)
        {
            var subastaExistente =
                await _context.Subastas.FindAsync(id);

            if (subastaExistente == null)
            {
                return null;
            }

            subastaExistente.vendedor_id =
                subasta.vendedor_id;

            subastaExistente.categoria_id =
                subasta.categoria_id;

            subastaExistente.titulo =
                subasta.titulo;

            subastaExistente.descripcion =
                subasta.descripcion;

            subastaExistente.url_imagen =
                subasta.url_imagen;

            subastaExistente.precio_base =
                subasta.precio_base;

            subastaExistente.incremento_minimo =
                subasta.incremento_minimo;

            subastaExistente.fecha_inicio =
                subasta.fecha_inicio;

            subastaExistente.fecha_fin =
                subasta.fecha_fin;

            subastaExistente.estado =
                subasta.estado;

            subastaExistente.version =
                subasta.version;

            await _context.SaveChangesAsync();

            return subastaExistente;
        }

        public async Task<bool> EliminarSubastaAsync(int id)
        {
            var subasta =
                await _context.Subastas.FindAsync(id);

            if (subasta == null)
            {
                return false;
            }

            _context.Subastas.Remove(subasta);

            await _context.SaveChangesAsync();

            return true;
        }

        // ============================================================
        // API DEL FRONTEND
        // ============================================================

        public async Task<List<SubastaListadoDto>>
            GetSubastasListadoAsync(
                int? usuarioId = null)
        {
            var subastas =
                await _context.Subastas
                    .AsNoTracking()
                    .ToListAsync();

            var resultado =
                new List<SubastaListadoDto>();

            foreach (var subasta in subastas)
            {
                var categoria =
                    await _context.Categorias
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            c => c.Id ==
                            subasta.categoria_id);

                var pujas =
                    await _context.Pujas
                        .AsNoTracking()
                        .Where(
                            p => p.subasta_id ==
                            subasta.id)
                        .OrderByDescending(
                            p => p.fecha_puja)
                        .ToListAsync();

                var ultimaPuja =
                    pujas.FirstOrDefault();

                int? ultimaPujaUsuarioId = null;

                if (usuarioId.HasValue &&
                    ultimaPuja != null &&
                    ultimaPuja.comprador_id ==
                    usuarioId.Value)
                {
                    ultimaPujaUsuarioId =
                        ultimaPuja.comprador_id;
                }

                resultado.Add(
                    new SubastaListadoDto(
                        subasta.id,
                        subasta.titulo,
                        subasta.descripcion,
                        subasta.url_imagen,
                        subasta.categoria_id,
                        categoria?.nombre ?? string.Empty,
                        subasta.precio_base,
                        subasta.incremento_minimo,
                        ultimaPuja?.monto,
                        pujas.Count,
                        subasta.fecha_inicio,
                        subasta.fecha_fin,
                        subasta.estado,
                        ultimaPujaUsuarioId
                    )
                );
            }

            return resultado;
        }

        public async Task<SubastaDetalleDto?>
            GetSubastaDetalleAsync(
                int id,
                int? usuarioId = null)
        {
            var subasta =
                await _context.Subastas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        s => s.id == id);

            if (subasta == null)
            {
                return null;
            }

            var categoria =
                await _context.Categorias
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        c => c.Id ==
                        subasta.categoria_id);

            var pujas =
                await _context.Pujas
                    .AsNoTracking()
                    .Where(
                        p => p.subasta_id ==
                        subasta.id)
                    .OrderByDescending(
                        p => p.fecha_puja)
                    .ToListAsync();

            var ultimaPuja =
                pujas.FirstOrDefault();

            int? ultimaPujaUsuarioId = null;

            if (usuarioId.HasValue &&
                ultimaPuja != null &&
                ultimaPuja.comprador_id ==
                usuarioId.Value)
            {
                ultimaPujaUsuarioId =
                    ultimaPuja.comprador_id;
            }

            var historial =
                pujas
                    .Select(
                        p => new PujaHistorialDto(
                            p.id,
                            $"Postor {p.comprador_id + 64}",
                            p.monto,
                            p.fecha_puja,
                            p.comprador_id
                        ))
                    .ToList();

            return new SubastaDetalleDto(
                subasta.id,
                subasta.titulo,
                subasta.descripcion,
                subasta.url_imagen,
                subasta.categoria_id,
                categoria?.nombre ?? string.Empty,
                subasta.precio_base,
                subasta.incremento_minimo,
                ultimaPuja?.monto,
                pujas.Count,
                subasta.fecha_inicio,
                subasta.fecha_fin,
                subasta.estado,
                ultimaPujaUsuarioId,
                historial
            );
        }
    }
}