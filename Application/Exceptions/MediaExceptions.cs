namespace Application.Exceptions;

public class ArchivoDemasiadoGrandeException : Exception
{
    public ArchivoDemasiadoGrandeException(string mensaje)
        : base(mensaje)
    {
    }
}

public class FormatoArchivoNoSoportadoException : Exception
{
    public FormatoArchivoNoSoportadoException(string mensaje)
        : base(mensaje)
    {
    }
}