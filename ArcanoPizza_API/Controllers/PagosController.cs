using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using ArcanoPizza_API.DTOs; // Trae tu nuevo DTO
using ArcanoPizza_API.Data.Interface; // Trae tu IProductoRepository

namespace ArcanoPizza_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagosController : Controller
    {
        private readonly IProductoRepository _productoRepository;

        public PagosController(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        [HttpPost("crear-sesion")]
        public async Task<ActionResult> CrearSesionCheckout([FromBody] List<CarritoItemSeguroDto> carritoDesdeAngular)
        {
            // 🔴 IMPORTANTE: Aquí deberás pegar tu llave secreta de Stripe que empieza con "sk_test_"
            Stripe.StripeConfiguration.ApiKey = "sk_test_51TDZVQ2ODoi89sv6OUlMNFe2cxkV1ZRbgCcvyRLtvoeVG4JaFk5BXZ2HllLpinseh95gePIIIVN6AZIJQdVTOzOY00a7GOEZu3";

            var lineItems = new List<SessionLineItemOptions>();

            // ¡Aquí usamos el método exacto que acabas de encontrar!
            var productosEnBaseDeDatos = await _productoRepository.GetAllAsync();

            foreach (var item in carritoDesdeAngular)
            {
                // Usamos IdProducto de tu base de datos
                var productoReal = productosEnBaseDeDatos.FirstOrDefault(p => p.IdProducto == item.ProductoId);

                if (productoReal != null)
                {
                    lineItems.Add(new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            // Usamos PrecioBase de tu base de datos
                            UnitAmount = (long)(productoReal.PrecioBase * 100),
                            Currency = "mxn",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = productoReal.Nombre
                            },
                        },
                        Quantity = item.Cantidad,
                    });
                }
            }

            if (lineItems.Count == 0) return BadRequest("El carrito está vacío o los productos no son válidos.");

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                // Si tu Angular corre en otro puerto (ej. 4200), asegúrate de que esto coincida
                SuccessUrl = "http://localhost:4200/pago-exitoso",
                CancelUrl = "http://localhost:4200/carrito-compra",
            };

            var service = new SessionService();
            Session session = await service.CreateAsync(options);

            return Ok(new { url = session.Url });
        }
    }
}