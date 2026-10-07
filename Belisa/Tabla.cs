using System.Drawing;
using System.Windows.Forms;

namespace Belisa
{
    public static class Tabla
    {
        public static DataGridView Crear()
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Estilo.LilaSuave,
                EnableHeadersVisualStyles = false,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight = 40,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            g.RowTemplate.Height = 36;
            g.ColumnHeadersDefaultCellStyle.BackColor = Estilo.LilaSuave;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Estilo.Texto;
            g.ColumnHeadersDefaultCellStyle.Font = Estilo.Negrita;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Estilo.LilaSuave;
            g.DefaultCellStyle.Font = Estilo.Normal;
            g.DefaultCellStyle.ForeColor = Estilo.Texto;
            g.DefaultCellStyle.SelectionBackColor = Estilo.LilaClaro;
            g.DefaultCellStyle.SelectionForeColor = Estilo.Texto;
            return g;
        }

        // Columna que se reparte el espacio sobrante (peso = proporción)
        public static void Columna(DataGridView g, string nombre, string titulo, int peso)
        {
            g.Columns.Add(nombre, titulo);
            g.Columns[nombre].FillWeight = peso;
        }

        // Columna de ancho fijo
        public static void ColumnaFija(DataGridView g, string nombre, string titulo, int ancho,
                                       string formato, bool alinearDerecha)
        {
            g.Columns.Add(nombre, titulo);
            var col = g.Columns[nombre];
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            col.Width = ancho;
            if (formato != null) col.DefaultCellStyle.Format = formato;
            if (alinearDerecha) col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
    }
}