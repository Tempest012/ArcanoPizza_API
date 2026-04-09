using ArcanoPizza_API.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcanoPizza_API.Data.Interface
{
    public interface IAdminRepository
    {
        Task<Usuario?> GetUsuarioById(int id);
        Task<Usuario?> UpdateUsuario(int id, Usuario usuario);

        Task<Producto?> GetProductoById(int id);
        Task<Producto?> UpdateProducto(int id, Producto producto);
        Task<bool> DeleteProducto(int id);
        // Usuarios
        Task<List<Usuario>> GetUsuariosAsync();
        Task<Usuario?> GetUsuarioByIdAsync(int id);
        Task<Usuario> CrearUsuarioAsync(Usuario usuario);
        Task<Usuario?> ActualizarUsuarioAsync(Usuario usuario);
        Task<bool> EliminarUsuarioAsync(int id);

        // Productos
        Task<List<Producto>> GetProductosAsync();
        Task<Producto?> GetProductoByIdAsync(int id);
        Task<Producto> CrearProductoAsync(Producto producto);
        Task<Producto?> ActualizarProductoAsync(Producto producto);
        Task<bool> EliminarProductoAsync(int id);
    }
}
