namespace Domain.Entities;

public class Especie
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
}