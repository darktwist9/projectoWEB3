using FinanzasPersonales.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanzasPersonales.Data;

/// <summary>
/// Contexto de Entity Framework Core. Define las tablas, las relaciones del DER y sus restricciones.
/// La cadena de conexión se registra en Program.cs y se lee de appsettings.json ("FinanzasDb").
/// </summary>
public class FinanzasDbContext(DbContextOptions<FinanzasDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Billetera> Billeteras => Set<Billetera>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Transaccion> Transacciones => Set<Transaccion>();
    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<PrestamoDeuda> PrestamosDeudas => Set<PrestamoDeuda>();
    public DbSet<MetaAhorro> MetasAhorro => Set<MetaAhorro>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Todos los montos se guardan como decimal(18,2)
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Usuario (1) — posee — (N) Billetera
        modelBuilder.Entity<Usuario>(entidad =>
        {
            entidad.HasIndex(u => u.Email).IsUnique();
            entidad.HasMany(u => u.Billeteras)
                .WithOne(b => b.Usuario)
                .HasForeignKey(b => b.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Billetera (1) — registra — (N) Transacción
        modelBuilder.Entity<Billetera>(entidad =>
        {
            entidad.HasIndex(b => new { b.UsuarioId, b.Nombre }).IsUnique();
            entidad.HasMany(b => b.Transacciones)
                .WithOne(t => t.Billetera)
                .HasForeignKey(t => t.BilleteraId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Categoría (1) — clasifica — (N) Transacción
        modelBuilder.Entity<Categoria>(entidad =>
        {
            entidad.HasIndex(c => new { c.Nombre, c.Tipo }).IsUnique();
            entidad.HasMany(c => c.Transacciones)
                .WithOne(t => t.Categoria)
                .HasForeignKey(t => t.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Persona (1) — participa en — (N) Transacción  y  Persona (1) — (N) Préstamo/Deuda
        modelBuilder.Entity<Persona>(entidad =>
        {
            entidad.HasMany(p => p.Transacciones)
                .WithOne(t => t.Persona)
                .HasForeignKey(t => t.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
            entidad.HasMany(p => p.PrestamosDeudas)
                .WithOne(pd => pd.Persona)
                .HasForeignKey(pd => pd.PersonaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Transacción (N) — se registra como — (1) Préstamo/Deuda
        modelBuilder.Entity<PrestamoDeuda>(entidad =>
        {
            entidad.HasMany(pd => pd.Transacciones)
                .WithOne(t => t.PrestamoDeuda)
                .HasForeignKey(t => t.PrestamoDeudaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Meta de ahorro (1) — recibe aportes de — (N) Transacción
        modelBuilder.Entity<MetaAhorro>(entidad =>
        {
            entidad.HasIndex(m => m.Nombre).IsUnique();
            entidad.HasMany(m => m.Aportes)
                .WithOne(t => t.MetaAhorro)
                .HasForeignKey(t => t.MetaAhorroId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Transaccion>()
            .ToTable(tabla => tabla.HasCheckConstraint("CK_Transacciones_Monto", "[Monto] > 0"));
        modelBuilder.Entity<PrestamoDeuda>()
            .ToTable(tabla => tabla.HasCheckConstraint("CK_PrestamosDeudas_Monto", "[Monto] > 0"));
        modelBuilder.Entity<MetaAhorro>()
            .ToTable(tabla => tabla.HasCheckConstraint("CK_MetasAhorro_MontoObjetivo", "[MontoObjetivo] > 0"));

        DatosIniciales.Cargar(modelBuilder);
    }
}
