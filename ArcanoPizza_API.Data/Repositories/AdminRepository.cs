using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;


namespace ArcanoPizza_API.Data.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ArcanoPizzaDbContext _context;

        public AdminRepository(ArcanoPizzaDbContext context)
        {
            _context = context;
        }

        // ===== USUARIOS =====

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            return await _context.Usuarios.ToListAsync();
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario> CrearUsuarioAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario?> ActualizarUsuarioAsync(Usuario usuario)
        {
            var existente = await _context.Usuarios.FindAsync(usuario.IdUsuario);
            if (existente == null) return null;

            _context.Entry(existente).CurrentValues.SetValues(usuario);
            await _context.SaveChangesAsync();

            return existente;
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return false;

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return true;
        }

        // ===== PRODUCTOS =====

        public async Task<List<Producto>> GetProductosAsync()
        {
            return await _context.Productos.ToListAsync();
        }

        public async Task<Producto?> GetProductoByIdAsync(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<Producto> CrearProductoAsync(Producto producto)
        {
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();
            return producto;
        }

        public async Task<Producto?> ActualizarProductoAsync(Producto producto)
        {
            var existente = await _context.Productos.FindAsync(producto.IdProducto);
            if (existente == null) return null;

            _context.Entry(existente).CurrentValues.SetValues(producto);
            await _context.SaveChangesAsync();

            return existente;
        }

        public async Task<bool> EliminarProductoAsync(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return false;

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<Usuario?> GetUsuarioById(int id)
        {
            return await _context.Usuarios.FindAsync(id);
        }

        public async Task<Usuario?> UpdateUsuario(int id, Usuario usuario)
        {
            var existing = await _context.Usuarios.FindAsync(id);

            if (existing == null) return null;

            existing.NombreUsuario = usuario.NombreUsuario;
            existing.Correo = usuario.Correo;
            existing.Telefono = usuario.Telefono;
            existing.Rol = usuario.Rol;
            existing.Activo = usuario.Activo;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<Producto?> GetProductoById(int id)
        {
            return await _context.Productos.FindAsync(id);
        }

        public async Task<Producto?> UpdateProducto(int id, Producto producto)
        {
            var existing = await _context.Productos.FindAsync(id);

            if (existing == null) return null;

            existing.Nombre = producto.Nombre;
            existing.Descripcion = producto.Descripcion;
            existing.PrecioBase = producto.PrecioBase;
            existing.Activo = producto.Activo;
            existing.FkIdCategoria = producto.FkIdCategoria;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null) return false;

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
