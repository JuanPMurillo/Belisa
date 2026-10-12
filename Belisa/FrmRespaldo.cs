using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmRespaldo : Form
    {
        private static readonly Color Verde = Color.FromArgb(39, 130, 80);
        private static readonly Color Ambar = Color.FromArgb(176, 110, 10);

        private readonly RespaldoN negocio = new RespaldoN();

        private TextBox txtCarpeta;
        private Label lblEstado, lblMensaje;
        private DataGridView grid;
        private Button btnRespaldar;

        public FrmRespaldo()
        {
            // 1. Evitar parpadeos y asegurar rendimiento visual fluido al redimensionar
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            // 2. Asegurar el escalado correcto de fuentes en distintas resoluciones
            AutoScaleMode = AutoScaleMode.Font;

            BackColor = Estilo.Fondo;

            var contenido = new Panel { Dock = DockStyle.Fill, Padding = new Padding(24), AutoScroll = true };
            ConstruirLista(contenido);        // el centro (Fill) primero...
            ConstruirAyuda(contenido);        // ...luego abajo...
            ConstruirEncabezado(contenido);   // ...y arriba
            Controls.Add(contenido);

            Load += (s, e) => Cargar();
        }

        // ---------- Construcción ----------

        private void ConstruirLista(Panel padre)
        {
            grid = Tabla.Crear();
            Tabla.ColumnaFija(grid, "Fecha", "Fecha", 170, null, false);
            Tabla.Columna(grid, "Archivo", "Archivo", 200);
            Tabla.ColumnaFija(grid, "Tamano", "Tamaño", 120, null, true);
            padre.Controls.Add(grid);
        }

        private void ConstruirAyuda(Panel padre)
        {
            var abajo = new Panel { Dock = DockStyle.Bottom, Height = 92, Padding = new Padding(0, 14, 0, 0) };
            abajo.Controls.Add(new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = Color.Gray,
                Text = "El sistema guarda un respaldo automático una vez al día, al cerrar sesión, y conserva los últimos 10.\n"
                     + "Una vez por semana copia esta carpeta a una USB o a la nube: si la computadora falla, "
                     + "los respaldos que estén en el mismo disco también se pierden.\n"
                     + "Para restaurar uno hace falta SQL Server Management Studio: no se hace desde aquí a propósito."
            });
            padre.Controls.Add(abajo);
        }

        private void ConstruirEncabezado(Panel padre)
        {
            var arriba = new Panel { Dock = DockStyle.Top, Height = 212, Padding = new Padding(0, 0, 0, 12) };
            var tarjeta = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };

            txtCarpeta = Estilo.Campo(20, 38, 600);
            txtCarpeta.ReadOnly = true;
            txtCarpeta.TabStop = false;
            txtCarpeta.BackColor = Estilo.LilaSuave;

            var btnCambiar = Estilo.Boton("Cambiar carpeta", 636, 34, 160, 34, false);
            btnRespaldar = Estilo.Boton("Respaldar ahora", 20, 92, 200, 44, true);
            var btnAbrir = Estilo.Boton("Abrir carpeta", 232, 92, 170, 44, false);

            lblEstado = new Label
            {
                Location = new Point(420, 92),
                Size = new Size(480, 44),
                TextAlign = ContentAlignment.MiddleLeft,
                Font = Estilo.Normal
            };

            lblMensaje = new Label
            {
                Location = new Point(20, 148),
                Size = new Size(880, 48),
                Font = Estilo.Normal
            };

            btnCambiar.Click += (s, e) => CambiarCarpeta();
            btnRespaldar.Click += (s, e) => Respaldar();
            btnAbrir.Click += (s, e) => AbrirCarpeta();

            tarjeta.Controls.AddRange(new Control[]
            {
                Estilo.Etiqueta("Carpeta de respaldos", 20, 14), txtCarpeta, btnCambiar,
                btnRespaldar, btnAbrir, lblEstado, lblMensaje
            });

            arriba.Controls.Add(tarjeta);
            padre.Controls.Add(arriba);
        }

        // ---------- Datos ----------

        private void Cargar()
        {
            txtCarpeta.Text = negocio.Carpeta;

            List<ArchivoRespaldo> lista;
            try
            {
                lista = negocio.Listar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Respaldos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lista = new List<ArchivoRespaldo>();
            }

            grid.Rows.Clear();
            foreach (var a in lista)
                grid.Rows.Add(a.Fecha.ToString("dd/MM/yyyy HH:mm"), a.Nombre, a.TamanoMb.ToString("0.0") + " MB");
            grid.ClearSelection();

            if (lista.Count == 0)
            {
                lblEstado.ForeColor = Estilo.Error;
                lblEstado.Text = "Todavía no hay respaldos en esta carpeta.";
                return;
            }

            double dias = (DateTime.Now - lista[0].Fecha).TotalDays;
            string texto = "Último respaldo: " + lista[0].Fecha.ToString("dd/MM/yyyy HH:mm");

            if (dias >= 7)
            {
                lblEstado.ForeColor = Estilo.Error;
                texto += "\nHace más de una semana: haz uno ahora.";
            }
            else if (dias >= 3)
            {
                lblEstado.ForeColor = Ambar;
                texto += "\nHace varios días.";
            }
            else
            {
                lblEstado.ForeColor = Verde;
            }
            lblEstado.Text = texto;
        }

        // ---------- Acciones ----------

        private void Respaldar()
        {
            lblMensaje.Text = "";
            btnRespaldar.Enabled = false;
            Cursor = Cursors.WaitCursor;

            try
            {
                string archivo, mensaje;
                bool ok = negocio.Respaldar(out archivo, out mensaje);

                Cargar();
                lblMensaje.ForeColor = ok ? Verde : Estilo.Error;
                lblMensaje.Text = ok ? "Respaldo creado: " + Path.GetFileName(archivo) : mensaje;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Respaldos", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnRespaldar.Enabled = true;
            }
        }

        private void CambiarCarpeta()
        {
            lblMensaje.Text = "";

            using (var dialogo = new FolderBrowserDialog())
            {
                dialogo.Description = "Elige dónde guardar los respaldos (una carpeta de tu disco o una USB). "
                                    + "SQL Server necesita permiso para escribir ahí.";
                dialogo.ShowNewFolderButton = true;
                if (Directory.Exists(negocio.Carpeta)) dialogo.SelectedPath = negocio.Carpeta;

                if (dialogo.ShowDialog() != DialogResult.OK) return;

                try
                {
                    negocio.CambiarCarpeta(dialogo.SelectedPath);
                }
                catch (Exception ex)
                {
                    lblMensaje.ForeColor = Estilo.Error;
                    lblMensaje.Text = ex.Message;
                    return;
                }
            }

            Cargar();
            lblMensaje.ForeColor = Estilo.Texto;
            lblMensaje.Text = "Carpeta actualizada. Presiona «Respaldar ahora» para comprobar que funciona.";
        }

        private void AbrirCarpeta()
        {
            string carpeta = negocio.Carpeta;
            if (!Directory.Exists(carpeta))
            {
                lblMensaje.ForeColor = Estilo.Error;
                lblMensaje.Text = "La carpeta todavía no existe. Se crea con el primer respaldo.";
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(carpeta) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Respaldos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}