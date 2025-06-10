using ApiWhatsapp.Data;
using ApiWhatsapp.Entities;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Repositorio para gestionar las operaciones relacionadas con los botones en la base de datos.
/// </summary>
public class BotonRepository
{
    private readonly DbWhatsapp context;

    public BotonRepository(DbWhatsapp context)
    {
        this.context = context;
    }

    /// <summary>
    /// Obtiene un botón por su ID.
    /// </summary>
    /// <param name="id">ID del botón a buscar.</param>
    /// <returns>El objeto Boton si se encuentra; de lo contrario, null.</returns>
    public async Task<Boton?> GetBotonById(int id)
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

    /// <summary>
    /// Obtiene la lista de todos los botones disponibles.
    /// </summary>
    /// <returns>Lista de objetos Boton; o null en caso de error.</returns>
    public async Task<List<Boton>> GetBotones()
    {
        try
        {
            return await context.Botones.ToListAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null!;
        }
    }

    public static int GetBotonId(string boton)
    {
        switch (boton)
        {
            case "iniciar_jornada":
                return 1;

            case "pausar_jornada":
                return 2;

            case "reaunudar_jornada":
                return 3;

            case "finalizar_jornada":
                return 4;

            default:
                return 0;
        }
    }
}
