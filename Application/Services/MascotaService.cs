
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _repository;

    public MascotaService(IMascotaRepository repository)
    {
        _repository = repository;
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
            Raza = string.IsNullOrWhiteSpace(dto.Raza) ? "Mestizo" : dto.Raza.Trim(),
            EdadMeses = dto.EdadMeses,
            Sexo = dto.Sexo,
            Tamano = dto.Tamano,
            Vacunada = dto.Vacunada,
            Esterilizada = dto.Esterilizada,
            Historia = dto.Historia.Trim(),
            FotoRuta = dto.FotoRuta,
            VideoRuta = dto.VideoRuta
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
        mascota.Raza = string.IsNullOrWhiteSpace(dto.Raza) ? "Mestizo" : dto.Raza.Trim();
        mascota.EdadMeses = dto.EdadMeses;
        mascota.Sexo = dto.Sexo;
        mascota.Tamano = dto.Tamano;
        mascota.Vacunada = dto.Vacunada;
        mascota.Esterilizada = dto.Esterilizada;
        mascota.Historia = dto.Historia.Trim();
        mascota.FotoRuta = dto.FotoRuta;
        mascota.VideoRuta = dto.VideoRuta;

        mascota.CambiarEstado(dto.Estado);

        await _repository.GuardarCambiosAsync();

        return true;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var mascota = await _repository.ObtenerPorIdAsync(id);

        if (mascota == null)
            return false;

        await _repository.EliminarAsync(mascota);
        await _repository.GuardarCambiosAsync();

        return true;
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
