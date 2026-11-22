package com.ucenm.mascotascrud;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;
import androidx.recyclerview.widget.LinearLayoutManager;
import androidx.recyclerview.widget.RecyclerView;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/** Pantalla de lista con búsqueda, filtro por especie, paginación y estados. */
public class MainActivity extends AppCompatActivity {

    private static final int CODIGO_FORMULARIO = 1;

    private EditText campoBusqueda;
    private Spinner filtroEspecie;
    private TextView textoBaseUrl;
    private TextView paginaInfo;
    private TextView mensajeError;
    private TextView textoVacio;
    private ProgressBar cargando;
    private LinearLayout panelError;
    private Button botonAnterior;
    private Button botonSiguiente;

    private AdaptadorMascotas adaptador;
    private final List<Modelos.Especie> especies = new ArrayList<>();
    private final Map<Integer, String> nombresEspecies = new HashMap<>();

    private String busqueda = "";
    private int especieId = 0;
    private int pagina = 1;
    private int totalPaginas = 1;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        campoBusqueda = findViewById(R.id.campoBusqueda);
        filtroEspecie = findViewById(R.id.filtroEspecie);
        textoBaseUrl = findViewById(R.id.textoBaseUrl);
        paginaInfo = findViewById(R.id.paginaInfo);
        mensajeError = findViewById(R.id.mensajeError);
        textoVacio = findViewById(R.id.vacio);
        cargando = findViewById(R.id.cargando);
        panelError = findViewById(R.id.panelError);
        botonAnterior = findViewById(R.id.botonAnterior);
        botonSiguiente = findViewById(R.id.botonSiguiente);

        RecyclerView lista = findViewById(R.id.listaMascotas);
        lista.setLayoutManager(new LinearLayoutManager(this));
        adaptador = new AdaptadorMascotas(mascota -> {
            Intent intento = new Intent(this, DetalleActivity.class);
            intento.putExtra("mascota", mascota);
            startActivityForResult(intento, CODIGO_FORMULARIO);
        });
        lista.setAdapter(adaptador);

        findViewById(R.id.botonBuscar).setOnClickListener(v -> {
            busqueda = campoBusqueda.getText().toString();
            pagina = 1;
            cargar();
        });

        findViewById(R.id.fabNueva).setOnClickListener(v -> {
            Intent intento = new Intent(this, FormularioActivity.class);
            startActivityForResult(intento, CODIGO_FORMULARIO);
        });

        findViewById(R.id.botonReintentar).setOnClickListener(v -> cargar());

        View.OnClickListener cambiarUrl = v -> {
            boolean emulador = !ApiClient.usandoEmulador(this);
            ApiClient.usarEmulador(this, emulador);
            actualizarTextoBaseUrl();
            cargar();
            Toast.makeText(this,
                    emulador ? "Usando http://10.0.2.2:5176/"
                            : "Usando http://192.168.56.1:5176/",
                    Toast.LENGTH_SHORT).show();
        };
        findViewById(R.id.botonLocalhost).setOnClickListener(cambiarUrl);
        findViewById(R.id.botonProbarLocalhost).setOnClickListener(cambiarUrl);

        botonAnterior.setOnClickListener(v -> {
            if (pagina > 1) {
                pagina--;
                cargar();
            }
        });
        botonSiguiente.setOnClickListener(v -> {
            if (pagina < totalPaginas) {
                pagina++;
                cargar();
            }
        });

        actualizarTextoBaseUrl();
        configurarFiltro();
        cargarEspecies();
        cargar();
    }

    private void actualizarTextoBaseUrl() {
        textoBaseUrl.setText("API: " + ApiClient.baseUrl(this));
        boolean emulador = ApiClient.usandoEmulador(this);
        ((Button) findViewById(R.id.botonLocalhost)).setText(
                emulador ? R.string.usar_localhost : R.string.usar_10_0_2_2);
        ((Button) findViewById(R.id.botonProbarLocalhost)).setText(
                emulador ? R.string.usar_localhost : R.string.usar_10_0_2_2);
    }

    private void configurarFiltro() {
        List<String> opciones = new ArrayList<>();
        opciones.add(getString(R.string.todas_las_especies));
        ArrayAdapter<String> adaptadorEspecies = new ArrayAdapter<>(
                this, android.R.layout.simple_spinner_item, opciones);
        adaptadorEspecies.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        filtroEspecie.setAdapter(adaptadorEspecies);

        filtroEspecie.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> padre, View vista, int posicion, long id) {
                int nuevo = posicion == 0 ? 0 : especies.get(posicion - 1).id;
                if (nuevo != especieId) {
                    especieId = nuevo;
                    pagina = 1;
                    cargar();
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> padre) {
            }
        });
    }

    private void cargarEspecies() {
        Repositorio.ejecutar(Repositorio.especies(this), new Repositorio.Resultado<List<Modelos.Especie>>() {
            @Override
            public void exito(List<Modelos.Especie> datos) {
                especies.clear();
                nombresEspecies.clear();
                List<String> opciones = new ArrayList<>();
                opciones.add(getString(R.string.todas_las_especies));
                if (datos != null) {
                    for (Modelos.Especie especie : datos) {
                        especies.add(especie);
                        opciones.add(especie.nombre);
                        nombresEspecies.put(especie.id, especie.nombre);
                    }
                }
                int seleccion = filtroEspecie.getSelectedItemPosition();
                ArrayAdapter<String> adaptadorEspecies = new ArrayAdapter<>(
                        MainActivity.this, android.R.layout.simple_spinner_item, opciones);
                adaptadorEspecies.setDropDownViewResource(
                        android.R.layout.simple_spinner_dropdown_item);
                filtroEspecie.setAdapter(adaptadorEspecies);
                if (seleccion > 0 && seleccion < opciones.size()) {
                    filtroEspecie.setSelection(seleccion);
                }
                adaptador.setEspecies(nombresEspecies);
            }

            @Override
            public void error(String mensaje, boolean reintentar) {
                // La lista se carga por separado; aquí solo dejamos el filtro en "Todas".
            }
        });
    }

    private void cargar() {
        cargando.setVisibility(View.VISIBLE);
        panelError.setVisibility(View.GONE);
        textoVacio.setVisibility(View.GONE);

        Repositorio.ejecutar(Repositorio.listar(this, busqueda, pagina, especieId),
                new Repositorio.Resultado<Modelos.Pagina>() {
                    @Override
                    public void exito(Modelos.Pagina datos) {
                        cargando.setVisibility(View.GONE);
                        if (datos == null || datos.items == null || datos.items.isEmpty()) {
                            adaptador.mostrar(new ArrayList<>());
                            textoVacio.setVisibility(View.VISIBLE);
                        } else {
                            adaptador.mostrar(datos.items);
                            textoVacio.setVisibility(View.GONE);
                        }
                        totalPaginas = Math.max(1, datos == null ? 1 : datos.totalPaginas);
                        int total = datos == null ? 0 : datos.totalRegistros;
                        paginaInfo.setText(getString(R.string.pagina, pagina, totalPaginas, total));
                        botonAnterior.setEnabled(pagina > 1);
                        botonSiguiente.setEnabled(pagina < totalPaginas);
                    }

                    @Override
                    public void error(String mensaje, boolean reintentar) {
                        cargando.setVisibility(View.GONE);
                        AdaptadorMascotas vacio = adaptador;
                        vacio.mostrar(new ArrayList<>());
                        mensajeError.setText(mensaje);
                        panelError.setVisibility(View.VISIBLE);
                        paginaInfo.setText("");
                        botonAnterior.setEnabled(false);
                        botonSiguiente.setEnabled(false);
                    }
                });
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == CODIGO_FORMULARIO && resultCode == RESULT_OK) {
            cargar();
        }
    }
}
