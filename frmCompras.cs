using BELISA_POV.Modales;
using BELISA_POV.Utilidades;
using BelisaEntidad;
using BelisaNegocio;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BELISA_POV
{
    public partial class frmCompras : Form
    {
        private Usuario _Usuario;

        public frmCompras(Usuario oUsuario = null)
        {
            _Usuario = oUsuario;
            InitializeComponent();
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmCompras_Load(object sender, EventArgs e)
        {
            dgvdata.AllowUserToAddRows = false;
            var dataSource = new List<OpcionCombo>();
            dataSource.Add(new OpcionCombo() { Texto = "Factura", Valor = "0" });
            dataSource.Add(new OpcionCombo() { Texto = "Boleta", Valor = "1" });

            this.cbotipodocumento.DataSource = dataSource;
            this.cbotipodocumento.DisplayMember = "Texto";
            this.cbotipodocumento.ValueMember = "Valor";
            this.cbotipodocumento.DropDownStyle = ComboBoxStyle.DropDownList;

            txtfecha.Text = DateTime.Now.ToString("dd/MM/yyyy");

            txtidproducto.Text = "0";
            txtidproveedor.Text = "0";


        }

        private void btnsearchproveedor_Click(object sender, EventArgs e)
        {
            using (var modal = new mdProveedor())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    txtidproveedor.Text = modal._Proveedor.IdProveedor.ToString();
                    txtdocumentoproveedor.Text = modal._Proveedor.Documento;
                    txtrazonsocial.Text = modal._Proveedor.RazonSocial;
                }
                else
                {
                    txtdocumentoproveedor.Select();
                }
            }
        }

        private void btnbuscarproducto_Click(object sender, EventArgs e)
        {
            using (var modal = new mdProducto())
            {
                var result = modal.ShowDialog();

                if (result == DialogResult.OK)
                {
                    txtidproducto.Text = modal._Producto.IdProducto.ToString();
                    txtcodproducto.Text = modal._Producto.Codigo;
                    txtproducto.Text = modal._Producto.Nombre;
                    txtpreciocompra.Select();
                }
                else
                {
                    txtcodproducto.Select();
                }
            }
        }

        private void txtcodproducto_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                Producto oProducto = new BN_Producto().Listar().Where(p => p.Codigo == txtcodproducto.Text && p.Estado == true).FirstOrDefault();
                if (oProducto != null)
                {
                    txtcodproducto.BackColor = System.Drawing.Color.Honeydew;
                    txtidproducto.Text = oProducto.IdProducto.ToString();
                    txtproducto.Text = oProducto.Nombre;
                    txtpreciocompra.Select();
                }
                else
                {
                    txtcodproducto.BackColor = System.Drawing.Color.MistyRose;
                    txtidproducto.Text = "0";
                    txtproducto.Text = "";
                }
            }
        }

        private void btnagregarproducto_Click(object sender, EventArgs e)
        {
            decimal preciocompra = 0;
            decimal precioventa = 0;
            bool producto_existe = false;

            if (int.Parse(txtidproducto.Text) == 0)
            {
                MessageBox.Show("Debe Seleccionar Un Producto", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            if (!decimal.TryParse(txtpreciocompra.Text, out preciocompra))
            {
                MessageBox.Show("Precio Compra - Formato Moneda Incorrecta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtpreciocompra.Select();
                return;
            }
            if (!decimal.TryParse(txtprecioventa.Text, out precioventa))
            {
                MessageBox.Show("Precio Venta - Formato Moneda Incorrecta", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                txtpreciocompra.Select();
                return;
            }
            foreach (DataGridViewRow fila in dgvdata.Rows)
            {
                if (fila.Cells["IdProducto"].Value != null && fila.Cells["IdProducto"].Value.ToString() == txtidproducto.Text)
                {
                    producto_existe = true;
                    break;
                }
            }
            if (!producto_existe)
            {
                dgvdata.Rows.Add(new object[]
                {
                    txtidproducto.Text,
                    txtproducto.Text,
                    preciocompra.ToString("0.00"),
                    precioventa.ToString("0.00"),
                    txtcantidad.Value.ToString(),
                    (txtcantidad.Value * preciocompra).ToString("0.00")
                });
                calcularTotal();
                LimpiarProducto();
                txtcodproducto.Select();
            }
        }
        private void LimpiarProducto()
        {
            txtidproducto.Text = "0";
            txtcodproducto.Text = "";
            txtcodproducto.BackColor = System.Drawing.Color.White;
            txtproducto.Text = "";
            txtpreciocompra.Text = "";
            txtprecioventa.Text = "";
            txtcantidad.Value = 1;
        }
        private void calcularTotal()
        {
            decimal total = 0;

            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    if (row.Cells["SubTotal"].Value != null && decimal.TryParse(row.Cells["SubTotal"].Value.ToString(), out decimal subTotal))
                    {
                        total += subTotal;
                    }
                }
            }
            txttotal.Text = total.ToString("0.00");
        }

        private void dgvdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (e.ColumnIndex == 6)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                var w = Properties.Resources.delete20.Width;
                var h = Properties.Resources.delete20.Height;
                var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;
                e.Graphics.DrawImage(Properties.Resources.delete20, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }

        private void dgvdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvdata.Columns[e.ColumnIndex].Name == "btneliminar")
            {
                int indice = e.RowIndex;

                if (indice >= 0)
                {
                    // Confirmar cambios en la fila en caso de que esté en modo de edición
                    dgvdata.EndEdit();

                    // Confirmar cambios en la celda seleccionada
                    dgvdata.CurrentCell = null;

                    // Mostrar cuadro de confirmación antes de eliminar
                    var confirmResult = MessageBox.Show("¿Estás seguro de que deseas eliminar esta fila?", "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (confirmResult == DialogResult.Yes)
                    {
                        dgvdata.Rows.RemoveAt(indice);
                    }
                }
            }
        }

        private void txtpreciocompra_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir solo números y un punto decimal
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false;  // Permitir el número
            }
            else if (e.KeyChar == '.')
            {
                // Verificar si ya hay un punto en el texto
                if (txtpreciocompra.Text.Contains("."))
                {
                    e.Handled = true;  // No permitir el segundo punto
                }
                else
                {
                    e.Handled = false;  // Permitir el primer punto
                }
            }
            else if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;  // Permitir teclas de control como Backspace
            }
            else
            {
                e.Handled = true;  // Bloquear cualquier otro carácter
            }
        }

        private void txtprecioventa_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir solo números y un punto decimal
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false;  // Permitir el número
            }
            else if (e.KeyChar == '.')
            {
                // Verificar si ya hay un punto en el texto
                if (txtprecioventa.Text.Contains("."))
                {
                    e.Handled = true;  // No permitir el segundo punto
                }
                else
                {
                    e.Handled = false;  // Permitir el primer punto
                }
            }
            else if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;  // Permitir teclas de control como Backspace
            }
            else
            {
                e.Handled = true;  // Bloquear cualquier otro carácter
            }
        }
    }
}
