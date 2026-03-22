using ArcanoPizza_API.Data.Interface;
using ArcanoPizza_API.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ArcanoPizza_API.Data.Repositories
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly ArcanoPizzaDbContext _context;

        public ProductoRepository(ArcanoPizzaDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .AsNoTracking() // Buena práctica: mejora el rendimiento en GETs
                .ToListAsync();
        }

        public async Task<Producto?> GetByIdAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProducto == id);
        }
    }
}