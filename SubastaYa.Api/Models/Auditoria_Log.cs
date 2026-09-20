namespace SubastaYa.Api.Models
{
    public class Auditoria_Log
    {
        public int id { get; set; }
        public string entidad { get; set; } = string.Empty;
        public int entidad_id { get; set; }
        public string accion { get; set; } = string.Empty;
        public int? usuario_id { get; set; }
        public string detalle_json { get; set; } = string.Empty;    
        public DateTime fecha { get; set; } 



    }
}
