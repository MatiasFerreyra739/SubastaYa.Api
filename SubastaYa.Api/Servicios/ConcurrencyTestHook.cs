namespace SubastaYa.Api.Servicios
{
    public static class ConcurrencyTestHook
    {
        public static System.Func<System.Threading.Tasks.Task>? OnSubastaLeida { get; set; }
    }
}