using System;
using System.Collections.Generic;

namespace ArcanoPizza_API.Data;

internal static class PostgresConnectionString
{
    public static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("La cadena de conexión está vacía.");

        value = value.Trim();

        if (value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return FromUri(value);
        }

        return value;
    }

    private static string FromUri(string uriString)
    {
        if (!Uri.TryCreate(uriString, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("DATABASE_URL no es un URI válido.");

        var (user, pass) = SplitUserInfo(uri.UserInfo);
        var query = ParseQuery(uri.Query);

        var port = uri.IsDefaultPort ? 5432 : uri.Port;
        var database = uri.AbsolutePath.Trim('/');

        if (string.IsNullOrWhiteSpace(database))
            throw new InvalidOperationException("DATABASE_URL no incluye el nombre de la base de datos.");

        var parts = new List<string>(12)
        {
            $"Host={uri.Host}",
            $"Port={port}",
            $"Database={database}",
            $"Username={user}",
            $"Password={pass}"
        };

        // Neon suele requerir TLS
        if (query.TryGetValue("sslmode", out var sslmode) && !string.IsNullOrWhiteSpace(sslmode))
            parts.Add($"Ssl Mode={ToTitleCase(sslmode)}");

        // Intentar mapear channel_binding si el provider lo soporta.
        if (query.TryGetValue("channel_binding", out var channelBinding) && !string.IsNullOrWhiteSpace(channelBinding))
            parts.Add($"Channel Binding={ToTitleCase(channelBinding)}");

        return string.Join(';', parts);
    }

    private static (string user, string pass) SplitUserInfo(string userInfo)
    {
        if (string.IsNullOrEmpty(userInfo))
            throw new InvalidOperationException("DATABASE_URL no incluye usuario/contraseña.");

        var idx = userInfo.IndexOf(':');
        if (idx < 0)
            throw new InvalidOperationException("DATABASE_URL no incluye contraseña.");

        var user = Uri.UnescapeDataString(userInfo[..idx]);
        var pass = Uri.UnescapeDataString(userInfo[(idx + 1)..]);

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            throw new InvalidOperationException("DATABASE_URL incluye usuario/contraseña inválidos.");

        return (user, pass);
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(query))
            return dict;

        var span = query.AsSpan();
        if (span.Length > 0 && span[0] == '?')
            span = span[1..];

        foreach (var pair in span.ToString().Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var eq = pair.IndexOf('=');
            if (eq <= 0)
                continue;

            var key = Uri.UnescapeDataString(pair[..eq]);
            var val = Uri.UnescapeDataString(pair[(eq + 1)..]);
            dict[key] = val;
        }

        return dict;
    }

    private static string ToTitleCase(string value)
    {
        value = value.Trim();
        if (value.Length == 0) return value;
        if (value.Length == 1) return value.ToUpperInvariant();
        return char.ToUpperInvariant(value[0]) + value[1..];
    }
}

