using Application.Storage;
using Microsoft.AspNetCore.Hosting;

namespace Infrastructure.Storage;

public class AlmacenamientoArchivos : IAlmacenamientoArchivos
{
    private readonly string _directorioBase;

    public AlmacenamientoArchivos(IWebHostEnvironment environment)
    {
        _directorioBase = Path.Combine(
            environment.ContentRootPath,
            "Archivos"
        );
    }

    public async Task<string> GuardarAsync(
        Stream archivo,
        string nombreArchivo,
        string carpeta)
    {
        var directorio = Path.Combine(_directorioBase, carpeta);

        Directory.CreateDirectory(directorio);

        var extension = Path.GetExtension(nombreArchivo);

        var nombreGenerado = $"{Guid.NewGuid()}{extension}";

        var rutaCompleta = Path.Combine(directorio, nombreGenerado);

        await using var stream = new FileStream(
            rutaCompleta,
            FileMode.CreateNew,
            FileAccess.Write
        );

        await archivo.CopyToAsync(stream);

        return Path.Combine(carpeta, nombreGenerado)
            .Replace("\\", "/");
    }

    public Task EliminarAsync(string? ruta)
    {
        if (string.IsNullOrWhiteSpace(ruta))
            return Task.CompletedTask;

        var rutaFisica = ObtenerRutaFisica(ruta);

        if (File.Exists(rutaFisica))
            File.Delete(rutaFisica);

        return Task.CompletedTask;
    }

    public string ObtenerRutaFisica(string ruta)
    {
        var rutaNormalizada = ruta.Replace(
            '/',
            Path.DirectorySeparatorChar
        );

        var rutaCompleta = Path.GetFullPath(
            Path.Combine(_directorioBase, rutaNormalizada)
        );

        var directorioBaseCompleto = Path.GetFullPath(_directorioBase)
            + Path.DirectorySeparatorChar;

        if (!rutaCompleta.StartsWith(
            directorioBaseCompleto,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("La ruta del archivo no es válida.");
        }

        return rutaCompleta;
    }
}
