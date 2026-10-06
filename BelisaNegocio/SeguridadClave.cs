using System;
using System.Security.Cryptography;

namespace BelisaNegocio
{
    public static class SeguridadClave
    {
        private const int Iteraciones = 100000;
        private const int TamSalt = 16;
        private const int TamHash = 32;

        // Hash de relleno: se usa cuando el usuario no existe, para que
        // tardar lo mismo no delate qué usuarios están registrados
        public static readonly string HashFalso = Hashear("relleno-no-valido");

        public static string Hashear(string clave)
        {
            byte[] salt = new byte[TamSalt];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(salt);

            byte[] hash = Derivar(clave, salt, Iteraciones);
            return "PBKDF2$" + Iteraciones + "$" +
                   Convert.ToBase64String(salt) + "$" +
                   Convert.ToBase64String(hash);
        }

        public static bool Verificar(string clave, string almacenado)
        {
            if (string.IsNullOrEmpty(almacenado)) return false;

            string[] p = almacenado.Split('$');
            if (p.Length != 4 || p[0] != "PBKDF2") return false;

            try
            {
                int iteraciones = int.Parse(p[1]);
                byte[] salt = Convert.FromBase64String(p[2]);
                byte[] esperado = Convert.FromBase64String(p[3]);
                byte[] calculado = Derivar(clave, salt, iteraciones);
                return IgualesEnTiempoConstante(calculado, esperado);
            }
            catch (FormatException) { return false; }      // hash guardado con formato inválido
            catch (OverflowException) { return false; }
        }

        private static byte[] Derivar(string clave, byte[] salt, int iteraciones)
        {
            using (var kdf = new Rfc2898DeriveBytes(clave, salt, iteraciones, HashAlgorithmName.SHA256))
                return kdf.GetBytes(TamHash);
        }

        private static bool IgualesEnTiempoConstante(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diferencia = 0;
            for (int i = 0; i < a.Length; i++)
                diferencia |= a[i] ^ b[i];
            return diferencia == 0;
        }
    }
}