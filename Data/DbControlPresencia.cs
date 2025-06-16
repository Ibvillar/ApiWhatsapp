using ApiRestDatosComunes.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.Data
{
    public class DbControlPresencia: DbContext
    {
        public DbControlPresencia(DbContextOptions<DbControlPresencia> options) : base(options) { }

        public DbSet<Movimientos> Movimientos { get; set; }
    }
}
