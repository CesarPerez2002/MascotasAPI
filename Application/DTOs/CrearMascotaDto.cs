using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Application.DTOs;

public class CrearMascotaDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede superar los 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especie es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una especie válida.")]
    public int EspecieId { get; set; }

    [StringLength(50, ErrorMessage = "La raza no puede superar los 50 caracteres.")]
    public string Raza { get; set; } = "Mestizo";

    [Range(0, 300, ErrorMessage = "La edad debe estar entre 0 y 300 meses.")]
    public int EdadMeses { get; set; }

    public Sexo Sexo { get; set; }

    public Tamano Tamano { get; set; }

    public bool Vacunada { get; set; }

    public bool Esterilizada { get; set; }

    [Required(ErrorMessage = "La historia es obligatoria.")]
    [StringLength(1500, ErrorMessage = "La historia no puede superar los 1500 caracteres.")]
    public string Historia { get; set; } = string.Empty;

    public string? FotoRuta { get; set; }

    public string? VideoRuta { get; set; }
}