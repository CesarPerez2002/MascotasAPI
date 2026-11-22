package com.ucenm.mascotascrud;

import java.io.Serializable;
import java.util.List;

/**
 * Modelos que reflejan exactamente el JSON de la API (nombres en camelCase).
 * La API serializa los enums como numeros:
 * Sexo: 0 = Macho, 1 = Hembra
 * Tamano: 0 = Pequeno, 1 = Mediano, 2 = Grande
 * Estado: 0 = Disponible, 1 = Reservada, 2 = Adoptada
 */
final class Modelos {

    private Modelos() {
    }

    static final class Mascota implements Serializable {
        int id;
        String nombre;
        int especieId;
        String especie;
        String raza;
        int edadMeses;
        int sexo;
        int tamano;
        boolean vacunada;
        boolean esterilizada;
        String historia;
        int estado;
        String fotoRuta;
        String videoRuta;
    }

    static final class Pagina {
        List<Mascota> items;
        int page;
        int pageSize;
        int totalRegistros;
        int totalPaginas;
    }

    static final class Especie {
        int id;
        String nombre;
    }

    static final class CrearMascota {
        String nombre;
        int especieId;
        String raza;
        int edadMeses;
        int sexo;
        int tamano;
        boolean vacunada;
        boolean esterilizada;
        String historia;
    }

    static final class ActualizarMascota {
        String nombre;
        int especieId;
        String raza;
        int edadMeses;
        int sexo;
        int tamano;
        boolean vacunada;
        boolean esterilizada;
        String historia;
        int estado;
    }

    static final class SubirImagen {
        String nombreArchivo;
        String imagenBase64;

        SubirImagen(String nombreArchivo, String imagenBase64) {
            this.nombreArchivo = nombreArchivo;
            this.imagenBase64 = imagenBase64;
        }
    }
}
