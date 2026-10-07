using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    [DesignerCategory("Code")]
    public class FrmTicketConfig : Form
    {
        private ComboBox cmbImpresora, cmbAncho, cmbModo;
        private TextBox txtContacto, txtMensaje;
        private Label lblMensaje;

        public FrmTicketConfig()
        {
            Estilo.ConfigurarVentana(this, 460, 540, "Configurar ticket");
            BackColor = Color.White;

            var cfg = TicketConfig.Cargar();

            cmbImpresora = Combo(24, 44);
            cmbImpresora.Items.Add("(Impresora predeterminada de Windows)");
            foreach (string nombre in PrinterSettings.InstalledPrinters)
                cmbImpresora.Items.Add(nombre);
            int pos = string.IsNullOrEmpty(cfg.Impresora) ? 0 : cmbImpresora.Items.IndexOf(cfg.Impresora);
            cmbImpresora.SelectedIndex = pos < 0 ? 0 : pos;

            cmbAncho = Combo(24, 118);
            cmbAncho.Items.AddRange(new object[] { "80 mm", "58 mm" });
            cmbAncho.SelectedIndex = cfg.AnchoMm == 58 ? 1 : 0;

            cmbModo = Combo(24, 192);
            cmbModo.Items.AddRange(TicketConfig.Modos);
            int modo = Array.IndexOf(TicketConfig.Modos, cfg.AlCobrar);
            cmbModo.SelectedIndex = modo < 0 ? 0 : modo;

            txtContacto = Estilo.Campo(24, 266, 410);
            txtContacto.MaxLength = 60;
            txtContacto.Text = cfg.Contacto;

            txtMensaje = Estilo.Campo(24, 340, 410);
            txtMensaje.MaxLength = 60;
            txtMensaje.Text = cfg.Mensaje;

            lblMensaje = new Label
            {
                Location = new Point(24, 378),
                Size = new Size(410, 36),
                Font = Estilo.Normal
            };

            var btnPrueba = Estilo.Boton("Imprimir prueba", 24, 420, 200, 40, false);
            var btnGuardar = Estilo.Boton("Guardar", 234, 420, 200, 40, true);
            var btnCancelar = Estilo.Boton("Cancelar", 24, 472, 410, 34, false);

            btnPrueba.Click += (s, e) => Prueba();
            btnGuardar.Click += (s, e) => Guardar();
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                Estilo.Etiqueta("Impresora", 24, 20), cmbImpresora,
                Estilo.Etiqueta("Ancho del papel", 24, 94), cmbAncho,
                Estilo.Etiqueta("Al cobrar una venta, imprimir el ticket", 24, 168), cmbModo,
                Estilo.Etiqueta("Teléfono o dirección del negocio (opcional)", 24, 242), txtContacto,
                Estilo.Etiqueta("Mensaje al final del ticket", 24, 316), txtMensaje,
                lblMensaje, btnPrueba, btnGuardar, btnCancelar
            });

            AcceptButton = btnGuardar;
            CancelButton = btnCancelar;
        }

        private static ComboBox Combo(int x, int y)
        {
            return new ComboBox
            {
                Location = new Point(x, y),
                Width = 410,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal
            };
        }

        // Lo que hay en pantalla, sin guardarlo todavía
        private TicketConfig Leer()
        {
            return new TicketConfig
            {
                Impresora = cmbImpresora.SelectedIndex <= 0 ? "" : Convert.ToString(cmbImpresora.SelectedItem),
                AnchoMm = cmbAncho.SelectedIndex == 1 ? 58 : 80,
                AlCobrar = Convert.ToString(cmbModo.SelectedItem),
                Contacto = txtContacto.Text.Trim(),
                Mensaje = txtMensaje.Text.Trim()
            };
        }

        private void Prueba()
        {
            lblMensaje.Text = "";

            var venta = new Venta
            {
                NumeroDocumento = "VEN-PRUEBA",
                FechaRegistro = DateTime.Now,
                ClienteNombre = "Público en general",
                UsuarioNombre = Sesion.UsuarioActual == null ? "Prueba" : Sesion.UsuarioActual.NombreCompleto,
                MetodoPago = "Efectivo",
                Subtotal = 350m,
                DescuentoPorcentaje = 10m,
                MontoDescuento = 35m,
                MontoTotal = 315m,
                MontoPago = 400m,
                MontoCambio = 85m
            };

            var detalle = new List<DetalleVenta>
            {
                new DetalleVenta { ProductoNombre = "Cera tibia 500 g", Cantidad = 2, PrecioVenta = 100m, SubTotal = 200m },
                new DetalleVenta { ProductoNombre = "Bandas depilatorias", Cantidad = 3, PrecioVenta = 50m, SubTotal = 150m }
            };

            try
            {
                Ticket.Imprimir(venta, detalle, Leer(), false);
                lblMensaje.ForeColor = Color.FromArgb(39, 130, 80);
                lblMensaje.Text = "Prueba enviada a la impresora.";
            }
            catch (Exception ex)
            {
                lblMensaje.ForeColor = Estilo.Error;
                lblMensaje.Text = ex.Message;
            }
        }

        private void Guardar()
        {
            try
            {
                Leer().Guardar();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                lblMensaje.ForeColor = Estilo.Error;
                lblMensaje.Text = "No se pudo guardar: " + ex.Message;
            }
        }
    }
}