package com.ucenm.mascotascrud;

import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import java.util.ArrayList;
import java.util.List;

/** Formulario de creación y edición, con el catálogo en Spinner y validación. */
public class FormularioActivity extends AppCompatActivity {

    private EditText campoNombre;
    private EditText campoRaza;
    private EditText campoEdad;
    private EditText campoHistoria;
    private Spinner campoEspecie;
    private Spinner campoSexo;
    private Spinner campoTamano;
    private Spinner campoEstado;
    private CheckBox checkVacunada;
    private CheckBox checkEsterilizada;
    private ProgressBar formCargando;

    private final List<Modelos.Especie> especies = new ArrayList<>();
    private Modelos.Mascota original;
    private int especiePrevista = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_formulario);

        campoNombre = findViewById(R.id.campoNombre);
        campoRaza = findViewById(R.id.campoRaza);
        campoEdad = findViewById(R.id.campoEdad);
        campoHistoria = findViewById(R.id.campoHistoria);
        campoEspecie = findViewById(R.id.campoEspecie);
        campoSexo = findViewById(R.id.campoSexo);
        campoTamano = findViewById(R.id.campoTamano);
        campoEstado = findViewById(R.id.campoEstado);
        checkVacunada = findViewById(R.id.checkVacunada);
        checkEsterilizada = findViewById(R.id.checkEsterilizada);
        formCargando = findViewById(R.id.formCargando);

        original = (Modelos.Mascota) getIntent().getSerializableExtra("mascota");
        setTitle(original == null ? R.string.titulo_nueva : R.string.titulo_editar);

        if (original != null) {
            especiePrevista = original.especieId;
            campoNombre.setText(original.nombre);
            campoRaza.setText(original.raza);
            campoEdad.setText(String.valueOf(original.edadMeses));
            campoHistoria.setText(original.historia);
            campoSexo.setSelection(original.sexo);
            campoTamano.setSelection(original.tamano);
            campoEstado.setSelection(original.estado);
            checkVacunada.setChecked(original.vacunada);
            checkEsterilizada.setChecked(original.esterilizada);
        }

        cargarEspecies();

        findViewById(R.id.botonGuardar).setOnClickListener(v -> guardar());
        findViewById(R.id.botonCancelar).setOnClickListener(v -> finish());
    }

    private void cargarEspecies() {
        Repositorio.ejecutar(Repositorio.especies(this), new Repositorio.Resultado<List<Modelos.Especie>>() {
            @Override
            public void exito(List<Modelos.Especie> datos) {
                especies.clear();
                List<String> opciones = new ArrayList<>();
                opciones.add(getString(R.string.seleccione_especie));
                if (datos != null) {
                    for (Modelos.Especie especie : datos) {
                        especies.add(especie);
                        opciones.add(especie.nombre);
                    }
                }
                ArrayAdapter<String> adaptador = new ArrayAdapter<>(
                        FormularioActivity.this,
                        android.R.layout.simple_spinner_item,
                        opciones);
                adaptador.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
                campoEspecie.setAdapter(adaptador);
                if (especiePrevista > 0) {
                    for (int i = 0; i < especies.size(); i++) {
                        if (especies.get(i).id == especiePrevista) {
                            campoEspecie.setSelection(i + 1);
                            break;
                        }
                    }
                }
            }

            @Override
            public void error(String mensaje, boolean reintentar) {
                Toast.makeText(FormularioActivity.this, mensaje, Toast.LENGTH_LONG).show();
            }
        });
    }

    private boolean validar() {
        if (campoNombre.getText().toString().trim().isEmpty()) {
            campoNombre.setError(getString(R.string.datos_invalidos));
            return false;
        }
        if (campoEspecie.getSelectedItemPosition() <= 0) {
            Toast.makeText(this, "Seleccione una especie.", Toast.LENGTH_SHORT).show();
            return false;
        }
        if (campoHistoria.getText().toString().trim().isEmpty()) {
            campoHistoria.setError(getString(R.string.datos_invalidos));
            return false;
        }
        String textoEdad = campoEdad.getText().toString().trim();
        if (!textoEdad.isEmpty()) {
            try {
                int edad = Integer.parseInt(textoEdad);
                if (edad < 0 || edad > 300) {
                    campoEdad.setError("La edad debe estar entre 0 y 300 meses.");
                    return false;
                }
            } catch (NumberFormatException error) {
                campoEdad.setError("Edad no válida.");
                return false;
            }
        }
        return true;
    }

    private int especieSeleccionada() {
        int posicion = campoEspecie.getSelectedItemPosition();
        return posicion <= 0 ? 0 : especies.get(posicion - 1).id;
    }

    private int edad() {
        String texto = campoEdad.getText().toString().trim();
        return texto.isEmpty() ? 0 : Integer.parseInt(texto);
    }

    private void guardar() {
        if (!validar()) {
            return;
        }

        String nombre = campoNombre.getText().toString().trim();
        String raza = campoRaza.getText().toString().trim();
        String historia = campoHistoria.getText().toString().trim();
        int especieId = especieSeleccionada();
        int sexo = campoSexo.getSelectedItemPosition();
        int tamano = campoTamano.getSelectedItemPosition();
        boolean vacunada = checkVacunada.isChecked();
        boolean esterilizada = checkEsterilizada.isChecked();

        formCargando.setVisibility(View.VISIBLE);

        if (original == null) {
            Modelos.CrearMascota datos = new Modelos.CrearMascota();
            datos.nombre = nombre;
            datos.especieId = especieId;
            datos.raza = raza.isEmpty() ? "Mestizo" : raza;
            datos.edadMeses = edad();
            datos.sexo = sexo;
            datos.tamano = tamano;
            datos.vacunada = vacunada;
            datos.esterilizada = esterilizada;
            datos.historia = historia;

            Repositorio.ejecutar(Repositorio.crear(this, datos), new Repositorio.Resultado<Modelos.Mascota>() {
                @Override
                public void exito(Modelos.Mascota datos) {
                    terminarBien();
                }

                @Override
                public void error(String mensaje, boolean reintentar) {
                    fallo(mensaje);
                }
            });
        } else {
            Modelos.ActualizarMascota datos = new Modelos.ActualizarMascota();
            datos.nombre = nombre;
            datos.especieId = especieId;
            datos.raza = raza.isEmpty() ? "Mestizo" : raza;
            datos.edadMeses = edad();
            datos.sexo = sexo;
            datos.tamano = tamano;
            datos.vacunada = vacunada;
            datos.esterilizada = esterilizada;
            datos.historia = historia;
            datos.estado = campoEstado.getSelectedItemPosition();

            Repositorio.ejecutar(Repositorio.editar(this, original.id, datos),
                    new Repositorio.Resultado<Void>() {
                        @Override
                        public void exito(Void datos) {
                            terminarBien();
                        }

                        @Override
                        public void error(String mensaje, boolean reintentar) {
                            fallo(mensaje);
                        }
                    });
        }
    }

    private void terminarBien() {
        formCargando.setVisibility(View.GONE);
        Toast.makeText(this, R.string.guardado, Toast.LENGTH_SHORT).show();
        setResult(RESULT_OK);
        finish();
    }

    private void fallo(String mensaje) {
        formCargando.setVisibility(View.GONE);
        Toast.makeText(this, mensaje, Toast.LENGTH_LONG).show();
    }
}
