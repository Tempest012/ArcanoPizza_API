# Política de Seguridad — ArcanoPizza API

## Cobertura OWASP Top 10:2025

Este proyecto implementa medidas de seguridad alineadas con el [OWASP Top 10:2025](https://owasp.org/Top10/2025/).

### Resumen de implementación

| OWASP | Riesgo | Implementación |
|-------|--------|----------------|
| A01 | Broken Access Control | JWT configurado, `[Authorize]` disponible |
| A02 | Security Misconfiguration | Cabeceras de seguridad, HSTS, Swagger solo en dev |
| A03 | Software Supply Chain | Dependencias NuGet oficiales |
| A04 | Cryptographic Failures | HTTPS, TLS en BD |
| A05 | Injection | EF Core parametrizado, validación DTOs, rate limiting |
| A06 | Insecure Design | Arquitectura en capas |
| A07 | Authentication Failures | JWT, rate limiting en auth |
| A08 | Software/Data Integrity | Buenas prácticas en CI/CD |
| A09 | Security Logging | Logging estructurado de excepciones |
| A10 | Mishandling of Exceptions | Global exception handler |

### Reportar vulnerabilidades

Si encuentras una vulnerabilidad de seguridad. Contacta al equipo de desarrollo de forma privada.
