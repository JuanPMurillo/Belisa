using System;

namespace BelisaEntidad
{
    public class Ajuste
    {
        public int IdAjuste { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string ProductoCodigo { get; set; }
        public string ProductoNombre { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public int Diferencia { get; set; }
        public string Motivo { get; set; }
        public string Nota { get; set; }
        public string UsuarioNombre { get; set; }
    }
}