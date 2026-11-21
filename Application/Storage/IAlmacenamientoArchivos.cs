namespace Application.Storage;

public interface IAlmacenamientoArchivos
{
    Task<string> GuardarAsync(
        Stream archivo,
        string nombreArchivo,
        string carpeta);

    Task EliminarAsync(string? ruta);

    string ObtenerRutaFisica(string ruta);
}
