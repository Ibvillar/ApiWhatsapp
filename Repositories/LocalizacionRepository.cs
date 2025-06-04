using ApiWhatsapp.Data;
using ApiWhatsapp.Entities;
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

        /// <summary>
        /// Añade una nueva localización a la base de datos.
        /// </summary>
        /// <param name="localizacion">Objeto Localizacion a agregar.</param>
        /// <returns>Devuelve 1 si la localización no se encontró después de agregarla, -1 en caso contrario.</returns>
        public async Task<int> AddLocalizacion(Localizacion localizacion)
        {
            await context.AddAsync(localizacion);
            await context.SaveChangesAsync();

            return (GetLocalizacionById(localizacion.Id, localizacion.IdTelefono) == null) ? 1 : -1;
        }

        /// <summary>
        /// Verifica si un usuario tiene una localización registrada para el día actual.
        /// </summary>
        /// <param name="telefonoId">ID del teléfono del usuario.</param>
        /// <returns>Devuelve true si existe una localización para el día actual, false en caso contrario.</returns>
        public async Task<bool> UsuarioTieneLocalizacion(long telefonoId)
        {
            var ahora = DateOnly.FromDateTime(DateTime.UtcNow);

            var localizacionReciente = await (
                from loc in context.Localizaciones
                join tel in context.Telefonos
                    on loc.IdTelefono equals tel.Id
                where loc.IdTelefono == telefonoId && loc.Dia == ahora
                && tel.ubicacion == true
                select loc
            ).FirstOrDefaultAsync();

            return localizacionReciente != null;
        }

        /// <summary>
        /// Obtiene una localización específica por sus coordenadas y teléfono asociado.
        /// </summary>
        /// <param name="longitud">Longitud geográfica de la localización.</param>
        /// <param name="latitud">Latitud geográfica de la localización.</param>
        /// <param name="telefonoId">ID del teléfono asociado a la localización.</param>
        /// <returns>Devuelve la localización encontrada o null si no existe.</returns>
        public async Task<Localizacion?> GetLocalizacionByPosicion(double longitud, double latitud, long telefonoId)
        {
            return await context.Localizaciones.FirstOrDefaultAsync(x =>
            x.Longitud == longitud && x.Latitud == latitud);
        }

        /// <summary>
        /// Obtiene una localización por su ID y teléfono asociado.
        /// </summary>
        /// <param name="Id">ID de la localización.</param>
        /// <param name="telefonoId">ID del teléfono asociado.</param>
        /// <returns>Devuelve la localización encontrada o null si no existe.</returns>
        public async Task<Localizacion?> GetLocalizacionById(int Id, long telefonoId)
        {
            return await context.Localizaciones.FirstOrDefaultAsync(x => x.Id == Id);
        }

        /// <summary>
        /// Obtiene una localización por fecha y teléfono asociado.
        /// </summary>
        /// <param name="date">Fecha (día) de la localización.</param>
        /// <param name="telefonoId">ID del teléfono asociado.</param>
        /// <returns>Devuelve la localización encontrada o null si no existe.</returns>
        public async Task<Localizacion?> GetLocalizacionByDia(DateOnly date, long telefonoId)
        {
            return await context.Localizaciones.Where(x =>
            x.Dia == date && x.IdTelefono == telefonoId).FirstOrDefaultAsync();
        }
    }
}
