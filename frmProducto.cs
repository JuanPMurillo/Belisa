using BELISA_POV.Utilidades;
using BelisaEntidad;
using BelisaNegocio;
using ClosedXML.Excel;
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
    public partial class frmProducto : Form
    {
        public frmProducto()
        {
            InitializeComponent();
        }

        private void frmProducto_Load(object sender, EventArgs e)
        {
            // List To Combobox Estado
            var dataSource = new List<OpcionCombo>();
            dataSource.Add(new OpcionCombo() { Texto = "Activo", Valor = "1" });
            dataSource.Add(new OpcionCombo() { Texto = "No Activo", Valor = "0" });

            this.cboestado.DataSource = dataSource;
            this.cboestado.DisplayMember = "Texto";
            this.cboestado.ValueMember = "Valor";
            this.cboestado.DropDownStyle = ComboBoxStyle.DropDownList;

            // List To Combobox Categoria
            List<Categoria> listaCategoria = new BN_Categoria().Listar();
            foreach (Categoria item in listaCategoria)
            {
                cbocategoria.Items.Add(new OpcionCombo() { Valor = item.IdCategoria, Texto = item.Descripcion });
                cbocategoria.DisplayMember = "Texto";
                cbocategoria.ValueMember = "Valor";
                cbocategoria.SelectedIndex = 0;
                this.cbocategoria.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            // Comcobox Search to Datagirdview
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                if (columna.Visible == true && columna.Name != "btnseleccionar")
                {
                    cbosearch.Items.Add(new OpcionCombo() { Texto = columna.HeaderText, Valor = columna.Name });
                }
            }
            cbosearch.DisplayMember = "Texto";
            cbosearch.ValueMember = "Valor";
            cbosearch.SelectedIndex = 0;

            //SHOW THE ALL PRODUCTS
            List<Producto> listproduct = new BN_Producto().Listar();
            foreach (Producto item in listproduct)
            {
                dgvdata.Rows.Add(new object[] {"",
                item.IdProducto,
                item.Codigo,
                item.Nombre,
                item.Descripcion,
                item.oCategoria.IdCategoria,
                item.oCategoria.Descripcion,
                item.Stock,
                item.PrecioCompra,
                item.PrecioVenta,
                item.Estado == true ? 1 : 0,
                item.Estado == true ? "Activo" : "No Activo"
                });
            }

        }

        private void btnguardar_Click(object sender, EventArgs e)
        {
            string mensaje = string.Empty;
            Producto objProducto = new Producto()
            {
                IdProducto = Convert.ToInt32(txtid.Text),
                Codigo = txtcodigo.Text,
                Nombre = txtnombre.Text,
                Descripcion = txtdescripcion.Text,
                oCategoria = new Categoria() { IdCategoria = Convert.ToInt32(((OpcionCombo)cbocategoria.SelectedItem).Valor) },
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false,
            };

            if (objProducto.IdProducto == 0)
            {
                // IF IS A NEW PRODUCT -- ADD
                int idnewproduct = new BN_Producto().Registrar(objProducto, out mensaje);
                if (idnewproduct != 0)
                {
                    dgvdata.Rows.Add(new object[] {"",
                    idnewproduct,
                    txtcodigo.Text,
                    txtnombre.Text,
                    txtdescripcion.Text,
                    ((OpcionCombo)cbocategoria.SelectedItem).Valor.ToString(),
                    ((OpcionCombo)cbocategoria.SelectedItem).Texto.ToString(),
                    "0",
                    "0.00",
                    "0.00",
                    ((OpcionCombo)cboestado.SelectedItem).Valor.ToString(),
                    ((OpcionCombo)cboestado.SelectedItem).Texto.ToString()
                    });
                    Limpiar();
                }
                else
                {
                    MessageBox.Show(mensaje);
                }

            }
            else
            {
                // IF IS A OLD PRODUCT -- EDIT
                bool rest = new BN_Producto().Editar(objProducto, out mensaje);
                if (rest)
                {
                    DataGridViewRow row = dgvdata.Rows[Convert.ToInt32(txtindice.Text)];
                    row.Cells["Id"].Value = txtid.Text;
                    row.Cells["Codigo"].Value = txtcodigo.Text;
                    row.Cells["Nombre"].Value = txtnombre.Text;
                    row.Cells["Descripcion"].Value = txtdescripcion.Text;
                    row.Cells["IdCategoria"].Value = ((OpcionCombo)cbocategoria.SelectedItem).Valor.ToString();
                    row.Cells["Categoria"].Value = ((OpcionCombo)cbocategoria.SelectedItem).Texto.ToString();
                    row.Cells["EstadoValor"].Value = ((OpcionCombo)cboestado.SelectedItem).Valor.ToString();
                    row.Cells["Estado"].Value = ((OpcionCombo)cboestado.SelectedItem).Texto.ToString();
                    Limpiar();
                }
                else
                {
                    MessageBox.Show(mensaje);
                }
            }
        }
        private void Limpiar()
        {
            txtindice.Text = "-1";
            txtid.Text = "0";
            txtcodigo.Text = "";
            txtdescripcion.Text = "";
            txtnombre.Text = "";
            cboestado.SelectedIndex = 0;
            cboestado.SelectedIndex = 0;
            txtcodigo.Select();
        }

        private void dgvdata_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            if (e.ColumnIndex == 0)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                var w = Properties.Resources.check20.Width;
                var h = Properties.Resources.check20.Height;
                var x = e.CellBounds.Left + (e.CellBounds.Width - w) / 2;
                var y = e.CellBounds.Top + (e.CellBounds.Height - h) / 2;
                e.Graphics.DrawImage(Properties.Resources.check, new Rectangle(x, y, w, h));
                e.Handled = true;
            }
        }

        private void dgvdata_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvdata.Columns[e.ColumnIndex].Name == "btnseleccionar")
            {
                int indice = e.RowIndex;

                if (indice >= 0)
                {
                    txtindice.Text = indice.ToString();

                    // Make sure cells have non - null values
                    if (dgvdata.Rows[indice].Cells["id"].Value != null)
                        txtid.Text = dgvdata.Rows[indice].Cells["id"].Value.ToString();
                    if (dgvdata.Rows[indice].Cells["Codigo"].Value != null)
                        txtcodigo.Text = dgvdata.Rows[indice].Cells["Codigo"].Value.ToString();
                    if (dgvdata.Rows[indice].Cells["Nombre"].Value != null)
                        txtnombre.Text = dgvdata.Rows[indice].Cells["Nombre"].Value.ToString();
                    if (dgvdata.Rows[indice].Cells["Descripcion"].Value != null)
                        txtdescripcion.Text = dgvdata.Rows[indice].Cells["Descripcion"].Value.ToString();
                    foreach (OpcionCombo oc in cbocategoria.Items)
                    {
                        if (Convert.ToInt32(oc.Valor) == Convert.ToInt32(dgvdata.Rows[indice].Cells["IdCategoria"].Value))
                        {
                            int indicecombo = cbocategoria.Items.IndexOf(oc);
                            cbocategoria.SelectedIndex = indicecombo;
                            break;
                        }
                    }

                    foreach (OpcionCombo oc in cboestado.Items)
                    {
                        if (Convert.ToInt32(oc.Valor) == Convert.ToInt32(dgvdata.Rows[indice].Cells["EstadoValor"].Value))
                        {
                            int indicecombo = cboestado.Items.IndexOf(oc);
                            cboestado.SelectedIndex = indicecombo;
                            break;
                        }
                    }
                }
            }
        }

        private void btneliminar_Click(object sender, EventArgs e)
        {
            if (Convert.ToInt32(txtid.Text) != 0)
            {
                if (MessageBox.Show("¿Deseas Eliminar El Producto?", "Confirmacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string mensaje = string.Empty;
                    Producto objProducto = new Producto()
                    {
                        IdProducto = Convert.ToInt32(txtid.Text),
                    };

                    bool answer = new BN_Producto().Eliminar(objProducto, out mensaje);

                    if (answer)
                    {
                        dgvdata.Rows.RemoveAt(Convert.ToInt32(txtindice.Text));
                    }
                    else
                    {
                        MessageBox.Show(mensaje, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    Limpiar();
                }
            }
        }

        private void btneditar_Click(object sender, EventArgs e)
        {
            Limpiar();
        }

        private void btnsearch_Click(object sender, EventArgs e)
        {
            string filtercolum = ((OpcionCombo)cbosearch.SelectedItem).Valor.ToString();

            if (dgvdata.Rows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    if (row.Cells[filtercolum].Value.ToString().Trim().ToUpper().Contains(txtsearch.Text.Trim().ToUpper()))
                    {
                        row.Visible = true;
                    }
                    else { row.Visible = false; }
                }
            }
        }

        private void btnclear_Click(object sender, EventArgs e)
        {
            txtsearch.Text = "";
            foreach (DataGridViewRow row in dgvdata.Rows)
            {
                row.Visible = true;
            }
            txtsearch.Select();
        }

        private void btnexcel_Click(object sender, EventArgs e)
        {
            if (dgvdata.Rows.Count < 1)
            {
                MessageBox.Show("No Hay Datos Para Exportar", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            else
            {
                DataTable dt = new DataTable();

                // Add visible columns
                foreach (DataGridViewColumn column in dgvdata.Columns)
                {
                    if (column.HeaderText != "" && column.Visible)
                    {
                        dt.Columns.Add(column.HeaderText, typeof(string));
                    }
                }

                // Add visible rows
                foreach (DataGridViewRow row in dgvdata.Rows)
                {
                    if (row.Visible)
                    {
                        dt.Rows.Add(new object[]
                        {
                    row.Cells[2].Value?.ToString(),  // Check if the value is not null
                    row.Cells[3].Value?.ToString(),
                    row.Cells[4].Value?.ToString(),
                    row.Cells[6].Value?.ToString(),
                    row.Cells[7].Value?.ToString(),
                    row.Cells[8].Value?.ToString(),
                    row.Cells[9].Value?.ToString(),
                    row.Cells[11].Value?.ToString(),
                        });
                    }
                }

                // Show the save file dialog only once
                SaveFileDialog savefile = new SaveFileDialog();
                savefile.FileName = string.Format("ReporteProductos_{0}.xlsx", DateTime.Now.ToString("ddMMyyyyHHmmss"));
                savefile.Filter = "Excel Files | *.xlsx";

                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        XLWorkbook wb = new XLWorkbook();
                        var hoja = wb.Worksheets.Add(dt, "Informe");
                        hoja.ColumnsUsed().AdjustToContents(); // Adjust the column widths to fit the content
                        wb.SaveAs(savefile.FileName); // Save the file
                        MessageBox.Show("Reporte Generado", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No Se Pudo Generar El Reporte: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
            }
        }
    } 
}