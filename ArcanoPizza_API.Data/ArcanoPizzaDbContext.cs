using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data;

public class ArcanoPizzaDbContext : DbContext
{
    public ArcanoPizzaDbContext(DbContextOptions<ArcanoPizzaDbContext> options)
        : base(options) { }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        configurationBuilder.Properties<DateTime?>().HaveColumnType("timestamp without time zone");
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Direccion> Direcciones => Set<Direccion>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<TamanoPizza> TamanosPizza => Set<TamanoPizza>();
    public DbSet<PedidoItem> PedidosItem => Set<PedidoItem>();
    public DbSet<Extra> Extras => Set<Extra>();
    public DbSet<PedidoItemExtra> PedidosItemExtras => Set<PedidoItemExtra>();
    public DbSet<Promocion> Promociones => Set<Promocion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // usuarios
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(x => x.IdUsuario);
            e.Property(x => x.NombreUsuario).HasMaxLength(100).IsRequired();
            e.Property(x => x.Correo).HasMaxLength(200).IsRequired();
            e.HasIndex(x => x.Correo).IsUnique();
            e.Property(x => x.PasswordHash).HasMaxLength(500);
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Rol).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.ToTable("refresh_tokens");
            e.HasKey(x => x.IdRefreshToken);
            e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            e.Property(x => x.ReplacedByTokenHash).HasMaxLength(64);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasOne(x => x.Usuario)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.FkIdUsuario)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // direcciones
        modelBuilder.Entity<Direccion>(e =>
        {
            e.ToTable("direcciones");
            e.HasKey(x => x.IdDireccion);
            e.Property(x => x.Calle).HasMaxLength(200).IsRequired();
            e.Property(x => x.Colonia).HasMaxLength(100).IsRequired();
            e.Property(x => x.CodigoPostal).HasMaxLength(10).IsRequired();
            e.HasOne(x => x.Usuario)
                .WithMany(u => u.Direcciones)
                .HasForeignKey(x => x.FkIdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // promociones
        modelBuilder.Entity<Promocion>(e =>
        {
            e.ToTable("promociones");
            e.HasKey(x => x.IdPromocion);
            e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(1000);
            e.Property(x => x.Contenido).HasMaxLength(4000);
            e.Property(x => x.ImagenURL).HasMaxLength(2048);
            e.Property(x => x.PrecioOriginal).HasPrecision(10, 2);
            e.Property(x => x.PrecioPromocional).HasPrecision(10, 2);
            e.Property(x => x.PorcentajeDescuento).HasPrecision(5, 2);
            e.Property(x => x.TipoVigencia).HasConversion<int>();
            e.HasIndex(x => x.Activo);
        });

        // pedidos
        modelBuilder.Entity<Pedido>(e =>
        {
            e.ToTable("pedidos");
            e.HasKey(x => x.IdPedido);
            e.Property(x => x.Total).HasPrecision(10, 2);
            e.Property(x => x.Subtotal).HasPrecision(10, 2);
            e.Property(x => x.Impuestos).HasPrecision(10, 2);
            e.Property(x => x.DescuentoTotal).HasPrecision(10, 2);
            e.Property(x => x.Estado).HasMaxLength(50).IsRequired();
            e.Property(x => x.TipoEntrega).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Promocion)
                .WithMany(p => p.Pedidos)
                .HasForeignKey(x => x.FkIdPromocion)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasOne(x => x.Direccion)
                .WithMany(d => d.Pedidos)
                .HasForeignKey(x => x.FkIdDireccion)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(x => x.FkIdUsuario)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // pagos
        modelBuilder.Entity<Pago>(e =>
        {
            e.ToTable("pagos");
            e.HasKey(x => x.IdPago);
            e.Property(x => x.Monto).HasPrecision(10, 2);
            e.Property(x => x.Proveedor).HasMaxLength(100).IsRequired();
            e.Property(x => x.ProveedorPagoId).HasMaxLength(100);
            e.Property(x => x.Estado).HasMaxLength(50).IsRequired();
            e.Property(x => x.MetodoPago).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Pedido)
                .WithOne(p => p.Pago)
                .HasForeignKey<Pago>(x => x.FkIdPedido)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // categorias_producto
        modelBuilder.Entity<CategoriaProducto>(e =>
        {
            e.ToTable("categorias_producto");
            e.HasKey(x => x.IdCategoriasProductos);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        });

        // productos
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("productos");
            e.HasKey(x => x.IdProducto);
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(500);
            e.Property(x => x.Ingredientes).HasMaxLength(1000);
            e.Property(x => x.ImagenURL).HasMaxLength(2048);
            e.Property(x => x.PrecioBase).HasPrecision(10, 2);
            e.HasOne(x => x.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(x => x.FkIdCategoria)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // tamanos_pizza
        modelBuilder.Entity<TamanoPizza>(e =>
        {
            e.ToTable("tamanos_pizza");
            e.HasKey(x => x.IdPizza);
            e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            e.Property(x => x.ModificadorPrecio).HasPrecision(6, 2);
        });

        // pedidos_item
        modelBuilder.Entity<PedidoItem>(e =>
        {
            e.ToTable("pedidos_item");
            e.HasKey(x => x.IdPedidoItem);
            e.Property(x => x.PrecioUnitario).HasPrecision(10, 2);
            e.HasOne(x => x.Pedido)
                .WithMany(p => p.PedidosItem)
                .HasForeignKey(x => x.FkIdPedido)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Producto)
                .WithMany(p => p.PedidosItem)
                .HasForeignKey(x => x.FkIdProducto)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.TamanoPizza)
                .WithMany(t => t.PedidosItem)
                .HasForeignKey(x => x.FkIdTamanoPizza)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // extras
        modelBuilder.Entity<Extra>(e =>
        {
            e.ToTable("extras");
            e.HasKey(x => x.IdExtra);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.Property(x => x.Precio).HasPrecision(10, 2);
        });

        // pedidos_item_extras
        modelBuilder.Entity<PedidoItemExtra>(e =>
        {
            e.ToTable("pedidos_item_extras");
            e.HasKey(x => x.IdPedidoItemExtra);
            e.Property(x => x.PrecioExtra).HasPrecision(10, 2);
            e.HasOne(x => x.PedidoItem)
                .WithMany(p => p.PedidosItemExtras)
                .HasForeignKey(x => x.FkIdPedidoItem)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Extra)
                .WithMany(ex => ex.PedidosItemExtras)
                .HasForeignKey(x => x.FkIdExtra)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
