package com.ucenm.mascotascrud;

import android.content.ContentResolver;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.provider.OpenableColumns;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import androidx.activity.result.ActivityResultLauncher;
import androidx.activity.result.contract.ActivityResultContracts;
import androidx.appcompat.app.AlertDialog;
import androidx.appcompat.app.AppCompatActivity;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;

import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.RequestBody;
import okhttp3.ResponseBody;

/** Detalle de la mascota: edición, borrado, imagen Base64 y video MP4. */
public class DetalleActivity extends AppCompatActivity {

    private static final int CODIGO_EDITAR = 2;

    private Modelos.Mascota mascota;
    private ActivityResultLauncher<String> selectorImagen;
    private ActivityResultLauncher<String> selectorVideo;
    private ImageView vistaFoto;
    private ProgressBar cargando;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_detalle);

        mascota = (Modelos.Mascota) getIntent().getSerializableExtra("mascota");
        if (mascota == null) {
            finish();
            return;
        }
        setTitle(mascota.nombre);

        vistaFoto = findViewById(R.id.detalleFoto);
        cargando = findViewById(R.id.detalleCargando);

        selectorImagen = registerForActivityResult(
                new ActivityResultContracts.GetContent(), uri -> {
                    if (uri != null) {
                        subirImagen(uri);
                    }
                });
        selectorVideo = registerForActivityResult(
                new ActivityResultContracts.GetContent(), uri -> {
                    if (uri != null) {
                        subirVideo(uri);
                    }
                });

        findViewById(R.id.botonEditar).setOnClickListener(v -> {
            Intent intento = new Intent(this, FormularioActivity.class);
            intento.putExtra("mascota", mascota);
            startActivityForResult(intento, CODIGO_EDITAR);
        });

        findViewById(R.id.botonEliminar).setOnClickListener(v -> confirmarEliminacion());
        findViewById(R.id.botonSubirImagen).setOnClickListener(v -> selectorImagen.launch("image/*"));
        findViewById(R.id.botonSubirVideo).setOnClickListener(v -> selectorVideo.launch("video/*"));
        findViewById(R.id.botonVerVideo).setOnClickListener(v -> verVideo());

        findViewById(R.id.botonEliminarImagen).setOnClickListener(v ->
                borrarArchivo(Repositorio.eliminarImagen(this, mascota.id), "imagen"));
        findViewById(R.id.botonEliminarVideo).setOnClickListener(v ->
                borrarArchivo(Repositorio.eliminarVideo(this, mascota.id), "video"));

        pintar();
        cargarImagen();
    }

    private void pintar() {
        ((TextView) findViewById(R.id.detalleNombre)).setText(mascota.nombre);
        ((TextView) findViewById(R.id.detalleEspecie)).setText("Especie: " + mascota.especieId);
        ((TextView) findViewById(R.id.detalleRaza)).setText("Raza: " + mascota.raza);
        ((TextView) findViewById(R.id.detalleEdad)).setText("Edad: " + mascota.edadMeses + " meses");
        ((TextView) findViewById(R.id.detalleSexo)).setText("Sexo: " + Utilidades.sexo(mascota.sexo));
        ((TextView) findViewById(R.id.detalleTamano)).setText("Tamaño: " + Utilidades.tamano(mascota.tamano));
        ((TextView) findViewById(R.id.detalleVacunada)).setText(
                "Vacunada: " + (mascota.vacunada ? "Sí" : "No"));
        ((TextView) findViewById(R.id.detalleEsterilizada)).setText(
                "Esterilizada: " + (mascota.esterilizada ? "Sí" : "No"));
        ((TextView) findViewById(R.id.detalleEstado)).setText(
                "Estado: " + Utilidades.estado(mascota.estado));
        ((TextView) findViewById(R.id.detalleHistoria)).setText(mascota.historia);
    }

    private void cargarImagen() {
        Repositorio.ejecutar(Repositorio.obtenerImagen(this, mascota.id),
                new Repositorio.Resultado<ResponseBody>() {
                    @Override
                    public void exito(ResponseBody cuerpo) {
                        try (InputStream entrada = cuerpo.byteStream()) {
                            Bitmap bitmap = BitmapFactory.decodeStream(entrada);
                            if (bitmap != null) {
                                vistaFoto.setImageBitmap(bitmap);
                            }
                        } catch (IOException ignorado) {
                        }
                    }

                    @Override
                    public void error(String mensaje, boolean reintentar) {
                        // Sin fotografía: se conserva la imagen placeholder.
                    }
                });
    }

    private void subirImagen(Uri uri) {
        byte[] bytes = leerBytes(uri);
        if (bytes == null || bytes.length == 0) {
            Toast.makeText(this, "No se pudo leer la imagen.", Toast.LENGTH_SHORT).show();
            return;
        }
        if (bytes.length > 5 * 1024 * 1024) {
            Toast.makeText(this, "La imagen no puede superar los 5 MB.", Toast.LENGTH_LONG).show();
            return;
        }

        String tipo = getContentResolver().getType(uri);
        String extension;
        if (tipo != null && tipo.contains("png")) {
            extension = "png";
        } else if (tipo != null && tipo.contains("webp")) {
            extension = "webp";
        } else {
            extension = "jpg";
        }

        String base64 = android.util.Base64.encodeToString(bytes, android.util.Base64.NO_WRAP);
        cargando.setVisibility(View.VISIBLE);
        Repositorio.ejecutar(Repositorio.subirImagen(this, mascota.id, "foto." + extension, base64),
                new Repositorio.Resultado<Void>() {
                    @Override
                    public void exito(Void datos) {
                        cargando.setVisibility(View.GONE);
                        Toast.makeText(DetalleActivity.this, "Imagen guardada.", Toast.LENGTH_SHORT).show();
                        recargar();
                    }

                    @Override
                    public void error(String mensaje, boolean reintentar) {
                        cargando.setVisibility(View.GONE);
                        Toast.makeText(DetalleActivity.this, mensaje, Toast.LENGTH_LONG).show();
                    }
                });
    }

    private void subirVideo(Uri uri) {
        byte[] bytes = leerBytes(uri);
        if (bytes == null || bytes.length == 0) {
            Toast.makeText(this, "No se pudo leer el video.", Toast.LENGTH_SHORT).show();
            return;
        }
        if (bytes.length > 30 * 1024 * 1024) {
            Toast.makeText(this, "El video no puede superar los 30 MB.", Toast.LENGTH_LONG).show();
            return;
        }

        String nombre = nombreArchivo(uri);
        RequestBody cuerpo = RequestBody.create(bytes, MediaType.parse("video/mp4"));
        MultipartBody.Part parte = MultipartBody.Part.createFormData("archivo", nombre, cuerpo);

        cargando.setVisibility(View.VISIBLE);
        Repositorio.ejecutar(Repositorio.subirVideo(this, mascota.id, parte),
                new Repositorio.Resultado<Void>() {
                    @Override
                    public void exito(Void datos) {
                        cargando.setVisibility(View.GONE);
                        Toast.makeText(DetalleActivity.this, "Video guardado.", Toast.LENGTH_SHORT).show();
                        recargar();
                    }

                    @Override
                    public void error(String mensaje, boolean reintentar) {
                        cargando.setVisibility(View.GONE);
                        Toast.makeText(DetalleActivity.this, mensaje, Toast.LENGTH_LONG).show();
                    }
                });
    }

    private void borrarArchivo(retrofit2.Call<Void> peticion, String nombre) {
        Repositorio.ejecutar(peticion, new Repositorio.Resultado<Void>() {
            @Override
            public void exito(Void datos) {
                Toast.makeText(DetalleActivity.this, nombre + " eliminado.", Toast.LENGTH_SHORT).show();
                recargar();
            }

            @Override
            public void error(String mensaje, boolean reintentar) {
                Toast.makeText(DetalleActivity.this, mensaje, Toast.LENGTH_LONG).show();
            }
        });
    }

    private void verVideo() {
        String url = ApiClient.baseUrl(this) + "api/Mascotas/" + mascota.id + "/video";
        Intent intento = new Intent(Intent.ACTION_VIEW);
        intento.setDataAndType(Uri.parse(url), "video/mp4");
        try {
            startActivity(intento);
        } catch (Exception error) {
            Toast.makeText(this, "No hay una app para reproducir el video.", Toast.LENGTH_SHORT).show();
        }
    }

    private void confirmarEliminacion() {
        new AlertDialog.Builder(this)
                .setTitle(R.string.eliminar)
                .setMessage(R.string.confirmar_eliminar)
                .setNegativeButton(R.string.cancelar, null)
                .setPositiveButton(R.string.eliminar, (dialogo, cual) -> {
                    Repositorio.ejecutar(Repositorio.eliminar(this, mascota.id),
                            new Repositorio.Resultado<Void>() {
                                @Override
                                public void exito(Void datos) {
                                    Toast.makeText(DetalleActivity.this, "Mascota eliminada.",
                                            Toast.LENGTH_SHORT).show();
                                    setResult(RESULT_OK);
                                    finish();
                                }

                                @Override
                                public void error(String mensaje, boolean reintentar) {
                                    Toast.makeText(DetalleActivity.this, mensaje,
                                            Toast.LENGTH_LONG).show();
                                }
                            });
                })
                .show();
    }

    private void recargar() {
        Repositorio.ejecutar(Repositorio.detalle(this, mascota.id),
                new Repositorio.Resultado<Modelos.Mascota>() {
                    @Override
                    public void exito(Modelos.Mascota datos) {
                        if (datos != null) {
                            mascota = datos;
                            pintar();
                            cargarImagen();
                        }
                    }

                    @Override
                    public void error(String mensaje, boolean reintentar) {
                    }
                });
    }

    private byte[] leerBytes(Uri uri) {
        try (InputStream entrada = getContentResolver().openInputStream(uri)) {
            if (entrada == null) {
                return null;
            }
            ByteArrayOutputStream salida = new ByteArrayOutputStream();
            byte[] buffer = new byte[16384];
            int leidos;
            while ((leidos = entrada.read(buffer)) != -1) {
                salida.write(buffer, 0, leidos);
            }
            return salida.toByteArray();
        } catch (IOException error) {
            return null;
        }
    }

    private String nombreArchivo(Uri uri) {
        String nombre = "video.mp4";
        ContentResolver resolver = getContentResolver();
        try (Cursor cursor = resolver.query(uri, null, null, null, null)) {
            if (cursor != null && cursor.moveToFirst()) {
                int indice = cursor.getColumnIndex(OpenableColumns.DISPLAY_NAME);
                if (indice >= 0 && cursor.getString(indice) != null) {
                    nombre = cursor.getString(indice);
                }
            }
        } catch (Exception ignorado) {
        }
        return nombre;
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == CODIGO_EDITAR && resultCode == RESULT_OK) {
            recargar();
        }
    }
}
