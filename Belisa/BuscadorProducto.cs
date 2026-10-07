using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using BelisaEntidad;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class BuscadorProducto : TextBox
    {
        private readonly Dictionary<string, Producto> mapa =
            new Dictionary<string, Producto>(StringComparer.OrdinalIgnoreCase);

        public BuscadorProducto()
        {
            Font = Estilo.Normal;
            ForeColor = Estilo.Texto;
            BorderStyle = BorderStyle.FixedSingle;
            AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        public void Cargar(IEnumerable<Producto> productos)
        {
            mapa.Clear();
            var sugerencias = new AutoCompleteStringCollection();

            foreach (var p in productos)
            {
                string porCodigo = p.Codigo + " · " + p.Nombre;
                string porNombre = p.Nombre + " · " + p.Codigo;

                mapa[porCodigo] = p;
                mapa[porNombre] = p;
                mapa[p.Codigo] = p;

                sugerencias.Add(porCodigo);
                sugerencias.Add(porNombre);
            }

            AutoCompleteCustomSource = sugerencias;
        }

        // El producto que corresponde a lo escrito, o null si no coincide con ninguno
        public Producto Seleccionado()
        {
            Producto p;
            return mapa.TryGetValue(Text.Trim(), out p) ? p : null;
        }
    }
}