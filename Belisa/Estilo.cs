using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace Belisa
{
    public static class Estilo
    {
        // Paleta tomada del logo de Belisa
        public static readonly Color Lila = Color.FromArgb(174, 126, 234);         // #AE7EEA
        public static readonly Color LilaClaro = Color.FromArgb(204, 187, 232);    // #CCBBE8
        public static readonly Color LilaSuave = Color.FromArgb(243, 238, 250);    // fondos suaves
        public static readonly Color Dorado = Color.FromArgb(216, 186, 78);        // #D8BA4E
        public static readonly Color Acento = Color.FromArgb(142, 91, 201);        // botones
        public static readonly Color AcentoOscuro = Color.FromArgb(123, 75, 184);  // hover
        public static readonly Color Texto = Color.FromArgb(74, 69, 80);           // gris carbón
        public static readonly Color Fondo = Color.FromArgb(251, 249, 253);
        public static readonly Color Error = Color.FromArgb(192, 57, 43);
        private static Icon _icono;
        public static Icon IconoApp
        {
            get
            {
                if (_icono == null)
                {
                    using (var ms = new MemoryStream(Properties.Resources.BelisaIcono))
                        _icono = new Icon(ms);
                }
                return _icono;
            }
        }
        public static float EscalaFactor = 1.0f;
        public static readonly Font Normal = new Font("Segoe UI", 10.5f);
        public static readonly Font Negrita = new Font("Segoe UI", 10.5f, FontStyle.Bold);
        public static readonly Font Titulo = new Font("Segoe UI", 22f, FontStyle.Bold);

        public static Label Etiqueta(string texto, int x, int y)
        {
            return new Label
            {
                Text = texto,
                Location = new Point(x, y),
                AutoSize = true,
                Font = Normal,
                ForeColor = Texto
            };
        }

        public static TextBox Campo(int x, int y, int ancho, bool esClave = false)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Width = ancho,
                Font = Normal,
                BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Texto,
                UseSystemPasswordChar = esClave
            };
        }

        public static Button Boton(string texto, int x, int y, int ancho, int alto, bool primario)
        {
            var b = new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(ancho, alto),
                Font = primario ? Negrita : Normal,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = primario ? 0 : 1;
            b.FlatAppearance.BorderColor = Acento;
            b.BackColor = primario ? Acento : Color.White;
            b.ForeColor = primario ? Color.White : Acento;
            b.FlatAppearance.MouseOverBackColor = primario ? AcentoOscuro : LilaSuave;
            return b;
        }

        public static PictureBox Imagen(Image img, int x, int y, int ancho, int alto)
        {
            return new PictureBox
            {
                Image = img,
                Location = new Point(x, y),
                Size = new Size(ancho, alto),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };
        }

        // Franja de marca: lila arriba y una línea fina dorada debajo
        public static void AgregarFranja(Form f)
        {
            var lila = new Panel { Dock = DockStyle.Top, Height = 8, BackColor = Lila };
            var dorado = new Panel { Dock = DockStyle.Top, Height = 3, BackColor = Dorado };
            f.Controls.Add(dorado);
            f.Controls.Add(lila);
        }

        public static void ConfigurarVentana(Form f, int ancho, int alto, string titulo)
        {
            // Configuración base
            f.Text = titulo;
            // Evitar escalado automático por DPI del framework para controlar nosotros la escala
            f.AutoScaleMode = AutoScaleMode.None;
            // Inicialmente usar el tamaño objetivo sin escala
            f.ClientSize = new Size(ancho, alto);

            // Calcula factor de escala según el área de trabajo de la pantalla
            var area = Screen.PrimaryScreen.WorkingArea;
            double factorW = area.Width / (double)ancho;
            double factorH = area.Height / (double)alto;
            double factor = System.Math.Min(System.Math.Min(factorW, factorH), 1.0);
            EscalaFactor = (float)factor;

            if (EscalaFactor < 1f)
            {
                // Escalar controles existentes y reducir el tamaño del cliente
                f.Scale(new SizeF(EscalaFactor, EscalaFactor));
                f.ClientSize = new Size((int)(ancho * EscalaFactor), (int)(alto * EscalaFactor));
                f.AutoScroll = true; // permitir scroll si algo queda fuera
                f.FormBorderStyle = FormBorderStyle.Sizable;
                f.MaximizeBox = true;
                f.MinimizeBox = true;
            }
            else
            {
                f.ClientSize = new Size(ancho, alto);
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
            }

            f.StartPosition = FormStartPosition.CenterScreen;
            f.BackColor = Fondo;
            f.Icon = IconoApp;
        }
    }
}