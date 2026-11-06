using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class MascotasDbContext : DbContext
{
    public MascotasDbContext(DbContextOptions<MascotasDbContext> options)
        : base(options)
    {
    }

    public DbSet<Mascota> Mascotas { get; set; }

    public DbSet<Especie> Especies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Especie>(entity =>
        {
            entity.ToTable("Especies");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasData(
                new Especie { Id = 1, Nombre = "Perro" },
                new Especie { Id = 2, Nombre = "Gato" },
                new Especie { Id = 3, Nombre = "Otro" }
            );
        });

        modelBuilder.Entity<Mascota>(entity =>
        {
            entity.ToTable("Mascotas");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.Nombre)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(m => m.Raza)
                .HasMaxLength(50)
                .HasDefaultValue("Mestizo");

            entity.Property(m => m.Historia)
                .HasMaxLength(1500)
                .IsRequired();

            entity.Property(m => m.Estado)
                .HasConversion<string>()
                .HasDefaultValue(Domain.Enums.EstadoMascota.Disponible);

            entity.Property(m => m.Sexo)
                .HasConversion<string>();

            entity.Property(m => m.Tamano)
                .HasConversion<string>();

            entity.Property(m => m.FotoRuta)
                .HasMaxLength(500);

            entity.Property(m => m.VideoRuta)
                .HasMaxLength(500);

            entity.HasOne<Especie>()
                .WithMany(e => e.Mascotas)
                .HasForeignKey(m => m.EspecieId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}