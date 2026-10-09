using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmConexion : Form
    {
        private static readonly Color Verde = Color.FromArgb(39, 130, 80);

        private readonly ConexionN negocio = new ConexionN();
        private readonly ComboBox cmbServidor;
        private readonly TextBox txtBase;
        private readonly Label lblResultado;
        private readonly Button btnProbar, btnGuardar, btnCancelar;

        public FrmConexion(string aviso)
        {
            Estilo.ConfigurarVentana(this, 520, 520, "Configurar conexión");
            BackColor = Color.White;

            var titulo = new Label
            {
                Text = "Conexión a la base de datos",
                Location = new Point(24, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };

            var lblAviso = new Label
            {
                Location = new Point(24, 62),
                Size = new Size(472, 84),
                Font = Estilo.Normal,
                ForeColor = Estilo.Texto,
                Text = string.IsNullOrWhiteSpace(aviso)
                    ? "Indica en qué servidor de SQL Server está la base de datos del programa."
                    : aviso
            };

            cmbServidor = new ComboBox
            {
                Location = new Point(24, 180),
                Width = 472,
                DropDownStyle = ComboBoxStyle.DropDown,   // se puede elegir de la lista o escribir
                Font = Estilo.Normal
            };

            List<string> detectadas = negocio.InstanciasLocales();
            foreach (string servidor in detectadas) cmbServidor.Items.Add(servidor);

            string actual = negocio.ServidorActual;
            if (actual.Length > 0) cmbServidor.Text = actual;
            else if (detectadas.Count > 0) cmbServidor.Text = detectadas[0];
            else cmbServidor.Text = ConexionN.ServidorPorDefecto;

            var lblAyuda = new Label
            {
                Location = new Point(24, 214),
                Size = new Size(472, 38),
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.Gray,
                Text = detectadas.Count > 0
                    ? "Se detectó SQL Server en esta computadora: elige una instancia de la lista."
                    : "No se detectó SQL Server en esta computadora. Con la instalación Basic de SQL Server Express "
                      + "el servidor suele ser " + ConexionN.ServidorPorDefecto
            };

            txtBase = Estilo.Campo(24, 282, 472);
            txtBase.MaxLength = 128;
            txtBase.Text = negocio.BaseDatosActual.Length > 0 ? negocio.BaseDatosActual : ConexionN.BaseDatosPorDefecto;

            lblResultado = new Label
            {
                Location = new Point(24, 326),
                Size = new Size(472, 80),
                Font = Estilo.Normal
            };

            btnProbar = Estilo.Boton("Probar conexión", 24, 414, 224, 42, false);
            btnGuardar = Estilo.Boton("Guardar", 264, 414, 232, 42, true);
            btnCancelar = Estilo.Boton("Cancelar", 24, 464, 472, 36, false);

            btnProbar.Click += async (s, e) => await Verificar();
            btnGuardar.Click += (s, e) => Guardar();
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                titulo, lblAviso,
                Estilo.Etiqueta("Servidor de SQL Server", 24, 156), cmbServidor, lblAyuda,
                Estilo.Etiqueta("Nombre de la base de datos", 24, 258), txtBase,
                lblResultado, btnProbar, btnGuardar, btnCancelar
            });

            AcceptButton = btnGuardar;    // Enter = guardar
            CancelButton = btnCancelar;   // Esc = cancelar
        }

        private void Ocupado(bool si)
        {
            UseWaitCursor = si;
            cmbServidor.Enabled = !si;
            txtBase.Enabled = !si;
            btnProbar.Enabled = !si;
            btnGuardar.Enabled = !si;
        }

        // Prueba la conexión sin congelar la ventana y muestra el resultado
        private async Task<bool> Verificar()
        {
            string servidor = cmbServidor.Text;
            string baseDatos = txtBase.Text;

            string invalido;
            if (!negocio.Validar(servidor, baseDatos, out invalido))
            {
                lblResultado.ForeColor = Estilo.Error;
                lblResultado.Text = invalido;
                return false;
            }

            Ocupado(true);
            lblResultado.ForeColor = Estilo.Texto;
            lblResultado.Text = "Probando la conexión...";

            bool ok;
            string mensaje;
            try
            {
                var resultado = await Task.Run(() =>
                {
                    string m;
                    bool bien = negocio.Probar(servidor, baseDatos, out m);
                    return new KeyValuePair<bool, string>(bien, m);
                });
                ok = resultado.Key;
                mensaje = resultado.Value;
            }
            catch (Exception ex)   // la prueba ya traduce los errores de SQL; esto cubre cualquier otro imprevisto
            {
                ok = false;
                mensaje = "No se pudo probar la conexión: " + ex.Message;
            }

            if (IsDisposed) return false;   // cerraron la ventana mientras se probaba
            Ocupado(false);

            lblResultado.ForeColor = ok ? Verde : Estilo.Error;
            lblResultado.Text = ok
                ? "Conexión correcta. La base de datos tiene las tablas del sistema."
                : mensaje;
            return ok;
        }

        private async void Guardar()
        {
            string invalido;
            if (!negocio.Validar(cmbServidor.Text, txtBase.Text, out invalido))
            {
                lblResultado.ForeColor = Estilo.Error;
                lblResultado.Text = invalido;
                return;
            }

            bool ok = await Verificar();
            if (IsDisposed) return;

            if (!ok && MessageBox.Show(
                    "La conexión todavía no funciona con estos datos.\n\n¿Guardarlos de todos modos?",
                    "Configurar conexión", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            string mensaje;
            if (!negocio.Guardar(cmbServidor.Text, txtBase.Text, out mensaje))
            {
                lblResultado.ForeColor = Estilo.Error;
                lblResultado.Text = mensaje;
                return;
            }

            DialogResult = DialogResult.OK;
        }
    }
}