using ApiWhatsapp.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.Data
{
    public class DbTerceros: DbContext
    {
        public DbTerceros(DbContextOptions<DbTerceros> options) : base(options) {}

        public DbSet<Terceros> Terceros { get; set; }
    }
}
