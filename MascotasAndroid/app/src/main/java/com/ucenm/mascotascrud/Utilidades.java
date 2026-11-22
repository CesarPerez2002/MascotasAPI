package com.ucenm.mascotascrud;

/** Traduce los enums numéricos de la API a texto para la interfaz. */
final class Utilidades {

    private Utilidades() {
    }

    static String sexo(int valor) {
        switch (valor) {
            case 1:
                return "Hembra";
            default:
                return "Macho";
        }
    }

    static String tamano(int valor) {
        switch (valor) {
            case 1:
                return "Mediano";
            case 2:
                return "Grande";
            default:
                return "Pequeño";
        }
    }

    static String estado(int valor) {
        switch (valor) {
            case 1:
                return "Reservada";
            case 2:
                return "Adoptada";
            default:
                return "Disponible";
        }
    }
}
