using BelisaEntidad;

namespace BelisaDatos
{
    public class CredencialUsuario
    {
        public Usuario Usuario { get; set; }
        public string ClaveHash { get; set; }
        public bool Bloqueado { get; set; }
    }
}