namespace SubastaYa.Api.Dtos
{
    public record SubastaListadoDto(
        int Id,
        string Titulo,
        string Descripcion,
        string UrlImagen,
        int CategoriaId,
        string CategoriaNombre,
        decimal PrecioBase,
        decimal IncrementoMinimo,
        decimal? PujaActual,
        int CantidadPujas,
        DateTime FechaInicio,
        DateTime FechaFin,
        string Estado,
        int? UltimaPujaUsuarioId
    );
}