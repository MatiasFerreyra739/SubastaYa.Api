namespace SubastaYa.Api.Dtos
{
    public record PujaHistorialDto(
        int Id,
        string Seudonimo,
        decimal Monto,
        DateTime FechaPuja,
        int CompradorId
    );
}