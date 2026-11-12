using Domain.Entities;

namespace Application.Interfaces;

public interface IMascotaRepository
{
    Task<(List<Mascota> Mascotas, int TotalRegistros)> ObtenerPaginadasAsync(
        string? search,
        int page,
        int pageSize,
        int? especieId);

    Task<Mascota?> ObtenerPorIdAsync(int id);

    Task<bool> ExisteEspecieAsync(int especieId);

    Task AgregarAsync(Mascota mascota);

    Task EliminarAsync(Mascota mascota);

    Task GuardarCambiosAsync();
}