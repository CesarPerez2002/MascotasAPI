using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EspecieRepository : IEspecieRepository
{
    private readonly MascotasDbContext _context;

    public EspecieRepository(MascotasDbContext context)
    {
        _context = context;
    }

    public async Task<List<Especie>> ObtenerTodasAsync()
    {
        return await _context.Especies
            .AsNoTracking()
            .OrderBy(e => e.Nombre)
            .ToListAsync();
    }
}