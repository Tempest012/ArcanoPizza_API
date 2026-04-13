using ArcanoPizza_API.Data;
using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.DTOs;
using ArcanoPizza_API.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcanoPizza_API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Administrador")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminRepository _repo;
        private readonly ArcanoPizzaDbContext _context;

        public AdminController(IAdminRepository repo, ArcanoPizzaDbContext context)
        {
            _repo = repo;
            _context = context;
        }

        // ================= USUARIOS =================
        // (Tus métodos de usuarios se mantienen igual)

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
            if (usuario == null) return NotFound();

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
            if (!ModelState.IsValid) return BadRequest(ModelState);

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
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var usuario = await _repo.GetUsuarioByIdAsync(id);
            if (usuario == null) return NotFound();

            usuario.NombreUsuario = dto.Nombre;
            usuario.Correo = dto.Email;
            usuario.Telefono = dto.Telefono;
            usuario.Rol = dto.Tipo;
            usuario.Activo = dto.Activo;

            var actualizado = await _repo.ActualizarUsuarioAsync(usuario);
            if (actualizado == null) return StatusCode(500, "No se pudo actualizar el usuario");
            return NoContent();
        }

        [HttpPatch("usuarios/{id:int}/toggle")]
        public async Task<IActionResult> Toggle(int id)
        {
            var usuario = await _repo.GetUsuarioByIdAsync(id);
            if (usuario == null) return NotFound();
            usuario.Activo = !usuario.Activo;
            await _repo.ActualizarUsuarioAsync(usuario);
            return NoContent();
        }

        [HttpDelete("usuarios/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var eliminado = await _repo.EliminarUsuarioAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }

        // ================= PRODUCTOS =================
        // (Tus métodos de productos se mantienen igual)

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

        [HttpPost("productos")]
        public async Task<IActionResult> CrearProducto([FromBody] ProductoAdminDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var producto = new Producto
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                PrecioBase = dto.Precio,
                Activo = true,
                FkIdCategoria = 1,
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
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existente = await _repo.GetProductoByIdAsync(id);
            if (existente is null) return NotFound();

            existente.Nombre = dto.Nombre;
            existente.Descripcion = dto.Descripcion;
            existente.PrecioBase = dto.Precio;
            existente.Activo = dto.Activo;
            existente.FkIdCategoria = dto.FkIdCategoria;
            existente.UpdatedAt = DateTime.UtcNow;

            var actualizado = await _repo.ActualizarProductoAsync(existente);
            if (actualizado is null) return StatusCode(500, "No se pudo actualizar el producto");
            return NoContent();
        }

        [HttpPatch("productos/{id:int}/toggle")]
        public async Task<IActionResult> ToggleProducto(int id)
        {
            var existente = await _repo.GetProductoByIdAsync(id);
            if (existente is null) return NotFound();

            existente.Activo = !existente.Activo;
            existente.UpdatedAt = DateTime.UtcNow;

            var actualizado = await _repo.ActualizarProductoAsync(existente);
            if (actualizado is null) return StatusCode(500, "No se pudo actualizar el producto");
            return NoContent();
        }

        [HttpDelete("productos/{id:int}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var eliminado = await _repo.EliminarProductoAsync(id);
            if (!eliminado) return NotFound();
            return NoContent();
        }

        // ================= DASHBOARD =================

        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardDto>> ObtenerMetricasDashboard()
        {
            try
            {
                // 1. Ajustamos el reloj al horario de Sonora (UTC -7)
                var horaLocalSonora = DateTime.UtcNow.AddHours(-7);

                // Obtenemos las 00:00:00 y las 23:59:59 de HOY en Sonora
                var inicioHoyLocal = horaLocalSonora.Date;
                var finHoyLocal = inicioHoyLocal.AddDays(1).AddTicks(-1);

                // 2. Convertimos esos límites locales de nuevo a UTC para buscar en la base de datos
                // (porque tus fechas CreatedAt están guardadas en UTC)
                var inicioHoyUTC = inicioHoyLocal.AddHours(7);
                var finHoyUTC = finHoyLocal.AddHours(7);

                var pedidosHoy = await _context.Pedidos
                    .Include(p => p.PedidosItem)
                    .ThenInclude(i => i.Producto)
                    // 🔥 Usamos los límites corregidos con la zona horaria
                    .Where(p => p.CreatedAt >= inicioHoyUTC && p.CreatedAt <= finHoyUTC)
                    .ToListAsync();

                var dashboard = new DashboardDto
                {
                    VentasHoy = pedidosHoy.Where(p => p.Estado != "Cancelado" && p.Estado != "Rechazado").Sum(p => p.Total),

                    PedidosActivos = pedidosHoy.Count(p => p.Estado == "Pendiente" || p.Estado == "En Preparacion"),

                    ProductosVendidos = pedidosHoy
                        .Where(p => p.Estado != "Cancelado")
                        .SelectMany(p => p.PedidosItem)
                        .GroupBy(i => i.Producto.Nombre)
                        .Select(g => new ProductoVendidoDto
                        {
                            Nombre = g.Key,
                            Vendidos = g.Sum(i => i.Cantidad),
                            Total = g.Sum(i => i.Cantidad * i.PrecioUnitario)
                        })
                        .OrderByDescending(p => p.Vendidos)
                        .Take(5)
                        .ToList(),

                    // 🔥 CORRECCIÓN EXTRA: Agrupamos la hora forzando la hora de Sonora
                    PedidosPorHora = pedidosHoy
                        .GroupBy(p => p.CreatedAt.AddHours(-7).Hour)
                        .Select(g => new PedidosHoraDto
                        {
                            Hora = $"{g.Key:00}:00",
                            Cantidad = g.Count()
                        })
                        .OrderBy(p => p.Hora)
                        .ToList()
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error interno al calcular métricas: " + ex.Message);
            }
        }
    }
}