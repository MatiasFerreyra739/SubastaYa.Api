namespace SubastaYa.Api.Models
{
    public class Usuario
    {
        public int id { get; set; }
        public string email { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string password_hash { get; set; } = string.Empty;
        public DateTime fecha_registro { get; set; }



    }
}
