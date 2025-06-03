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
            if (GetLocalizacionByPosicion(localizacion.Longitud, localizacion.Latitud, localizacion.IdTelefono) is not null)
                return 0;

            await context.AddAsync(localizacion);

            return (GetLocalizacionById(localizacion.Id, localizacion.IdTelefono) == null) ? 1: -1;
        }

        public async Task<bool> UsuarioTieneLocalizacionValida(long telefonoId)
        {
            var haceCuatroMinutos = DateTime.UtcNow.AddMinutes(-4);

            var localizacionReciente = await context.Localizaciones
                .Where(x => x.IdTelefono == telefonoId && x.UltimaActualizacion > haceCuatroMinutos)
                .FirstOrDefaultAsync();

            return localizacionReciente != null;
        }

        public async Task<bool> ActualizarTiempo(int Id)
        {
            Localizacion? localizacion = await context.Localizaciones.FirstOrDefaultAsync(x => x.Id == Id);

            localizacion!.UltimaActualizacion = DateTime.UtcNow;

            int result = await context.SaveChangesAsync();

            return result != 0;
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
    }
}
