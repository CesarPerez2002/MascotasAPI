using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EspeciesController : ControllerBase
{
    private readonly IEspecieRepository _repository;

    public EspeciesController(IEspecieRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerEspecies()
    {
        var especies = await _repository.ObtenerTodasAsync();

        return Ok(especies);
    }
}