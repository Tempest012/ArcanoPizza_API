using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.AspNetCore.Mvc;

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
            var usuario = await _repo.GetUsuarioById(id);

            if (usuario == null)
                return NotFound();

            return Ok(usuario);
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
                return Ok(creado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al guardar usuario",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpPut("usuarios/{id:int}")]
        public async Task<IActionResult> UpdateUsuario(int id, Usuario usuario)
        {
            usuario.UpdatedAt = DateTime.UtcNow; // ✅ FIX

            var actualizado = await _repo.UpdateUsuario(id, usuario);

            if (actualizado == null)
                return NotFound();

            return Ok(actualizado);
        }

        [HttpPatch("usuarios/{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var usuario = await _repo.GetUsuarioByIdAsync(id);

            if (usuario == null)
                return NotFound();

            usuario.Activo = !usuario.Activo;
            usuario.UpdatedAt = DateTime.UtcNow; // ✅ FIX

            await _repo.ActualizarUsuarioAsync(usuario);

            return Ok(usuario);
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
            return Ok(productos);
        }

        [HttpGet("productos/{id:int}")]
        public async Task<IActionResult> GetProducto(int id)
        {
            var producto = await _repo.GetProductoById(id);

            if (producto == null)
                return NotFound();

            return Ok(producto);
        }

        [HttpPost("productos")]
        public async Task<IActionResult> CrearProducto(ProductoAdminDto dto)
        {
            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                PrecioBase = dto.Precio,
                Activo = true,
                FkIdCategoria = 1,
                CreatedAt = DateTime.UtcNow,   // ✅ FIX
                UpdatedAt = DateTime.UtcNow    // ✅ FIX
            };

            var creado = await _repo.CrearProductoAsync(producto);
            return Ok(creado);
        }

        [HttpPut("productos/{id:int}")]
        public async Task<IActionResult> UpdateProducto(int id, Producto producto)
        {
            producto.UpdatedAt = DateTime.UtcNow; // ✅ FIX

            var actualizado = await _repo.UpdateProducto(id, producto);

            if (actualizado == null)
                return NotFound();

            return Ok(actualizado);
        }

        [HttpDelete("productos/{id:int}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var eliminado = await _repo.DeleteProducto(id);

            if (!eliminado)
                return NotFound();

            return NoContent();
        }
    }
}