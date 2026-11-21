using Application.DTOs;
using Application.Exceptions;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MascotasController : ControllerBase
{
    private readonly IMascotaService _service;

    private const long MaxVideoBytes = 30 * 1024 * 1024;

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

    // PUT: api/Mascotas/1/imagen-base64
    [HttpPut("{id:int}/imagen-base64")]
    public async Task<IActionResult> SubirImagenBase64(
        int id,
        [FromBody] SubirImagenBase64Dto dto)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        var guardada = await _service.SubirImagenBase64Async(id, dto);

        if (!guardada)
            return NotFound("No se encontró la mascota.");

        return NoContent();
    }

    // GET: api/Mascotas/1/imagen
    [HttpGet("{id:int}/imagen")]
    public async Task<IActionResult> ObtenerImagen(int id)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        var imagen = await _service.ObtenerImagenAsync(id);

        if (imagen == null)
            return NotFound("No se encontró la imagen de esta mascota.");

        return PhysicalFile(
            imagen.Value.RutaFisica,
            imagen.Value.ContentType);
    }

    // DELETE: api/Mascotas/1/imagen
    [HttpDelete("{id:int}/imagen")]
    public async Task<IActionResult> EliminarImagen(int id)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        var eliminada = await _service.EliminarImagenAsync(id);

        if (!eliminada)
            return NotFound("No se encontró la mascota o su imagen.");

        return NoContent();
    }

    // PUT: api/Mascotas/1/video
    [HttpPut("{id:int}/video")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(31 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 31 * 1024 * 1024)]
    public async Task<IActionResult> SubirVideo(
        int id,
        [FromForm] IFormFile? archivo)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        if (archivo == null || archivo.Length == 0)
            return BadRequest("Debe seleccionar un video.");

        if (archivo.Length > MaxVideoBytes)
            throw new ArchivoDemasiadoGrandeException(
                "El video no puede superar los 30 MB.");

        await using var stream = archivo.OpenReadStream();

        var guardado = await _service.SubirVideoAsync(
            id,
            stream,
            archivo.FileName,
            archivo.Length);

        if (!guardado)
            return NotFound("No se encontró la mascota.");

        return NoContent();
    }

    // GET: api/Mascotas/1/video
    [HttpGet("{id:int}/video")]
    public async Task<IActionResult> ObtenerVideo(int id)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        var rutaVideo = await _service.ObtenerVideoAsync(id);

        if (rutaVideo == null)
            return NotFound("No se encontró el video de esta mascota.");

        return PhysicalFile(
            rutaVideo,
            "video/mp4",
            enableRangeProcessing: true);
    }

    // DELETE: api/Mascotas/1/video
    [HttpDelete("{id:int}/video")]
    public async Task<IActionResult> EliminarVideo(int id)
    {
        if (id <= 0)
            return BadRequest("El ID debe ser mayor que cero.");

        var eliminado = await _service.EliminarVideoAsync(id);

        if (!eliminado)
            return NotFound("No se encontró la mascota o su video.");

        return NoContent();
    }
}
