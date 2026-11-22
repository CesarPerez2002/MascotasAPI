package com.ucenm.mascotascrud;

import android.content.Context;
import android.content.SharedPreferences;
import android.util.Log;

import java.net.Proxy;
import java.util.concurrent.TimeUnit;

import okhttp3.OkHttpClient;
import okhttp3.logging.HttpLoggingInterceptor;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;

/**
 * Cliente Retrofit / OkHttp.
 *
 * La URL por defecto es http://192.168.56.1:5176/ (IP fija del host), que no
 * requiere adb reverse. La API debe escuchar en 0.0.0.0:5176.
 * El boton de la pantalla permite probar http://10.0.2.2:5176/ como alternativa.
 */
final class ApiClient {

    private static final String PREFS = "mascotas_api_prefs";
    private static final String CLAVE_EMULADOR = "usar_10_0_2_2";
    private static final Object BLOQUEO = new Object();

    private static Retrofit retrofit;
    private static ApiServicio servicio;

    private ApiClient() {
    }

    static String baseUrl(Context context) {
        return usandoEmulador(context)
                ? BuildConfig.API_BASE_URL_EMULADOR
                : BuildConfig.API_BASE_URL;
    }

    static boolean usandoEmulador(Context context) {
        Context app = context.getApplicationContext();
        return app.getSharedPreferences(PREFS, Context.MODE_PRIVATE)
                .getBoolean(CLAVE_EMULADOR, false);
    }

    static void usarEmulador(Context context, boolean emulador) {
        Context app = context.getApplicationContext();
        SharedPreferences.Editor editor = app.getSharedPreferences(PREFS, Context.MODE_PRIVATE).edit();
        editor.putBoolean(CLAVE_EMULADOR, emulador);
        editor.apply();
        synchronized (BLOQUEO) {
            retrofit = null;
            servicio = null;
        }
    }

    static ApiServicio servicio(Context context) {
        synchronized (BLOQUEO) {
            if (servicio == null) {
                construir(context);
            }
            return servicio;
        }
    }

    private static void construir(Context context) {
        HttpLoggingInterceptor registro = new HttpLoggingInterceptor(mensaje -> {
            if (BuildConfig.DEBUG) {
                Log.d("MascotasAPI", mensaje);
            }
        });
        registro.setLevel(BuildConfig.DEBUG
                ? HttpLoggingInterceptor.Level.BASIC
                : HttpLoggingInterceptor.Level.NONE);

        OkHttpClient cliente = new OkHttpClient.Builder()
                .connectTimeout(20, TimeUnit.SECONDS)
                .readTimeout(30, TimeUnit.SECONDS)
                .writeTimeout(30, TimeUnit.SECONDS)
                .callTimeout(60, TimeUnit.SECONDS)
                .retryOnConnectionFailure(true)
                // Evita que OkHttp tome un proxy del sistema que no existe en el emulador.
                .proxy(Proxy.NO_PROXY)
                .addInterceptor(registro)
                .build();

        retrofit = new Retrofit.Builder()
                .baseUrl(baseUrl(context))
                .client(cliente)
                .addConverterFactory(GsonConverterFactory.create())
                .build();

        servicio = retrofit.create(ApiServicio.class);
        Log.i("MascotasAPI", "Base URL: " + baseUrl(context));
    }
}
