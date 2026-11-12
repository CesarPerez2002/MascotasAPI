using Domain.Enums;

namespace Application.DTOs;

public class MascotaRespuestaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int EspecieId { get; set; }

    public string? Especie { get; set; }

    public string Raza { get; set; } = string.Empty;

    public int EdadMeses { get; set; }

    public Sexo Sexo { get; set; }

    public Tamano Tamano { get; set; }

    public bool Vacunada { get; set; }

    public bool Esterilizada { get; set; }

    public string Historia { get; set; } = string.Empty;

    public EstadoMascota Estado { get; set; }

    public string? FotoRuta { get; set; }

    public string? VideoRuta { get; set; }
}