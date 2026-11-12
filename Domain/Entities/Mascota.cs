using Domain.Enums;

namespace Domain.Entities;

public class Mascota
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public int EspecieId { get; set; }

    public string Raza { get; set; } = "Mestizo";

    public int EdadMeses { get; set; }

    public Sexo Sexo { get; set; }

    public Tamano Tamano { get; set; }

    public bool Vacunada { get; set; }

    public bool Esterilizada { get; set; }

    public string Historia { get; set; } = string.Empty;

    public EstadoMascota Estado { get; set; } = EstadoMascota.Disponible;

    public string? FotoRuta { get; set; }

    public string? VideoRuta { get; set; }

    // Método para cambiar el estado de la mascota
    public void CambiarEstado(EstadoMascota nuevoEstado)
    {
        if (Estado == EstadoMascota.Adoptada &&
            nuevoEstado != EstadoMascota.Adoptada)
        {
            throw new InvalidOperationException(
                "Una mascota adoptada no puede cambiar a otro estado."
            );
        }

        Estado = nuevoEstado;
    }
}
