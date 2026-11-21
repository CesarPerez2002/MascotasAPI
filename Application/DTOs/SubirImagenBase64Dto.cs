using System.ComponentModel.DataAnnotations;

namespace Application.DTOs;

public class SubirImagenBase64Dto
{
    [Required(ErrorMessage = "El nombre del archivo es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del archivo es demasiado largo.")]
    public string NombreArchivo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La imagen Base64 es obligatoria.")]
    public string ImagenBase64 { get; set; } = string.Empty;
}