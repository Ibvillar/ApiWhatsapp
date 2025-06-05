using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entitties;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ApiWhatsapp.BBDD
{
    /// <summary>
    /// Repositorio para la gestión de entidades de tipo Teléfono.
    /// </summary>
    public class TelefonoRepository
    {
        private readonly DbWhatsapp context;
        private DbTerceros contextTerceros;
        private readonly IMapper mapper;

        /// <summary>
        /// Constructor del repositorio de teléfonos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos</param>
        /// <param name="mapper">Instancia de AutoMapper</param>
        public TelefonoRepository(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper)
        {
            this.context = context;
            this.contextTerceros = contextTerceros;
            this.mapper = mapper;
        }

        /// <summary>
        /// Agrega un teléfono a la base de datos.
        /// </summary>
        /// <param name="telefono">Teléfono a agregar</param>
        /// <returns>True si se ha insertado correctamente, False en caso contrario</returns>
        public async Task<bool> AddTelefono(Telefono telefono)
        {
            Console.WriteLine("1111111111111");
            try
            {
                TercerosDTO tercerosDTO = await GetTelefonoIdTerceros(telefono.Numero);
                telefono.IdTerceros = -1;

                if (tercerosDTO.Id != -1)
                {
                    telefono.Nombre = tercerosDTO.Nombre;
                    telefono.IdTerceros = tercerosDTO.Id;
                }

                context.Telefonos.Add(telefono);
                await context.SaveChangesAsync();

                if (await GetTelefonosById(telefono.Id) is null)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Actualiza el nombre asociado a un objeto Telefono en la base de datos.
        /// </summary>
        /// <param name="telefono">Instancia existente del teléfono que se desea actualizar.</param>
        /// <param name="nombre">Nuevo nombre que se desea asignar.</param>
        /// <returns>True si la operación fue exitosa; false si ocurrió una excepción.</returns>
        public async Task<bool> SetNombre(Telefono telefono, string nombre)
        {
            try
            {
                // Asigna el nuevo nombre al objeto telefono
                telefono.Nombre = nombre;

                // Marca el objeto como modificado para que EF lo actualice en la base de datos
                context.Telefonos.Update(telefono);

                // Guarda los cambios de forma asíncrona
                await context.SaveChangesAsync();

                // Si todo va bien, retorna true
                return true;
            }
            catch (Exception ex)
            {
                // Corrige el ToString que estaba mal (olvidaste los paréntesis)
                Console.WriteLine($"Error: {ex.ToString()}");

                // Retorna false en caso de error
                return false;
            }
        }

        /// <summary>
        /// Obtiene una lista de todos los teléfonos.
        /// </summary>
        /// <returns>Lista de teléfonos. Lista vacía si no hay ninguno</returns>
        public async Task<List<Telefono>> GetTelefonos()
        {
            List<Telefono> telefonos = [];
            try
            {
                telefonos = await context.Telefonos.ToListAsync();
                return telefonos;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return telefonos!;
            }
        }

        /// <summary>
        /// Obtiene un teléfono a partir de su ID.
        /// </summary>
        /// <param name="Id">ID del teléfono</param>
        /// <returns>Objeto Teléfono si se encuentra, null en caso contrario</returns>
        public async Task<Telefono?> GetTelefonosById(long Id)
        {
            try
            {
                return await context.Telefonos.FirstOrDefaultAsync(x => x.Id == Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            
        }

        /// <summary>
        /// Construye un objeto Teléfono a partir de sus atributos.
        /// </summary>
        /// <param name="numero">Número del teléfono</param>
        /// <param name="prefijo">Prefijo del país</param>
        /// <param name="nombre">Nombre asociado al teléfono</param>
        /// <returns>Objeto Teléfono</returns>
        public Telefono ConstruirTelefono(int numero, short prefijo, string nombre)
        {
            TelefonoDTO telefono = new TelefonoDTO
            {
                Numero = numero,
                Prefijo = prefijo,
                Nombre = nombre,
            };

            Telefono telefonoCompleto = mapper.Map<Telefono>(telefono);
            telefonoCompleto.IdTerceros = -1;
            telefonoCompleto.IdGenerales = "";
            telefonoCompleto.Token = "";
            telefonoCompleto.ubicacion = false;

            return telefonoCompleto;
        }

        /// <summary>
        /// Elimina un teléfono de la base de datos por su ID.
        /// </summary>
        /// <param name="telefonoId">ID del teléfono a eliminar</param>
        /// <returns>True si fue eliminado, False si no se encontró</returns>
        public async Task<bool> RemoveTelefono(long telefonoId)
        {
            var telefono = context.Telefonos.FirstOrDefault(x => x.Id == telefonoId);

            if (telefono != null)
            {
                context.Telefonos.Remove(telefono);
                await context.SaveChangesAsync();
                return true;
            }
            else
            {
                Console.WriteLine("Teléfono no encontrado.");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el nombre y ID de un tercero en base al número de teléfono proporcionado.
        /// </summary>
        /// <param name="telefono">Número de teléfono a buscar en las columnas TF1_002 y TF2_002 de la tabla TEF00002.</param>
        /// <returns>Un objeto TercerosDTO con el Id y Nombre del tercero asociado al teléfono, o valores predeterminados si no se encuentra.</returns>
        public async Task<TercerosDTO> GetTelefonoIdTerceros(int telefono)
        {
            string query = @"SELECT TEF.IDE_002 AS Id, Generales.dbo.DesencriptarCadena(TEF.NOM_002) AS Nombre FROM TEF00002 AS TEF WHERE TEF.TF1_002 = @telefono1 OR TEF.TF2_002 = @telefono2";

            TercerosDTO? result = null;

            try
            {
                result = await contextTerceros.TercerosDTOs
                                .FromSqlRaw(query, 
                                    new SqlParameter("@telefono1", telefono.ToString()), 
                                    new SqlParameter("@telefono2", telefono.ToString()))
                                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            if (result is null)
            {
                TercerosDTO tercerosDTO = new TercerosDTO();
                tercerosDTO.Id = -1;
                tercerosDTO.Nombre = null;
                return tercerosDTO;
            }

            return result;
        }

        /// <summary>
        /// Valida si un teléfono existe en la base de datos; si no, lo crea y agrega.
        /// </summary>
        /// <param name="telefonoDTO">Objeto DTO que contiene los datos del teléfono a validar.</param>
        /// <returns>Una tarea asincrónica.</returns>
        public async Task ValidateNumber(TelefonoDTO telefonoDTO)
        {
            try
            {
                long id = long.Parse(telefonoDTO.Prefijo.ToString() + telefonoDTO.Numero.ToString());

                if (await GetTelefonosById(id) is null)
                {
                    Telefono telefono = ConstruirTelefono(telefonoDTO.Numero, short.Parse(telefonoDTO.Prefijo.ToString()), telefonoDTO.Nombre);
                    await AddTelefono(telefono);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.ToString());
            }
        }

        /// <summary>
        /// Asocia un código específico a un teléfono existente.
        /// </summary>
        /// <param name="telefono">Objeto Telefono que se actualizará con el código.</param>
        /// <param name="cod">Código que se asignará al teléfono.</param>
        /// <returns>Devuelve true si la operación fue exitosa.</returns>
        /// <exception cref="Exception">Lanza excepción si el teléfono no existe.</exception>
        public async Task<bool> AddCodigo(Telefono? telefono, string cod)
        {
            telefono = await context.Telefonos.Where(x => x.Id == telefono!.Id).FirstOrDefaultAsync();

            if (telefono is null)
            {
                throw new Exception("Este teleofono no existe");
            }

            telefono.IdGenerales = cod;
            await context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Actualiza el estado de ubicación de un teléfono.
        /// </summary>
        /// <param name="estado">Nuevo estado de la ubicación (true o false).</param>
        /// <param name="Id">ID del teléfono a actualizar.</param>
        /// <returns>Devuelve true si la actualización fue exitosa, false si el teléfono no existe.</returns>
        public async Task<bool> SetUbicacion(bool estado, long Id)
        {
            Telefono telefono = await GetTelefonosById(Id);

            if (telefono is null)
                return false;

            telefono.ubicacion = estado;
            await context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Actualiza el token de un teléfono dado su ID.
        /// </summary>
        /// <param name="Id">ID del teléfono a actualizar.</param>
        /// <param name="Token">Nuevo token que se asignará al teléfono.</param>
        /// <returns>Devuelve true si la actualización fue exitosa, false si el teléfono no existe.</returns>
        public async Task<bool> UpdateToken(long Id, string Token)
        {
            Telefono? telefono = await context.Telefonos.FirstOrDefaultAsync(x => x.Id == Id);

            if (telefono is null)
                return false;

            telefono.Token = Token;
            await context.SaveChangesAsync();
            return true;
        }

    }
}
