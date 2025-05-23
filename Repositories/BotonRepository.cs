using ApiWhatsapp.Data;
using ApiWhatsapp.Entities;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.Repositories
{
    public class BotonRepository
    {

        private readonly DbWhatsapp context;
        public BotonRepository(DbWhatsapp context) 
        {
            this.context = context;
        }

        public async Task<Boton> GetBotonById(int id)
        {
            try
            {
                return await context.Botones.FirstOrDefaultAsync(x => x.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public async Task<List<Boton>> GetBotones()
        {
            try
            {
                return await context.Botones.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
