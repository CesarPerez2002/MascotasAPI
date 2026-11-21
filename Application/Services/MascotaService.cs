using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Application.Storage;
using Domain.Entities;

namespace Application.Services;

public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _repository;
    private readonly IAlmacenamientoArchivos _almacenamiento;

    private const int MaxImagenBytes = 5 * 1024 * 1024;
    private const int MaxVideoBytes = 30 * 1024 * 1024;

    public MascotaService(
        IMascotaRepository repository,
        IAlmacenamientoArchivos almacenamiento)
    {
        _repository = repository;
        _almacenamiento = almacenamiento;
    }

    public async Task<(List<MascotaRespuestaDto> Mascotas, int TotalRegistros)> ObtenerTodasAsync(
        string? search,
        int page,
        int pageSize,
        int? especieId)
    {
        if (page < 1)
            throw new ArgumentException("La página debe ser mayor que cero.");

        if (pageSize < 1 || pageSize > 100)
            throw new ArgumentException("El tamaño de página debe estar entre 1 y 100.");

        if (especieId.HasValue && especieId.Value < 1)
            throw new ArgumentException("El filtro de especie no es válido.");

        var resultado = await _repository.ObtenerPaginadasAsync(
            search, page, pageSize, especieId);

        var mascotasDto = resultado.Mascotas
            .Select(ConvertirARespuesta)
            .ToList();

        return (mascotasDto, resultado.TotalRegistros);
    }

    public async Task<MascotaRespuestaDto?> ObtenerPorIdAsync(int id)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null)
            return null;

        return ConvertirARespuesta(mascota);
    }

    public async Task<MascotaRespuestaDto> CrearAsync(CrearMascotaDto dto)
    {
        if (!await _repository.ExisteEspecieAsync(dto.EspecieId))
            throw new ArgumentException("La especie seleccionada no existe.");

        ValidarEnumeraciones(dto.Sexo, dto.Tamano);

        var mascota = new Mascota
        {
            Nombre = dto.Nombre.Trim(),
            EspecieId = dto.EspecieId,
            Raza = string.IsNullOrWhiteSpace(dto.Raza)
                ? "Mestizo"
                : dto.Raza.Trim(),
            EdadMeses = dto.EdadMeses,
            Sexo = dto.Sexo,
            Tamano = dto.Tamano,
            Vacunada = dto.Vacunada,
            Esterilizada = dto.Esterilizada,
            Historia = dto.Historia.Trim()
        };

        await _repository.AgregarAsync(mascota);
        await _repository.GuardarCambiosAsync();

        return ConvertirARespuesta(mascota);
    }

    public async Task<bool> ActualizarAsync(int id, ActualizarMascotaDto dto)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null)
            return false;

        if (!await _repository.ExisteEspecieAsync(dto.EspecieId))
            throw new ArgumentException("La especie seleccionada no existe.");

        ValidarEnumeraciones(dto.Sexo, dto.Tamano);

        if (!Enum.IsDefined(typeof(Domain.Enums.EstadoMascota), dto.Estado))
            throw new ArgumentException("El estado seleccionado no es válido.");

        mascota.Nombre = dto.Nombre.Trim();
        mascota.EspecieId = dto.EspecieId;
        mascota.Raza = string.IsNullOrWhiteSpace(dto.Raza)
            ? "Mestizo"
            : dto.Raza.Trim();
        mascota.EdadMeses = dto.EdadMeses;
        mascota.Sexo = dto.Sexo;
        mascota.Tamano = dto.Tamano;
        mascota.Vacunada = dto.Vacunada;
        mascota.Esterilizada = dto.Esterilizada;
        mascota.Historia = dto.Historia.Trim();

        // Las rutas de imagen y video se conservan.
        mascota.CambiarEstado(dto.Estado);

        await _repository.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null)
            return false;

        var fotoRuta = mascota.FotoRuta;
        var videoRuta = mascota.VideoRuta;

        await _repository.EliminarAsync(mascota);
        await _repository.GuardarCambiosAsync();

        await _almacenamiento.EliminarAsync(fotoRuta);
        await _almacenamiento.EliminarAsync(videoRuta);

        return true;
    }

    // SUBIR IMAGEN EN BASE64

    public async Task<bool> SubirImagenBase64Async(
        int id,
        SubirImagenBase64Dto dto)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null)
            return false;

        if (string.IsNullOrWhiteSpace(dto.NombreArchivo))
            throw new ArgumentException("El nombre del archivo es obligatorio.");

        if (string.IsNullOrWhiteSpace(dto.ImagenBase64))
            throw new ArgumentException("La imagen Base64 es obligatoria.");

        int caracteresBase64 = dto.ImagenBase64.Count(
            c => !char.IsWhiteSpace(c));

        int maxCaracteres = ((MaxImagenBytes + 2) / 3) * 4;

        if (caracteresBase64 > maxCaracteres)
            throw new ArchivoDemasiadoGrandeException(
                "La imagen no puede superar los 5 MB.");

        // Convertir Base64 a bytes.
        byte[] bytes = Convert.FromBase64String(dto.ImagenBase64);

        if (bytes.Length > MaxImagenBytes)
            throw new ArchivoDemasiadoGrandeException(
                "La imagen no puede superar los 5 MB.");

        string extension = Path.GetExtension(dto.NombreArchivo)
            .ToLowerInvariant();

        ValidarImagen(bytes, extension);

        var rutaAnterior = mascota.FotoRuta;

        using var memoria = new MemoryStream(bytes);

        string nuevaRuta = await _almacenamiento.GuardarAsync(
            memoria,
            "imagen" + extension,
            $"mascotas/{id}/imagenes");

        mascota.FotoRuta = nuevaRuta;

        try
        {
            await _repository.GuardarCambiosAsync();
        }
        catch
        {
            await _almacenamiento.EliminarAsync(nuevaRuta);
            throw;
        }

        // Eliminar la fotografía anterior después de guardar la nueva.
        await _almacenamiento.EliminarAsync(rutaAnterior);

        return true;
    }

    // OBTENER IMAGEN

    public async Task<(string RutaFisica, string ContentType)?> ObtenerImagenAsync(int id)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null || string.IsNullOrWhiteSpace(mascota.FotoRuta))
            return null;

        string extension = Path.GetExtension(mascota.FotoRuta)
            .ToLowerInvariant();

        string? contentType = extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };

        if (contentType == null)
            return null;

        string rutaFisica = _almacenamiento.ObtenerRutaFisica(
            mascota.FotoRuta);

        if (!File.Exists(rutaFisica))
            return null;

        return (rutaFisica, contentType);
    }

    // ELIMINAR IMAGEN

    public async Task<bool> EliminarImagenAsync(int id)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null || string.IsNullOrWhiteSpace(mascota.FotoRuta))
            return false;

        string rutaAnterior = mascota.FotoRuta;

        mascota.FotoRuta = null;

        await _repository.GuardarCambiosAsync();

        await _almacenamiento.EliminarAsync(rutaAnterior);

        return true;
    }

    // SUBIR VIDEO MP4

    public async Task<bool> SubirVideoAsync(
        int id,
        Stream archivo,
        string nombreArchivo,
        long longitud)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null)
            return false;

        if (longitud <= 0)
            throw new ArgumentException("El video está vacío.");

        if (longitud > MaxVideoBytes)
            throw new ArchivoDemasiadoGrandeException(
                "El video no puede superar los 30 MB.");

        if (!string.Equals(
            Path.GetExtension(nombreArchivo),
            ".mp4",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatoArchivoNoSoportadoException(
                "Solamente se permiten videos MP4.");
        }

        // Copiar el archivo comprobando también su tamaño real.
        await using var memoria = new MemoryStream();

        byte[] buffer = new byte[81920];
        long totalBytes = 0;
        int leidos;

        while ((leidos = await archivo.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
        {
            totalBytes += leidos;

            if (totalBytes > MaxVideoBytes)
                throw new ArchivoDemasiadoGrandeException(
                    "El video no puede superar los 30 MB.");

            await memoria.WriteAsync(buffer.AsMemory(0, leidos));
        }

        byte[] bytes = memoria.ToArray();

        // Verificar que realmente sea un archivo MP4.
        if (bytes.Length < 12 ||
            bytes[4] != (byte)'f' ||
            bytes[5] != (byte)'t' ||
            bytes[6] != (byte)'y' ||
            bytes[7] != (byte)'p')
        {
            throw new FormatoArchivoNoSoportadoException(
                "El archivo seleccionado no contiene un formato MP4 válido.");
        }

        var rutaAnterior = mascota.VideoRuta;

        memoria.Position = 0;

        string nuevaRuta = await _almacenamiento.GuardarAsync(
            memoria,
            "video.mp4",
            $"mascotas/{id}/videos");

        mascota.VideoRuta = nuevaRuta;

        try
        {
            await _repository.GuardarCambiosAsync();
        }
        catch
        {
            await _almacenamiento.EliminarAsync(nuevaRuta);
            throw;
        }

        await _almacenamiento.EliminarAsync(rutaAnterior);

        return true;
    }

    // OBTENER VIDEO

    public async Task<string?> ObtenerVideoAsync(int id)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null || string.IsNullOrWhiteSpace(mascota.VideoRuta))
            return null;

        string rutaFisica = _almacenamiento.ObtenerRutaFisica(
            mascota.VideoRuta);

        if (!File.Exists(rutaFisica))
            return null;

        return rutaFisica;
    }

    // ELIMINAR VIDEO

    public async Task<bool> EliminarVideoAsync(int id)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null || string.IsNullOrWhiteSpace(mascota.VideoRuta))
            return false;

        string rutaAnterior = mascota.VideoRuta;

        mascota.VideoRuta = null;

        await _repository.GuardarCambiosAsync();

        await _almacenamiento.EliminarAsync(rutaAnterior);

        return true;
    }

    // VALIDAR FORMATO REAL DE IMAGEN

    private static void ValidarImagen(byte[] bytes, string extension)
    {
        bool jpeg = bytes.Length >= 3 &&
                    bytes[0] == 0xFF &&
                    bytes[1] == 0xD8 &&
                    bytes[2] == 0xFF;

        bool png = bytes.Length >= 8 &&
                   bytes[0] == 0x89 &&
                   bytes[1] == 0x50 &&
                   bytes[2] == 0x4E &&
                   bytes[3] == 0x47 &&
                   bytes[4] == 0x0D &&
                   bytes[5] == 0x0A &&
                   bytes[6] == 0x1A &&
                   bytes[7] == 0x0A;

        bool webp = bytes.Length >= 12 &&
                    bytes[0] == (byte)'R' &&
                    bytes[1] == (byte)'I' &&
                    bytes[2] == (byte)'F' &&
                    bytes[3] == (byte)'F' &&
                    bytes[8] == (byte)'W' &&
                    bytes[9] == (byte)'E' &&
                    bytes[10] == (byte)'B' &&
                    bytes[11] == (byte)'P';

        bool extensionValida = extension switch
        {
            ".jpg" or ".jpeg" => jpeg,
            ".png" => png,
            ".webp" => webp,
            _ => false
        };

        if (!extensionValida)
        {
            throw new FormatoArchivoNoSoportadoException(
                "La imagen debe ser JPG, PNG o WebP y coincidir con su extensión.");
        }
    }

    private static void ValidarEnumeraciones(
        Domain.Enums.Sexo sexo,
        Domain.Enums.Tamano tamano)
    {
        if (!Enum.IsDefined(typeof(Domain.Enums.Sexo), sexo))
            throw new ArgumentException("El sexo seleccionado no es válido.");

        if (!Enum.IsDefined(typeof(Domain.Enums.Tamano), tamano))
            throw new ArgumentException("El tamaño seleccionado no es válido.");
    }

    private static MascotaRespuestaDto ConvertirARespuesta(Mascota mascota)
    {
        return new MascotaRespuestaDto
        {
            Id = mascota.Id,
            Nombre = mascota.Nombre,
            EspecieId = mascota.EspecieId,
            Raza = mascota.Raza,
            EdadMeses = mascota.EdadMeses,
            Sexo = mascota.Sexo,
            Tamano = mascota.Tamano,
            Vacunada = mascota.Vacunada,
            Esterilizada = mascota.Esterilizada,
            Historia = mascota.Historia,
            Estado = mascota.Estado,
            FotoRuta = mascota.FotoRuta,
            VideoRuta = mascota.VideoRuta
        };
    }
}
