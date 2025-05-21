using ApiWhatsapp.DTO;
using ApiWhatsapp.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.Data
{
    public class DbTerceros: DbContext
    {
        public DbTerceros(DbContextOptions<DbTerceros> options) : base(options) {}

        public DbSet<Terceros> Terceros { get; set; }
        public DbSet<TercerosDTO> TercerosDTOs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Terceros>().HasNoKey();
            modelBuilder.Entity<TercerosDTO>().HasNoKey();
        }
    }
}
