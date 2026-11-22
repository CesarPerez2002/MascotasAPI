package com.ucenm.mascotascrud;

import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

final class AdaptadorMascotas extends RecyclerView.Adapter<AdaptadorMascotas.Fila> {

    interface AlClic {
        void alClic(Modelos.Mascota mascota);
    }

    private final List<Modelos.Mascota> datos = new ArrayList<>();
    private final Map<Integer, String> especies = new HashMap<>();
    private final AlClic listener;

    AdaptadorMascotas(AlClic listener) {
        this.listener = listener;
    }

    void mostrar(List<Modelos.Mascota> registros) {
        datos.clear();
        if (registros != null) {
            datos.addAll(registros);
        }
        notifyDataSetChanged();
    }

    void setEspecies(Map<Integer, String> nombres) {
        especies.clear();
        if (nombres != null) {
            especies.putAll(nombres);
        }
        notifyDataSetChanged();
    }

    @NonNull
    @Override
    public Fila onCreateViewHolder(@NonNull ViewGroup padre, int vista) {
        View vistaFila = LayoutInflater.from(padre.getContext())
                .inflate(R.layout.item_mascota, padre, false);
        return new Fila(vistaFila);
    }

    @Override
    public void onBindViewHolder(@NonNull Fila fila, int posicion) {
        final Modelos.Mascota mascota = datos.get(posicion);
        String nombreEspecie = especies.get(mascota.especieId);
        if (nombreEspecie == null) {
            nombreEspecie = "Especie " + mascota.especieId;
        }

        fila.nombre.setText(mascota.nombre);
        fila.detalle.setText(nombreEspecie + " · " + mascota.raza
                + " · " + mascota.edadMeses + " meses");
        fila.estado.setText(Utilidades.estado(mascota.estado));

        fila.itemView.setOnClickListener(v -> listener.alClic(mascota));
    }

    @Override
    public int getItemCount() {
        return datos.size();
    }

    static final class Fila extends RecyclerView.ViewHolder {
        final TextView nombre;
        final TextView detalle;
        final TextView estado;

        Fila(View vista) {
            super(vista);
            nombre = vista.findViewById(R.id.itemNombre);
            detalle = vista.findViewById(R.id.itemDetalle);
            estado = vista.findViewById(R.id.itemEstado);
        }
    }
}
