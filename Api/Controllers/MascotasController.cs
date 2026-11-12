using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaService _service;

    public MascotasController(IMascotaService service)
    {
        _service = service;
    }

    // GET: api/Mascotas
    [HttpGet]
    public async Task<IActionResult> ObtenerTodas(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? especieId = null)
    {
        if (page < 1)
            return BadRequest("La página debe ser mayor que cero.");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("El tamaño de página debe estar entre 1 y 100.");

        if (especieId.HasValue && especieId.Value < 1)
            return BadRequest("El filtro de especie no es válido.");

        var resultado = await _service.ObtenerTodasAsync(
            search, page, pageSize, especieId);

        int totalPaginas = (int)Math.Ceiling(
            (double)resultado.TotalRegistros / pageSize);

        return Ok(new
        {
            items = resultado.Mascotas,
            page,
            pageSize,
            totalRegistros = resultado.TotalRegistros,
            totalPaginas
        });
    }

    // GET: api/Mascotas/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        var mascota = await _service.ObtenerPorIdAsync(id);

        if (mascota == null)
            return NotFound("No se encontró la mascota solicitada.");

        return Ok(mascota);
    }

    // POST: api/Mascotas
    [HttpPost]
    public async Task<IActionResult> Crear(CrearMascotaDto dto)
    {
        var mascota = await _service.CrearAsync(dto);

        return CreatedAtAction(
            nameof(ObtenerPorId),
            new { id = mascota.Id },
            mascota);
    }

    // PUT: api/Mascotas/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(
        int id,
        ActualizarMascotaDto dto)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        var actualizado = await _service.ActualizarAsync(id, dto);

        if (!actualizado)
            return NotFound("No se encontró la mascota que desea actualizar.");

        return NoContent();
    }

    // DELETE: api/Mascotas/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        var eliminado = await _service.EliminarAsync(id);

        if (!eliminado)
            return NotFound("No se encontró la mascota que desea eliminar.");

        return NoContent();
    }
}
