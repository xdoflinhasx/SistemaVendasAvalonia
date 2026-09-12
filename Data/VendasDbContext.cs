using System;
using Microsoft.EntityFrameworkCore;

namespace MeuAppAvalonia;

public sealed class VendasDbContext : DbContext
{
    public DbSet<ItemVenda> ItensVenda => Set<ItemVenda>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Vendedor> Vendedores => Set<Vendedor>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<ConfiguracaoSecret> ConfiguracoesSecret => Set<ConfiguracaoSecret>();

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
            entity.Property(item => item.VendaId);
            entity.Ignore(item => item.TextoPreco);
            entity.Ignore(item => item.Total);
            entity.HasOne(item => item.Venda)
                .WithMany(venda => venda.Itens)
                .HasForeignKey(item => item.VendaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Venda>(entity =>
        {
            entity.HasKey(venda => venda.Id);
            entity.Property(venda => venda.Cliente).HasMaxLength(200).IsRequired();
            entity.Property(venda => venda.Vendedor).HasMaxLength(200).IsRequired();
            entity.Property(venda => venda.FormaPagamento).HasMaxLength(100).IsRequired();
            entity.Property(venda => venda.Desconto).HasPrecision(18, 2);
            entity.HasMany(venda => venda.Itens)
                .WithOne(item => item.Venda)
                .HasForeignKey(item => item.VendaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(cliente => cliente.Id);
            entity.Property(cliente => cliente.Nome).HasMaxLength(200).IsRequired();
            entity.Property(cliente => cliente.Documento).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Vendedor>(entity =>
        {
            entity.HasKey(vendedor => vendedor.Id);
            entity.Property(vendedor => vendedor.Nome).HasMaxLength(200).IsRequired();
            entity.Property(vendedor => vendedor.Codigo).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(produto => produto.Id);
            entity.Property(produto => produto.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(produto => produto.Nome).HasMaxLength(200).IsRequired();
            entity.Property(produto => produto.Preco).HasPrecision(18, 2);
        });

        modelBuilder.Entity<ConfiguracaoSecret>(entity =>
        {
            entity.ToTable("Configuracao");
            entity.HasKey(config => config.Id);
            entity.Property(config => config.ClientId).HasMaxLength(200).IsRequired();
            entity.Property(config => config.EmpresaNome).HasMaxLength(200).IsRequired();
            entity.Property(config => config.EmpresaCnpj).HasMaxLength(30).IsRequired();
            entity.Property(config => config.DeviceName).HasMaxLength(200).IsRequired();
            entity.Property(config => config.SecretGerado).HasMaxLength(300).IsRequired();
            entity.Property(config => config.UrlEndpoint).HasMaxLength(1000).IsRequired();
            entity.Property(config => config.TokenAcesso).HasMaxLength(500).IsRequired();
            entity.Property(config => config.TokenExpiraEm).IsRequired();
            entity.Property(config => config.DataAtualizacao).IsRequired();
        });
    }
}