using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcanoPizza_API.Data.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ArcanoPizzaDbContext _context;

        public AdminRepository(ArcanoPizzaDbContext context)
        {
            _context = context;
        }

        // ================= USUARIOS =================

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

            // Actualiza solo los campos permitidos y la fecha de modificación
            existente.NombreUsuario = usuario.NombreUsuario;
            existente.Correo = usuario.Correo;
            existente.Telefono = usuario.Telefono;
            existente.Rol = usuario.Rol;
            existente.Activo = usuario.Activo;
            existente.UpdatedAt = DateTime.UtcNow;

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

        // ================= PRODUCTOS =================

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

            // Actualiza solo los campos permitidos y la fecha de modificación
            existente.Nombre = producto.Nombre;
            existente.Descripcion = producto.Descripcion;
            existente.PrecioBase = producto.PrecioBase;
            existente.Activo = producto.Activo;
            existente.FkIdCategoria = producto.FkIdCategoria;
            existente.UpdatedAt = DateTime.UtcNow;

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
    }
}