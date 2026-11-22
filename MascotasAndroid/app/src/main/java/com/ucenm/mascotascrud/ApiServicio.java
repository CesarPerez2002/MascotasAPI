package com.ucenm.mascotascrud;

import java.util.List;

import okhttp3.MultipartBody;
import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.http.Body;
import retrofit2.http.DELETE;
import retrofit2.http.GET;
import retrofit2.http.Multipart;
import retrofit2.http.PUT;
import retrofit2.http.Part;
import retrofit2.http.Path;
import retrofit2.http.Query;

/** Contrato HTTP: coincide con MascotasController y EspeciesController. */
interface ApiServicio {

    @GET("api/Mascotas")
    Call<Modelos.Pagina> listar(
            @Query("search") String search,
            @Query("page") int page,
            @Query("pageSize") int pageSize,
            @Query("especieId") Integer especieId);

    @GET("api/Mascotas/{id}")
    Call<Modelos.Mascota> detalle(@Path("id") int id);

    @retrofit2.http.POST("api/Mascotas")
    Call<Modelos.Mascota> crear(@Body Modelos.CrearMascota datos);

    @PUT("api/Mascotas/{id}")
    Call<Void> editar(@Path("id") int id, @Body Modelos.ActualizarMascota datos);

    @DELETE("api/Mascotas/{id}")
    Call<Void> eliminar(@Path("id") int id);

    @GET("api/Especies")
    Call<List<Modelos.Especie>> especies();

    @PUT("api/Mascotas/{id}/imagen-base64")
    Call<Void> subirImagen(@Path("id") int id, @Body Modelos.SubirImagen datos);

    @GET("api/Mascotas/{id}/imagen")
    Call<ResponseBody> obtenerImagen(@Path("id") int id);

    @DELETE("api/Mascotas/{id}/imagen")
    Call<Void> eliminarImagen(@Path("id") int id);

    @Multipart
    @PUT("api/Mascotas/{id}/video")
    Call<Void> subirVideo(@Path("id") int id, @Part MultipartBody.Part archivo);

    @GET("api/Mascotas/{id}/video")
    Call<ResponseBody> obtenerVideo(@Path("id") int id);

    @DELETE("api/Mascotas/{id}/video")
    Call<Void> eliminarVideo(@Path("id") int id);
}
