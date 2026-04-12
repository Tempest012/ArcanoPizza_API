using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArcanoPizza_API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _repo;

        public AdminController(IAdminRepository repo)
        {
            _repo = repo;
        }

        // ================= USUARIOS =================

        [HttpGet("usuarios")]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _repo.GetUsuariosAsync();

            var response = usuarios.Select(u => new UsuarioResponseDto
            {
                Id = u.IdUsuario,
                Nombre = u.NombreUsuario,
                Email = u.Correo,
                Telefono = u.Telefono,
                Tipo = u.Rol,
                Activo = u.Activo,
                FechaMiembro = u.CreatedAt
            });

            return Ok(response);
        }

        [HttpGet("usuarios/{id:int}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _repo.GetUsuarioByIdAsync(id);

            if (usuario == null)
                return NotFound();

            // Nunca devolver la entidad pura, siempre un ResponseDto
            var response = new UsuarioResponseDto
            {
                Id = usuario.IdUsuario,
                Nombre = usuario.NombreUsuario,
                Email = usuario.Correo,
                Telefono = usuario.Telefono,
                Tipo = usuario.Rol,
                Activo = usuario.Activo,
                FechaMiembro = usuario.CreatedAt
            };

            return Ok(response);
        }

        [HttpPost("usuarios")]
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioAdminDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = new Usuario
            {
                NombreUsuario = dto.Nombre,
                Correo = dto.Email,
                Telefono = dto.Telefono,
                Rol = dto.Tipo,
                Activo = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            try
            {
                var creado = await _repo.CrearUsuarioAsync(usuario);

                // Mapeamos a ResponseDto para la respuesta de éxito
                var response = new UsuarioResponseDto
                {
                    Id = creado.IdUsuario,
                    Nombre = creado.NombreUsuario,
                    Email = creado.Correo,
                    Telefono = creado.Telefono,
                    Tipo = creado.Rol,
                    Activo = creado.Activo,
                    FechaMiembro = creado.CreatedAt
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al guardar usuario", error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPut("usuarios/{id:int}")]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UsuarioUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var usuario = await _repo.GetUsuarioByIdAsync(id);
            if (usuario == null)
                return NotFound();

            // Actualizamos la entidad con los datos seguros del DTO
            usuario.NombreUsuario = dto.Nombre;
            usuario.Correo = dto.Email;
            usuario.Telefono = dto.Telefono;
            usuario.Rol = dto.Tipo;
            usuario.Activo = dto.Activo;

            var actualizado = await _repo.ActualizarUsuarioAsync(usuario);

            if (actualizado == null)
                return StatusCode(500, "No se pudo actualizar el usuario");

            return NoContent(); // 204 No Content es el estándar para PUT exitoso sin retorno
        }

        [HttpPatch("usuarios/{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var usuario = await _repo.GetUsuarioByIdAsync(id);

            if (usuario == null)
                return NotFound();

            usuario.Activo = !usuario.Activo;
            await _repo.ActualizarUsuarioAsync(usuario);

            return NoContent();
        }

        [HttpDelete("usuarios/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _repo.EliminarUsuarioAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }

        // ================= PRODUCTOS =================

        [HttpGet("productos")]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _repo.GetProductosAsync();

            var response = productos.Select(p => new ProductoResponseDto
            {
                Id = p.IdProducto,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                PrecioBase = p.PrecioBase,
                Activo = p.Activo,
                IdCategoria = p.FkIdCategoria
            });

            return Ok(response);
        }

        [HttpGet("productos/{id:int}")]
        public async Task<IActionResult> GetProducto(int id)
        {
            var producto = await _repo.GetProductoByIdAsync(id);

            if (producto == null)
                return NotFound();

            var response = new ProductoResponseDto
            {
                Id = producto.IdProducto,
                Nombre = producto.Nombre,
                Descripcion = producto.Descripcion,
                PrecioBase = producto.PrecioBase,
                Activo = producto.Activo,
                IdCategoria = producto.FkIdCategoria
            };

            return Ok(response);
        }

        [HttpPost("productos")]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoAdminDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                PrecioBase = dto.Precio,
                Activo = true,
                FkIdCategoria = 1, // Nota: Más adelante podrías querer enviar esto en el DTO
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var creado = await _repo.CrearProductoAsync(producto);

            var response = new ProductoResponseDto
            {
                Id = creado.IdProducto,
                Nombre = creado.Nombre,
                Descripcion = creado.Descripcion,
                PrecioBase = creado.PrecioBase,
                Activo = creado.Activo,
                IdCategoria = creado.FkIdCategoria
            };

            return Ok(response);
        }

        [HttpPut("productos/{id:int}")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] ProductoUpdateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var producto = await _repo.GetProductoByIdAsync(id);
            if (producto == null)
                return NotFound();

            // Mapeo seguro DTO -> Entidad
            producto.Nombre = dto.Nombre;
            producto.Descripcion = dto.Descripcion;
            producto.PrecioBase = dto.Precio;
            producto.Activo = dto.Activo;
            producto.FkIdCategoria = dto.FkIdCategoria;

            var actualizado = await _repo.ActualizarProductoAsync(producto);

            if (actualizado == null)
                return StatusCode(500, "No se pudo actualizar el producto");

            return NoContent();
        }


        [HttpPatch("productos/{id:int}/toggle")]
        public async Task<IActionResult> ToggleProducto(int id)
        {
            var producto = await _repo.GetProductoByIdAsync(id);

            if (producto == null)
                return NotFound();

            // Invertimos el estado directamente en el servidor
            producto.Activo = !producto.Activo;
            await _repo.ActualizarProductoAsync(producto);

            return NoContent(); // 204 No Content (Éxito sin devolver todo el objeto)
        }


        [HttpDelete("productos/{id:int}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var eliminado = await _repo.EliminarProductoAsync(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}