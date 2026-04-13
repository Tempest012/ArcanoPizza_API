using ArcanoPizza_API.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcanoPizza_API.Data.Interface
{
    public interface IProductoRepository
    {
        Task<IEnumerable<Producto>> GetAllAsync();
        Task<Producto?> GetByIdAsync(int id);
    }
}
