package com.ucenm.mascotascrud;

import android.content.Context;

import com.google.gson.JsonElement;
import com.google.gson.JsonObject;
import com.google.gson.JsonParser;

import java.io.IOException;
import java.util.List;

import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;

/** Repository: unico punto de acceso a la API desde la UI. */
final class Repositorio {

    interface Resultado<T> {
        void exito(T datos);

        void error(String mensaje, boolean reintentar);
    }

    private Repositorio() {
    }

    static Call<Modelos.Pagina> listar(Context c, String texto, int pagina, int especieId) {
        return ApiClient.servicio(c).listar(
                (texto == null || texto.trim().isEmpty()) ? null : texto.trim(),
                pagina,
                20,
                especieId <= 0 ? null : especieId);
    }

    static Call<Modelos.Mascota> detalle(Context c, int id) {
        return ApiClient.servicio(c).detalle(id);
    }

    static Call<Modelos.Mascota> crear(Context c, Modelos.CrearMascota datos) {
        return ApiClient.servicio(c).crear(datos);
    }

    static Call<Void> editar(Context c, int id, Modelos.ActualizarMascota datos) {
        return ApiClient.servicio(c).editar(id, datos);
    }

    static Call<Void> eliminar(Context c, int id) {
        return ApiClient.servicio(c).eliminar(id);
    }

    static Call<List<Modelos.Especie>> especies(Context c) {
        return ApiClient.servicio(c).especies();
    }

    static Call<Void> subirImagen(Context c, int id, String nombre, String base64) {
        return ApiClient.servicio(c).subirImagen(id, new Modelos.SubirImagen(nombre, base64));
    }

    static Call<ResponseBody> obtenerImagen(Context c, int id) {
        return ApiClient.servicio(c).obtenerImagen(id);
    }

    static Call<Void> eliminarImagen(Context c, int id) {
        return ApiClient.servicio(c).eliminarImagen(id);
    }

    static Call<Void> subirVideo(Context c, int id, okhttp3.MultipartBody.Part archivo) {
        return ApiClient.servicio(c).subirVideo(id, archivo);
    }

    static Call<Void> eliminarVideo(Context c, int id) {
        return ApiClient.servicio(c).eliminarVideo(id);
    }

    static <T> void ejecutar(Call<T> peticion, Resultado<T> resultado) {
        peticion.enqueue(new Callback<T>() {
            @Override
            public void onResponse(Call<T> llamada, Response<T> respuesta) {
                if (llamada.isCanceled()) {
                    return;
                }
                if (respuesta.isSuccessful()) {
                    resultado.exito(respuesta.body());
                } else {
                    resultado.error(mensajeHttp(respuesta), respuesta.code() >= 500);
                }
            }

            @Override
            public void onFailure(Call<T> llamada, Throwable error) {
                if (llamada.isCanceled()) {
                    return;
                }
                resultado.error(
                        "No se pudo conectar con MascotasAPI. "
                                + "Comprueba que el servidor esté encendido.",
                        true);
            }
        });
    }

    private static String mensajeHttp(Response<?> respuesta) {
        switch (respuesta.code()) {
            case 400:
                return "Los datos enviados no son válidos." + detalleError(respuesta);
            case 404:
                return "No se encontró el registro solicitado." + detalleError(respuesta);
            case 409:
                return "Conflicto con el estado actual del registro." + detalleError(respuesta);
            case 413:
                return "El archivo es demasiado grande." + detalleError(respuesta);
            case 415:
                return "Formato de archivo no permitido." + detalleError(respuesta);
            default:
                return "MascotasAPI respondió con HTTP " + respuesta.code() + "." + detalleError(respuesta);
        }
    }

    private static String detalleError(Response<?> respuesta) {
        try (ResponseBody cuerpo = respuesta.errorBody()) {
            if (cuerpo == null) {
                return "";
            }
            String texto = cuerpo.string().trim();
            if (texto.isEmpty() || texto.startsWith("<") || texto.length() > 2000) {
                return "";
            }
            if (!texto.startsWith("{")) {
                return "\n" + texto;
            }
            JsonObject problema = JsonParser.parseString(texto).getAsJsonObject();
            StringBuilder salida = new StringBuilder();
            JsonElement detalle = problema.get("detail");
            if (detalle != null && detalle.isJsonPrimitive()) {
                salida.append('\n').append(detalle.getAsString());
            }
            JsonElement errores = problema.get("errors");
            if (errores != null && errores.isJsonObject()) {
                errores.getAsJsonObject().entrySet().forEach(campo -> {
                    if (campo.getValue().isJsonArray()) {
                        campo.getValue().getAsJsonArray().forEach(e ->
                                salida.append('\n').append(e.getAsString()));
                    }
                });
            }
            return salida.toString();
        } catch (IOException | RuntimeException ignorado) {
            return "";
        }
    }
}
