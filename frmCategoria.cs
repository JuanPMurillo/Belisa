﻿using BELISA_POV.Utilidades;
using BelisaEntidad;
using BelisaNegocio;
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
    public partial class frmCategoria : Form
    {
        public frmCategoria()
        {
            InitializeComponent();
        }

        private void frmCategoria_Load(object sender, EventArgs e)
        {
            // List To Combobox Estado
            var dataSource = new List<OpcionCombo>();
            dataSource.Add(new OpcionCombo() { Texto = "Activo", Valor = "1" });
            dataSource.Add(new OpcionCombo() { Texto = "No Activo", Valor = "0" });

            this.cboestado.DataSource = dataSource;
            this.cboestado.DisplayMember = "Texto";
            this.cboestado.ValueMember = "Valor";
            this.cboestado.DropDownStyle = ComboBoxStyle.DropDownList;

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

            //SHOW THE ALL CATEGORYS
            List<Categoria> listcategory = new BN_Categoria().Listar();
            foreach (Categoria item in listcategory)
            {
                dgvdata.Rows.Add(new object[] {"",item.IdCategoria,
                item.Descripcion,
                item.Estado == true ? 1 : 0,
                item.Estado == true ? "Activo" : "No Activo"
                });
            }
        }

        private void btnguardar_Click(object sender, EventArgs e)
        {
            string mensaje = string.Empty;
            Categoria objCategoria = new Categoria()
            {
                IdCategoria = Convert.ToInt32(txtid.Text),
                Descripcion = txtdescripcion.Text,
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false,
            };

            if (objCategoria.IdCategoria == 0)
            {
                // IF IS A NEW CATEGORY -- ADD
                int idaddcategory = new BN_Categoria().Registrar(objCategoria, out mensaje);
                if (idaddcategory != 0)
                {
                    dgvdata.Rows.Add(new object[] {"",idaddcategory, txtdescripcion.Text,
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
                // IF IS A OLD CATEGORY -- EDIT
                bool rest = new BN_Categoria().Editar(objCategoria, out mensaje);
                if (rest)
                {
                    DataGridViewRow row = dgvdata.Rows[Convert.ToInt32(txtindice.Text)];
                    row.Cells["Id"].Value = txtid.Text;
                    row.Cells["Descripcion"].Value = txtdescripcion.Text;
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
            txtdescripcion.Text = "";
            cboestado.SelectedIndex = 0;
            txtdescripcion.Select();
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
                    if (dgvdata.Rows[indice].Cells["Descripcion"].Value != null)
                        txtdescripcion.Text = dgvdata.Rows[indice].Cells["Descripcion"].Value.ToString();

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
                if (MessageBox.Show("¿Deseas Eliminar La Categoria?", "Confirmacion", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string mensaje = string.Empty;
                    Categoria objCategoria = new Categoria()
                    {
                        IdCategoria = Convert.ToInt32(txtid.Text),
                    };

                    bool answer = new BN_Categoria().Eliminar(objCategoria, out mensaje);

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
    }
}
