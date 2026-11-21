
using Application.DTOs;

namespace Application.Interfaces;

public interface IMascotaService
{
    // Operaciones de mascotas
    Task<(List<MascotaRespuestaDto> Mascotas, int TotalRegistros)> ObtenerTodasAsync(
        string? search,
        int page,
        int pageSize,
        int? especieId);

    Task<MascotaRespuestaDto?> ObtenerPorIdAsync(int id);

    Task<MascotaRespuestaDto> CrearAsync(CrearMascotaDto dto);

    Task<bool> ActualizarAsync(int id, ActualizarMascotaDto dto);

    Task<bool> EliminarAsync(int id);

    // Operaciones de imágenes
    Task<bool> SubirImagenBase64Async(
        int id,
        SubirImagenBase64Dto dto);

    Task<(string RutaFisica, string ContentType)?> ObtenerImagenAsync(int id);

    Task<bool> EliminarImagenAsync(int id);

    // Operaciones de videos
    Task<bool> SubirVideoAsync(
        int id,
        Stream archivo,
        string nombreArchivo,
        long longitud);

    Task<string?> ObtenerVideoAsync(int id);

    Task<bool> EliminarVideoAsync(int id);
}
