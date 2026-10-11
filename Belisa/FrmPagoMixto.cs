using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using BelisaEntidad;
using BelisaNegocio;

namespace Belisa
{
    // Ventanita para cobrar con dos métodos de pago, por ejemplo un apartado por transferencia
    // y el resto en efectivo. Devuelve las dos partes y, si una es efectivo, cuánto entregó el cliente.
    [DesignerCategory("Code")]
    public class FrmPagoMixto : Form
    {
        private static readonly Color Verde = Color.FromArgb(39, 130, 80);

        private readonly decimal total;
        private readonly string[] metodos = VentaN.MetodosPago;

        private readonly ComboBox cmbMetodo1, cmbMetodo2;
        private readonly TextBox txtMonto1, txtRef1, txtRef2, txtEfectivo;
        private readonly Label lblMonto2, lblEfectivoTitulo, lblCambioTitulo, lblCambio, lblError;

        private bool ajustando = true;   // evita recalcular mientras se arma la ventana o se cambian valores por código
        private bool efectivoManual;     // true cuando la persona escribió el efectivo a mano

        // Resultado cuando se acepta
        public List<PagoVenta> Pagos { get; private set; }
        public decimal EfectivoRecibido { get; private set; }

        public FrmPagoMixto(decimal total, List<PagoVenta> previos, decimal efectivoPrevio)
        {
            this.total = total;
            Estilo.ConfigurarVentana(this, 520, 590, "Pago mixto");
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            var titulo = new Label
            {
                Text = "Pago mixto",
                Location = new Point(24, 14),
                AutoSize = true,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };
            var lblTotal = new Label
            {
                Text = "Total a cobrar: " + total.ToString("C2"),
                Location = new Point(24, 48),
                AutoSize = true,
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Estilo.Acento
            };

            // Primer pago
            cmbMetodo1 = NuevoCombo(24, 142, 190);
            txtMonto1 = Estilo.Campo(234, 142, 120);
            txtMonto1.TextAlign = HorizontalAlignment.Right;
            txtRef1 = Estilo.Campo(24, 204, 472);
            txtRef1.MaxLength = 50;

            // Segundo pago: recibe lo que falta
            cmbMetodo2 = NuevoCombo(24, 304, 190);
            lblMonto2 = new Label
            {
                Location = new Point(234, 302),
                Size = new Size(120, 30),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };
            txtRef2 = Estilo.Campo(24, 366, 472);
            txtRef2.MaxLength = 50;

            // Efectivo (solo si alguna parte es efectivo)
            lblEfectivoTitulo = Estilo.Etiqueta("Efectivo que entrega el cliente", 24, 412);
            txtEfectivo = Estilo.Campo(24, 436, 190);
            txtEfectivo.TextAlign = HorizontalAlignment.Right;
            lblCambioTitulo = Estilo.Etiqueta("Cambio", 234, 442);
            lblCambio = new Label
            {
                Location = new Point(300, 436),
                Size = new Size(196, 30),
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 13f, FontStyle.Bold)
            };

            lblError = new Label
            {
                Location = new Point(24, 480),
                Size = new Size(472, 30),
                Font = Estilo.Normal,
                ForeColor = Estilo.Error
            };

            var btnAceptar = Estilo.Boton("Aceptar", 24, 520, 236, 42, true);
            var btnCancelar = Estilo.Boton("Cancelar", 270, 520, 226, 42, false);

            Controls.AddRange(new Control[]
            {
                titulo, lblTotal,
                Encabezado("Primer pago", 88),
                Estilo.Etiqueta("Método", 24, 118), Estilo.Etiqueta("Monto", 234, 118),
                cmbMetodo1, txtMonto1,
                Estilo.Etiqueta("Referencia (opcional)", 24, 180), txtRef1,
                Encabezado("Segundo pago (lo que falta)", 250),
                Estilo.Etiqueta("Método", 24, 280), Estilo.Etiqueta("Monto", 234, 280),
                cmbMetodo2, lblMonto2,
                Estilo.Etiqueta("Referencia (opcional)", 24, 342), txtRef2,
                lblEfectivoTitulo, txtEfectivo, lblCambioTitulo, lblCambio,
                lblError, btnAceptar, btnCancelar
            });

            // Valores iniciales: lo habitual es un apartado por transferencia y el resto en efectivo
            cmbMetodo1.SelectedItem = "Transferencia";
            cmbMetodo2.SelectedItem = "Efectivo";
            decimal inicial = Math.Min(100m, Math.Round(total / 2m, 2));
            txtMonto1.Text = inicial.ToString("0.00", CultureInfo.CurrentCulture);

            // Si ya se había configurado para este mismo total, se vuelve a mostrar lo que se capturó
            if (previos != null && previos.Count == 2)
            {
                cmbMetodo1.SelectedItem = previos[0].MetodoPago;
                txtMonto1.Text = previos[0].Monto.ToString("0.00", CultureInfo.CurrentCulture);
                txtRef1.Text = previos[0].ReferenciaPago ?? "";
                cmbMetodo2.SelectedItem = previos[1].MetodoPago;
                txtRef2.Text = previos[1].ReferenciaPago ?? "";
                if (efectivoPrevio > 0)
                {
                    txtEfectivo.Text = efectivoPrevio.ToString("0.00", CultureInfo.CurrentCulture);
                    efectivoManual = true;
                }
            }

            // Los eventos se conectan al final, cuando ya existen todos los controles y valores
            cmbMetodo1.SelectedIndexChanged += (s, e) => { Distintos(cmbMetodo1, cmbMetodo2); Recalcular(); };
            cmbMetodo2.SelectedIndexChanged += (s, e) => { Distintos(cmbMetodo2, cmbMetodo1); Recalcular(); };
            txtMonto1.TextChanged += (s, e) => Recalcular();   // el resto se actualiza mientras se escribe
            txtEfectivo.TextChanged += (s, e) =>
            {
                if (ajustando) return;
                efectivoManual = true;
                Recalcular();
            };
            btnAceptar.Click += (s, e) => Aceptar();
            btnCancelar.Click += (s, e) => DialogResult = DialogResult.Cancel;

            AcceptButton = btnAceptar;    // Enter = aceptar
            CancelButton = btnCancelar;   // Esc = cancelar

            ajustando = false;
            Recalcular();
        }

        private ComboBox NuevoCombo(int x, int y, int ancho)
        {
            var combo = new ComboBox
            {
                Location = new Point(x, y),
                Width = ancho,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = Estilo.Normal
            };
            combo.Items.AddRange(metodos);
            combo.SelectedIndex = 0;
            return combo;
        }

        private static Label Encabezado(string texto, int y)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(24, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Estilo.Texto
            };
        }

        // Lee un importe escrito; si no se entiende, devuelve 0
        private static decimal Leer(TextBox caja)
        {
            decimal valor;
            if (decimal.TryParse(caja.Text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out valor) && valor >= 0)
                return Math.Round(valor, 2);
            return 0m;
        }

        // Los dos métodos no pueden ser el mismo: si se repite, el otro cambia solo
        private void Distintos(ComboBox cambio, ComboBox otro)
        {
            if (ajustando) return;

            string elegido = Convert.ToString(cambio.SelectedItem);
            if (elegido != Convert.ToString(otro.SelectedItem)) return;

            ajustando = true;   // el cambio hecho por código no debe disparar otro recálculo
            otro.SelectedItem = metodos.First(m => m != elegido);
            ajustando = false;
        }

        private void Recalcular()
        {
            if (ajustando) return;
            ajustando = true;
            try
            {
                decimal monto1 = Leer(txtMonto1);
                decimal monto2 = total - monto1;
                string m1 = Convert.ToString(cmbMetodo1.SelectedItem);
                string m2 = Convert.ToString(cmbMetodo2.SelectedItem);

                lblMonto2.ForeColor = monto2 > 0 ? Estilo.Texto : Estilo.Error;
                lblMonto2.Text = monto2.ToString("C2");

                // La referencia (folio, clave de rastreo) solo aplica si no es efectivo
                txtRef1.Enabled = m1 != "Efectivo";
                if (!txtRef1.Enabled) txtRef1.Clear();
                txtRef2.Enabled = m2 != "Efectivo";
                if (!txtRef2.Enabled) txtRef2.Clear();

                bool hayEfectivo = m1 == "Efectivo" || m2 == "Efectivo";
                decimal efectivoParte = (m1 == "Efectivo" ? monto1 : 0m) + (m2 == "Efectivo" ? monto2 : 0m);

                lblEfectivoTitulo.Visible = hayEfectivo;
                txtEfectivo.Visible = hayEfectivo;
                lblCambioTitulo.Visible = hayEfectivo;
                lblCambio.Visible = hayEfectivo;

                if (hayEfectivo)
                {
                    // Mientras no se escriba otra cosa, se asume que entrega justo lo que toca
                    if (!efectivoManual)
                        txtEfectivo.Text = efectivoParte.ToString("0.00", CultureInfo.CurrentCulture);

                    decimal entrega = Leer(txtEfectivo);
                    if (entrega >= efectivoParte)
                    {
                        lblCambioTitulo.Text = "Cambio";
                        lblCambio.ForeColor = Verde;
                        lblCambio.Text = (entrega - efectivoParte).ToString("C2");
                    }
                    else
                    {
                        lblCambioTitulo.Text = "Falta";
                        lblCambio.ForeColor = Estilo.Error;
                        lblCambio.Text = (efectivoParte - entrega).ToString("C2");
                    }
                }

                lblError.Text = (monto1 <= 0 || monto2 <= 0)
                    ? "El primer pago debe ser mayor a 0 y menor al total."
                    : "";
            }
            finally
            {
                ajustando = false;
            }
        }

        private void Aceptar()
        {
            lblError.Text = "";

            decimal monto1 = Leer(txtMonto1);
            decimal monto2 = total - monto1;
            if (monto1 <= 0 || monto2 <= 0)
            {
                lblError.Text = "El primer pago debe ser mayor a 0 y menor al total.";
                txtMonto1.Focus();
                return;
            }

            string m1 = Convert.ToString(cmbMetodo1.SelectedItem);
            string m2 = Convert.ToString(cmbMetodo2.SelectedItem);
            if (m1 == m2)
            {
                lblError.Text = "Los dos métodos deben ser distintos.";
                return;
            }

            decimal entrega = 0m;
            if (m1 == "Efectivo" || m2 == "Efectivo")
            {
                decimal efectivoParte = (m1 == "Efectivo" ? monto1 : 0m) + (m2 == "Efectivo" ? monto2 : 0m);
                entrega = Leer(txtEfectivo);
                if (entrega < efectivoParte)
                {
                    lblError.Text = "El efectivo que entrega el cliente es menor a lo que se cobra en efectivo.";
                    txtEfectivo.Focus();
                    return;
                }
            }

            Pagos = new List<PagoVenta>
            {
                new PagoVenta { MetodoPago = m1, Monto = monto1, ReferenciaPago = txtRef1.Text.Trim() },
                new PagoVenta { MetodoPago = m2, Monto = monto2, ReferenciaPago = txtRef2.Text.Trim() }
            };
            EfectivoRecibido = entrega;
            DialogResult = DialogResult.OK;
        }
    }
}