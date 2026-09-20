using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Models;
namespace SubastaYa.Api.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
        {
        }

        //Ahora vamos a decirle a Entity Framework Core qué entidades forman parte de nuestra base de datos.

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Subasta> Subastas { get; set; }
        public DbSet<Billetera> Billeteras { get; set; }
        public DbSet<Transaccion_Ledger> Transacciones_Ledgers { get; set; }
        public DbSet<Auditoria_Log> Auditorias_Log { get; set; }
        public DbSet<Puja> Pujas { get; set; }

        //Fluent API para configurar las relaciones entre las entidades y las restricciones de la base de datos.
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Categoria>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Usuario>()
                .Property(u => u.email)
                .IsRequired();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.email)
                .IsUnique();

            modelBuilder.Entity<Subasta>()
                .Property(s => s.precio_base)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Subasta>()
                .Property(s => s.incremento_minimo)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Subasta>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(s => s.vendedor_id);

            modelBuilder.Entity<Subasta>()
                .HasOne<Categoria>()
                .WithMany()
                .HasForeignKey(s => s.categoria_id);


            modelBuilder.Entity<Billetera>()
                .HasOne<Usuario>()
                .WithOne()
                .HasForeignKey<Billetera>(b => b.usuario_id);

            modelBuilder.Entity<Billetera>()
                .Property(b => b.saldo_total)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Billetera>()
                .Property(b => b.saldo_retenido)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Billetera>()
                .Property(b => b.saldo_disponible)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Subasta>()
            .Property(s => s.version)
            .IsConcurrencyToken();

            modelBuilder.Entity<Billetera>()
             .Property(b => b.version)
             .IsConcurrencyToken();

            modelBuilder.Entity<Puja>()
                 .HasOne<Usuario>()
                .WithMany()
            .HasForeignKey(p => p.comprador_id)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Puja>()
            .HasOne<Subasta>()
            .WithMany()
            .HasForeignKey(p => p.subasta_id);

            modelBuilder.Entity<Puja>()
            .Property(p => p.monto)
            .HasPrecision(18, 2);

            modelBuilder.Entity<Transaccion_Ledger>()
                .HasOne<Billetera>()
                .WithMany()
                .HasForeignKey(t => t.billetera_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaccion_Ledger>()
                .HasOne<Subasta>()
                .WithMany()
                .HasForeignKey(t => t.subasta_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaccion_Ledger>()
            .Property(t => t.monto)
            .HasPrecision(18, 2);

            modelBuilder.Entity<Auditoria_Log>()
                .HasOne<Usuario>()
                .WithMany()
                .HasForeignKey(a => a.usuario_id)
               .OnDelete(DeleteBehavior.Restrict);


        }





    }
}