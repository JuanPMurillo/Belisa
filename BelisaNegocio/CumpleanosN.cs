using System;

namespace BelisaNegocio
{
    // Cumpleaños de los clientes: se guarda solo el día y el mes, sin el año
    public static class CumpleanosN
    {
        public static readonly string[] Meses =
        {
            "enero", "febrero", "marzo", "abril", "mayo", "junio",
            "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
        };

        // Sin fecha (los dos vacíos) es válido; con fecha, tiene que existir (no hay 31 de febrero)
        public static bool EsValido(int? dia, int? mes)
        {
            if (!dia.HasValue && !mes.HasValue) return true;
            if (!dia.HasValue || !mes.HasValue) return false;
            if (mes.Value < 1 || mes.Value > 12) return false;

            // Se usa el año 2000 porque es bisiesto: el 29 de febrero sí es una fecha posible
            return dia.Value >= 1 && dia.Value <= DateTime.DaysInMonth(2000, mes.Value);
        }

        // "15 de marzo", o vacío si el cliente no dio su cumpleaños
        public static string Texto(int? dia, int? mes)
        {
            if (!dia.HasValue || !mes.HasValue || !EsValido(dia, mes)) return "";
            return dia.Value + " de " + Meses[mes.Value - 1];
        }

        // ¿Cumple años hoy? Quien nació un 29 de febrero lo celebra el 28 en los años que no son bisiestos
        public static bool EsHoy(int? dia, int? mes)
        {
            if (!dia.HasValue || !mes.HasValue || !EsValido(dia, mes)) return false;

            DateTime hoy = DateTime.Today;
            int d = dia.Value;
            if (mes.Value == 2 && d == 29 && !DateTime.IsLeapYear(hoy.Year)) d = 28;

            return mes.Value == hoy.Month && d == hoy.Day;
        }

        public static bool EsEsteMes(int? mes)
        {
            return mes.HasValue && mes.Value == DateTime.Today.Month;
        }
    }
}