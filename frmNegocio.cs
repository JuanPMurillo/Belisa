using BelisaEntidad;
using BelisaNegocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BELISA_POV
{
    public partial class frmNegocio : Form
    {
        public frmNegocio()
        {
            InitializeComponent();
        }

        public Image ByteToImage(byte[] imagebytes)
        {
            if (imagebytes == null || imagebytes.Length == 0)
                throw new ArgumentException("El arreglo de bytes está vacío o es nulo.");

            using (MemoryStream ms = new MemoryStream(imagebytes))
            {
                return Image.FromStream(ms); // Usar Image.FromStream para cargar la imagen
            }
        }


        private void frmNegocio_Load(object sender, EventArgs e)
        {
            bool obtenido = true;

            // Load the logo if it exists
            byte[] byteimagen = new BN_Negocio().GetLogo(out obtenido);
            if (obtenido && byteimagen.Length > 0)
            {
                piclogo.Image = ByteToImage(byteimagen);
            }
            else
            {
                piclogo.Image = null; // Or you can assign a default image
            }

            // Load the business data
            Negocio datos = new BN_Negocio().GetDates();

            // Ensure the data is not null before assigning it
            if (datos != null)
            {
                txtnombre.Text = datos.Nombre;
                txtruc.Text = datos.RUC;
                txtdireccion.Text = datos.Direccion;
            }
        }

        private void btnsubir_Click(object sender, EventArgs e)
        {
            string mensaje = string.Empty;
            OpenFileDialog oOpenFileDialog = new OpenFileDialog();
            oOpenFileDialog.FileName = "Files|*.jpg;*.jpeg;*.png";
            if(oOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                byte[] byteimage = File.ReadAllBytes(oOpenFileDialog.FileName);
                bool respuesta = new BN_Negocio().UpdateLogo(byteimage,out mensaje);

                if(respuesta)
                    piclogo.Image = ByteToImage(byteimage);
                else
                    MessageBox.Show(mensaje,"Mensaje",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }
        }

        private void btnguardar_Click(object sender, EventArgs e)
        {
            string mensaje = string.Empty;

            Negocio obj = new Negocio()
            {
                IdNegocio = 1, // Asegúrate de que este ID es el correcto
                Nombre = txtnombre.Text,
                RUC = txtruc.Text,
                Direccion = txtdireccion.Text,
            };
            bool respuesta = new BN_Negocio().SaveDates(obj, out mensaje);
            if (respuesta)
                MessageBox.Show("Los Cambios Fueron Guardados", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(mensaje, "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}