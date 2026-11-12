using Application.DTOs;

namespace Application.Interfaces;

public interface IMascotaService
{
    Task<(List<MascotaRespuestaDto> Mascotas, int TotalRegistros)> ObtenerTodasAsync(
        string? search,
        int page,
        int pageSize,
        int? especieId);

    Task<MascotaRespuestaDto?> ObtenerPorIdAsync(int id);

    Task<MascotaRespuestaDto> CrearAsync(CrearMascotaDto dto);

    Task<bool> ActualizarAsync(int id, ActualizarMascotaDto dto);

    Task<bool> EliminarAsync(int id);
}