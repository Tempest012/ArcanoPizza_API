-- =============================================================================
-- Datos de ejemplo para la tabla promociones (Arcano Pizza)
-- Ejecutar DESPUÉS de aplicar todas las migraciones (incl. PromocionAgregarContenido).
-- =============================================================================
--
-- Campos útiles para la pantalla de detalle:
--
-- • Descripcion — resumen corto (card, línea bajo el título).
-- • Contenido     — detalle del combo / “qué incluye” (lista). Recomendación: una línea
--                   por viñeta; el front hace split por \n y pinta bullets.
-- • tipoVigencia + fechaValidaHasta / diaSemanaRecurrente — “válido hasta…” (el texto
--   amigable lo arma el front con esos datos).
--
-- • TipoVigencia: 0 = FechaHasta | 1 = DiaSemanaRecurrente (martes = 2).
-- =============================================================================

-- DELETE FROM promociones;

INSERT INTO promociones (
    "Titulo",
    "Descripcion",
    "Contenido",
    "ImagenURL",
    "PrecioOriginal",
    "PrecioPromocional",
    "PorcentajeDescuento",
    "TipoVigencia",
    "FechaValidaHasta",
    "DiaSemanaRecurrente",
    "Activo",
    "CreatedAt",
    "UpdatedAt"
)
VALUES
(
    'Ritual Familiar',
    '2 pizzas grandes + 4 bebidas místicas + pan del conjuro',
    $rf$2 Pizzas Grandes Clásicas (a elegir)
4 Refrescos de cola 600ml
1 Orden de Pan del Conjuro$rf$,
    'https://images.unsplash.com/photo-1513104890138-7c749659a591?w=800',
    49.90,
    39.90,
    20.00,
    0,
    '2026-03-31 23:59:59',
    NULL,
    TRUE,
    NOW(),
    NOW()
),
(
    'Martes Arcano',
    'Todas las pizzas especiales con 25% de descuento',
    $ma$Aplica a pizzas marcadas como especiales en el menú.$ma$,
    'https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=800',
    19.90,
    14.93,
    25.00,
    1,
    NULL,
    2,
    TRUE,
    NOW(),
    NOW()
);

-- Si ya tenías filas sin Contenido (migración nueva), puedes actualizar:
-- UPDATE promociones SET "Contenido" = $rf$...$rf$ WHERE "Titulo" = 'Ritual Familiar';

-- SELECT "Titulo", "Descripcion", "Contenido" FROM promociones;
