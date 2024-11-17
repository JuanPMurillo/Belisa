using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using BELISA_POV.Utilidades;
using BelisaEntidad;
using BelisaNegocio;

namespace BELISA_POV
{
    public partial class frmUsuarios : Form
    {
        public frmUsuarios()
        {
            InitializeComponent();
        }

        private void frmUsuarios_Load(object sender, EventArgs e)
        {
            // List To Combobox Estado
            var dataSource = new List<OpcionCombo>();
            dataSource.Add(new OpcionCombo() { Texto = "Activo", Valor = "1"});
            dataSource.Add(new OpcionCombo() { Texto = "No Activo", Valor = "0"});

            this.cboestado.DataSource = dataSource;
            this.cboestado.DisplayMember = "Texto";
            this.cboestado.ValueMember = "Valor";
            this.cboestado.DropDownStyle = ComboBoxStyle.DropDownList;

            // List To Combobox Rol
            List<Rol> listaRol = new BN_Rol().Listar();
            foreach (Rol item in listaRol)
            {
                cborol.Items.Add(new OpcionCombo() { Valor = item.IdRol, Texto = item.Descripcion });
                cborol.DisplayMember = "Texto";
                cborol.ValueMember = "Valor";
                cborol.SelectedIndex = 0;
                this.cborol.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            // Comcobox Search to Datagirdview
            foreach (DataGridViewColumn columna in dgvdata.Columns)
            {
                if(columna.Visible == true && columna.Name != "btnseleccionar")
                {
                    cbosearch.Items.Add(new OpcionCombo() { Texto = columna.HeaderText, Valor = columna.Name });
                }
            }
            cbosearch.DisplayMember = "Texto";
            cbosearch.ValueMember = "Valor";
            cbosearch.SelectedIndex = 0;

            //SHOW THE ALL USERS
            List<Usuario> listuser = new BN_Usuario().Listar();
            foreach (Usuario item in listuser)
            {
                dgvdata.Rows.Add(new object[] {"",item.idUsuario,item.Documento,item.NombreCompleto,item.Correo,item.Clave,
                item.oRol.IdRol,
                item.oRol.Descripcion,
                item.Estado == true ? 1 : 0,
                item.Estado == true ? "Activo" : "No Activo"
                });
            }

        }

        private void btnguardar_Click(object sender, EventArgs e)
        {
            string mensaje = string.Empty;
            Usuario objusuario = new Usuario()
            {
                idUsuario = Convert.ToInt32(txtid.Text),
                Documento = txtdocumento.Text,
                NombreCompleto = txtnombrecompleto.Text,
                Correo = txtcorreo.Text,
                Clave = txtclave.Text,
                oRol = new Rol() { IdRol = Convert.ToInt32(((OpcionCombo)cborol.SelectedItem).Valor) },
                Estado = Convert.ToInt32(((OpcionCombo)cboestado.SelectedItem).Valor) == 1 ? true : false,
            };

            if(objusuario.idUsuario == 0)
            {
                // IF IS A NEW USER -- REGISTER
                int idnewuser = new BN_Usuario().Registrar(objusuario, out mensaje);
                if (idnewuser != 0)
                {
                    dgvdata.Rows.Add(new object[] {"",idnewuser,txtdocumento.Text,txtnombrecompleto.Text,txtcorreo.Text,txtclave.Text,
                    ((OpcionCombo)cborol.SelectedItem).Valor.ToString(),
                    ((OpcionCombo)cborol.SelectedItem).Texto.ToString(),
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
                // IF IS A OLD USER -- EDIT
                bool rest = new BN_Usuario().Editar(objusuario, out mensaje);
                if (rest)
                {
                    DataGridViewRow row = dgvdata.Rows[Convert.ToInt32(txtindice.Text)];
                    row.Cells["Id"].Value = txtid.Text;
                    row.Cells["Documento"].Value = txtdocumento.Text;
                    row.Cells["NombreCompleto"].Value = txtnombrecompleto.Text;
                    row.Cells["Correo"].Value = txtcorreo.Text;
                    row.Cells["Clave"].Value = txtclave.Text;
                    row.Cells["IdRol"].Value = ((OpcionCombo)cborol.SelectedItem).Valor.ToString();
                    row.Cells["Rol"].Value = ((OpcionCombo)cborol.SelectedItem).Texto.ToString();
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
            txtdocumento.Text = "";
            txtnombrecompleto.Text = "";
            txtcorreo.Text = "";
            txtclave.Text = "";
            txtconfirmarclave.Text = "";
            cborol.SelectedIndex = 0;
            cboestado.SelectedIndex = 0;
            txtdocumento.Select();
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
                    if (dgvdata.Rows[indice].Cells["Documento"].Value != null)
                        txtdocumento.Text = dgvdata.Rows[indice].Cells["Documento"].Value.ToString();
                    if (dgvdata.Rows[indice].Cells["NombreCompleto"].Value != null)
                        txtnombrecompleto.Text = dgvdata.Rows[indice].Cells["NombreCompleto"].Value.ToString();
                    if (dgvdata.Rows[indice].Cells["Correo"].Value != null)
                        txtcorreo.Text = dgvdata.Rows[indice].Cells["Correo"].Value.ToString();
                    if (dgvdata.Rows[indice].Cells["Clave"].Value != null)
                    {
                        txtclave.Text = dgvdata.Rows[indice].Cells["Clave"].Value.ToString();
                        txtconfirmarclave.Text = dgvdata.Rows[indice].Cells["Clave"].Value.ToString();
                    }

                    foreach (OpcionCombo oc in cborol.Items) 
                    {
                        if (Convert.ToInt32(oc.Valor) == Convert.ToInt32(dgvdata.Rows[indice].Cells["IdRol"].Value))
                        {
                            int indicecombo = cborol.Items.IndexOf(oc);
                            cborol.SelectedIndex = indicecombo;
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
                if(MessageBox.Show("¿Deseas Eliminar El Usuario?","Confirmacion",MessageBoxButtons.YesNo,MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    string mensaje = string.Empty;
                    Usuario objusuario = new Usuario()
                    {
                        idUsuario = Convert.ToInt32(txtid.Text),
                    };

                    bool answer = new BN_Usuario().Eliminar(objusuario, out mensaje);

                    if (answer) 
                    {
                        dgvdata.Rows.RemoveAt(Convert.ToInt32(txtindice.Text));
                    }
                    else
                    {
                        MessageBox.Show(mensaje,"Mensaje",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
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
            foreach(DataGridViewRow row in dgvdata.Rows)
            {
                row.Visible = true;
            }
            txtsearch.Select();
        }

        private void cboestado_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
 