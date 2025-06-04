using ApiWhatsapp.Data;
using ApiWhatsapp.Entities;
using ApiWhatsapp.Entitties;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.Repositories
{
    public class LocalizacionRepository
    {
        private readonly DbWhatsapp context;

        public LocalizacionRepository(DbWhatsapp _context)
        {
            context = _context;
        }

        public async Task<int> AddLocalizacion(Localizacion localizacion)
        {
            await context.AddAsync(localizacion);
            await context.SaveChangesAsync();

            return (GetLocalizacionById(localizacion.Id, localizacion.IdTelefono) == null) ? 1: -1;
        }

        public async Task<bool> UsuarioTieneLocalizacion(long telefonoId)
        {
            var ahora = DateOnly.FromDateTime(DateTime.UtcNow);

            var localizacionReciente = await context.Localizaciones
                .Where(x => x.IdTelefono == telefonoId && x.Dia == ahora)
                .FirstOrDefaultAsync();

            return localizacionReciente != null;
        }

        public async Task<Localizacion?> GetLocalizacionByPosicion(double longitud, double latitud, long telefonoId)
        {
            return await context.Localizaciones.FirstOrDefaultAsync(x => 
            x.Longitud == longitud && x.Latitud == latitud);
        }

        public async Task<Localizacion?> GetLocalizacionById(int Id, long telefonoId)
        {
            return await context.Localizaciones.FirstOrDefaultAsync(x => x.Id == Id);
        }

        public async Task<Localizacion?> GetLocalizacionByDia(DateOnly date, long telefonoId)
        {
            return await context.Localizaciones.Where(x =>
            x.Dia == date && x.IdTelefono == telefonoId).FirstOrDefaultAsync();
        }
    }
}
