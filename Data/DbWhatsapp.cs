using ApiWhatsapp.Entities;
using ApiWhatsapp.Entitties;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.Data
{
    public class DbWhatsapp: DbContext
    {
        public DbWhatsapp(DbContextOptions<DbWhatsapp> options): base(options) {}

        public DbSet<Fichero> Ficheros { get; set; }

        public DbSet<Mensaje> Mensajes { get; set; }

        public DbSet<Telefono> Telefonos { get; set; }

        public DbSet<Prefijos> Prefijos { get; set; }
    }
}
