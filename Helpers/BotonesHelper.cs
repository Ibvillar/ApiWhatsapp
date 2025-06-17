using ApiRestDatosComunes.Entities;
using ApiWhatsapp.BBDD;
using ApiWhatsapp.Controller;
using ApiWhatsapp.Data;
using ApiWhatsapp.DTO;
using ApiWhatsapp.Entities;
using ApiWhatsapp.Entitties;
using ApiWhatsapp.Repositories;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiWhatsapp.Helpers
{
    /// <summary>
    /// Clase auxiliar que maneja la lógica relacionada con los botones interactivos de WhatsApp para control de jornada laboral.
    /// </summary>
    public class BotonesHelper
    {
        private readonly MensajesController _mensajeController;
        private readonly ControlPresenciaController _controller;
        private readonly TelefonoRepository _telefonosRepository;
        private readonly LocalizacionRepository _localizacionRepository;
        private readonly DbControlPresencia _contextPresencia;

        public BotonesHelper(DbWhatsapp context, DbTerceros contextTerceros, IMapper mapper, IConfiguration _configuracion, DbControlPresencia contextPresencia)
        {
            _localizacionRepository = new LocalizacionRepository(context);
            _mensajeController = new MensajesController(context, contextTerceros, mapper, _configuracion);
            _controller = new ControlPresenciaController(_configuracion, _mensajeController, context, contextTerceros, mapper, contextPresencia);
            _telefonosRepository = new TelefonoRepository(context, contextTerceros, mapper);
            _contextPresencia = contextPresencia;
        }

        /// <summary>
        /// Procesa la respuesta del usuario a través de un botón interactivo.
        /// </summary>
        /// <param name="mensaje">Mensaje recibido desde el webhook de WhatsApp.</param>
        public async Task ResponderMensaje(MessageWebhook mensaje)
        {
            int id = GetId(mensaje);
            var codUsuario = await GetCodFromNumber(mensaje.from);
            string result;

            switch (id)
            {
                case 1:
                    bool tieneUbicacion = await _localizacionRepository.UsuarioTieneLocalizacion(long.Parse(mensaje.from));

                    if (!tieneUbicacion && await _telefonosRepository.GetUbicacion(long.Parse(mensaje.from)))
                    {
                        await enviarMensajeLocalizacion(mensaje.from);
                        return;
                    }

                    result = await _controller.IniciarJornada(codUsuario);

                    if (result == "1")
                    {
                        await _mensajeController.EnviarMensaje(RespuestasHelpers.RespuestaErrorJornadaIniciada(mensaje.from));
                        return;
                    }

                    await ProcesarAccion(id, result, mensaje.from);
                    break;
                case 2:
                    await ProcesarAccion(id, await _controller.PausarJornada(codUsuario), mensaje.from);
                    break;
                case 3:
                    await ProcesarAccion(id, await _controller.ReaunudarJornada(codUsuario), mensaje.from);
                    break;
                case 4:
                    result = await _controller.FinalizarJornada(codUsuario);

                    if (result == "1")
                    {
                        await _mensajeController.EnviarMensaje(
                            RespuestasHelpers.MensajeErrorLocalizacion(mensaje.from));
                        return;
                    }

                    await ProcesarAccion(id, result, mensaje.from);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Responde a mensajes con formato ubicacion
        /// </summary>
        /// <param name="numero">Numero al que se va a enviar el mensaje</param>
        /// <returns>True si se ha completado correctamente, false en caso contrario</returns>
        public async Task ResponderMensajeUbicacion(string numero)
        {
            var telefono = await _telefonosRepository.GetTelefonosById(long.Parse(numero));

            string query = @"SELECT TOP 1 M.*
                            FROM MOVIMIENTOS AS M
                            JOIN DIAS AS D ON D.IDDIAS = M.IDDIAS
                            JOIN SEMANAS AS S ON S.IDSEMANAS = D.IDSEMANAS
                            JOIN Generales.dbo.datgen_0003 AS G ON G.ide_0003 = S.IDUSUARIOS
                            Where G.cod_0003 = @UserCode
                            ORDER BY M.HORA DESC";

            Movimientos? result;

            try
            {
                result = await _contextPresencia.Movimientos
                    .FromSqlRaw(query, new SqlParameter("@UserCode", telefono!.IdGenerales))
                    .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }

            if (result!.IdTiposMovimientos == 1)
                await _mensajeController.EnviarMensaje(RespuestasHelpers.MensajeUbicacionCompartida(numero));
            else
                await _mensajeController.EnviarMensaje(RespuestasHelpers.MensajeUbicacionCompartidaFinalizada(numero));
        }

        /// <summary>
        /// Procesa la acción correspondiente (iniciar, pausar, reanudar, finalizar) y envía la respuesta al usuario.
        /// </summary>
        /// <param name="idAccion">Identificador de la acción realizada.</param>
        /// <param name="error">Resultado o mensaje de error.</param>
        /// <param name="numero">Número de teléfono del usuario.</param>
        private async Task ProcesarAccion(int idAccion, string error, string numero)
        {
            if (isError(error))
                await _mensajeController.EnviarMensaje(RespuestasHelpers.RespuestaError(numero, error));
            else
                switch (idAccion) {
                    case 1:
                        await _mensajeController.EnviarMensaje(RespuestasHelpers.RespuestaIniciarJornada(numero));
                        break;
                    case 2:
                        await _mensajeController.EnviarMensaje(RespuestasHelpers.RespuestaPausarJornada(numero));
                        break;
                    case 3:
                        await _mensajeController.EnviarMensaje(RespuestasHelpers.RespuestaReaunudarJornada(numero));
                        break;
                    case 4:
                        await _mensajeController.EnviarMensaje(RespuestasHelpers.RespuestaFinalizarJornada(numero));
                        break;
                } 
        }

        /// <summary>
        /// Envía un mensaje solicitando la ubicación al usuario.
        /// </summary>
        /// <param name="telefono">Número de teléfono del usuario.</param>
        private async Task enviarMensajeLocalizacion(string numero)
        {
            await _mensajeController.EnviarMensaje(
                RespuestasHelpers.MensajeErrorLocalizacion(numero));
        }

        /// <summary>
        /// Extrae el ID del botón presionado a partir del mensaje recibido.
        /// </summary>
        /// <param name="mensaje">Mensaje del webhook.</param>
        /// <returns>ID del botón presionado como entero.</returns>
        private int GetId(MessageWebhook mensaje)
        {
            return BotonRepository.GetBotonId(mensaje.button.payload);
        }

        /// <summary>
        /// Determina si el resultado representa un error.
        /// </summary>
        /// <param name="result">Texto del resultado.</param>
        /// <returns><c>true</c> si el resultado es un error; de lo contrario, <c>false</c>.</returns>
        private bool isError(string result)
        {
            try
            {
                Console.WriteLine(result);
                int.Parse(result.Substring(0, 2));
                return false;
            }
            catch
            {
                return true;
            }
        }

        /// <summary>
        /// Obtiene el código de usuario (IdGenerales) a partir del número de teléfono.
        /// </summary>
        /// <param name="numero">Número de teléfono como string.</param>
        /// <returns>Código de usuario.</returns>
        private async Task<string> GetCodFromNumber(string numero)
        {
            long longNumber = long.Parse(numero);
            Telefono? telefono = await _telefonosRepository.GetTelefonosById(longNumber);
            return telefono!.IdGenerales;
        }
    }
}
