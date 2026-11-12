
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class MascotaRepository : IMascotaRepository
{
    private readonly MascotasDbContext _context;

    public MascotaRepository(MascotasDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Mascota> Mascotas, int TotalRegistros)> ObtenerPaginadasAsync(
        string? search,
        int page,
        int pageSize,
        int? especieId)
    {
        var consulta = _context.Mascotas
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            consulta = consulta.Where(m =>
                m.Nombre.Contains(search) ||
                m.Raza.Contains(search));
        }

        if (especieId.HasValue)
        {
            consulta = consulta.Where(m => m.EspecieId == especieId.Value);
        }

        int totalRegistros = await consulta.CountAsync();

        var mascotas = await consulta
            .OrderBy(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (mascotas, totalRegistros);
    }

    public async Task<Mascota?> ObtenerPorIdAsync(int id)
    {
        return await _context.Mascotas
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> ExisteEspecieAsync(int especieId)
    {
        return await _context.Especies
            .AnyAsync(e => e.Id == especieId);
    }

    public async Task AgregarAsync(Mascota mascota)
    {
        await _context.Mascotas.AddAsync(mascota);
    }

    public Task EliminarAsync(Mascota mascota)
    {
        _context.Mascotas.Remove(mascota);
        return Task.CompletedTask;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
