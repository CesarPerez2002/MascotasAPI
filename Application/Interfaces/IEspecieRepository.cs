using Domain.Entities;

namespace Application.Interfaces;

public interface IEspecieRepository
{
    Task<List<Especie>> ObtenerTodasAsync();
}