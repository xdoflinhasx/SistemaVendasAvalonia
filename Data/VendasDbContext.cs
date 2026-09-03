using System;
using Microsoft.EntityFrameworkCore;

namespace MeuAppAvalonia;

public sealed class VendasDbContext : DbContext
{
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (optionsBuilder.IsConfigured)
            return;

        var connectionString = Environment.GetEnvironmentVariable("MEUAPPAVALONIA_CONNECTION_STRING")
            ?? "Server=localhost\\SQLEXPRESS;Database=SistemaVendas;Trusted_Connection=True;TrustServerCertificate=True;";

        optionsBuilder.UseSqlServer(connectionString, sqlOptions => sqlOptions.UseCompatibilityLevel(120));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ItemVenda>(entity =>
        {
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(item => item.Nome).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Preco).HasPrecision(18, 2);
            entity.Ignore(item => item.TextoPreco);
            entity.Ignore(item => item.Total);
        });
    }
}