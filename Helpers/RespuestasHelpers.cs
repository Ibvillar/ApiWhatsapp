using ApiWhatsapp.Entities;
using Newtonsoft.Json;

namespace ApiWhatsapp.Helpers
{
    public class RespuestasHelpers
    {
        public static string CrearMensaje(
            string numero,
            string nombrePlantilla,
            List<Component> componentes)
        {
            var mensaje = new JsonMensajeBienvenida
            {
                to = numero,
                template = new Template
                {
                    name = nombrePlantilla,
                    language = new Language { code = "es" },
                    components = componentes
                }
            };

            return JsonConvert.SerializeObject(mensaje, Formatting.Indented);
        }

        public static string RespuestaIniciarJornada(string numero) =>
            CrearMensaje(numero, "succes_iniciar_jornada",
            [
                CrearBodyConHora(),
                CrearBoton("0", "pausar_jornada"),
                CrearBoton("1", "finalizar_jornada")
            ]);

        public static string RespuestaPausarJornada(string numero) =>
            CrearMensaje(numero, "succes_pausa_jornada",
            [
                CrearBodyConHora(),
                CrearBoton("0", "reaunudar_jornada")
            ]);

        public static string RespuestaReaunudarJornada(string numero) =>
            CrearMensaje(numero, "succes_reaunudar_jornada",
            [
                CrearBodyConHora(),
                CrearBoton("0", "pausar_jornada"),
                CrearBoton("1", "finalizar_jornada")
            ]);

        public static string RespuestaFinalizarJornada(string numero) =>
            CrearMensaje(numero, "succes_finalizar_jornada",
            [
                CrearBodyConHora(),
                CrearBoton("0", "iniciar_jornada")
            ]);

        public static string RespuestaError(string numero, string error) =>
            CrearMensaje(numero, "error_control_presencia",
            [
                new Component
                {
                    type = "body",
                    parameters =
                    [
                        new Parameter { type = "text", text = error },
                        new Parameter { type = "text", text = DateTime.Now.ToString("HH:mm") }
                    ]
                },

                CrearBoton("0", "iniciar_jornada"),
                CrearBoton("1", "pausar_jornada"),
                CrearBoton("2", "reaunudar_jornada"),
                CrearBoton("3", "finalizar_jornada")
            ]);

        public static string RespuestaErrorJornadaIniciada(string numero) =>
            CrearMensaje(numero, "error_jornada_empezada",
            [
                CrearBodyConHora(),
                CrearBoton("0", "iniciar_jornada")
            ]);

        public static string MensajeBienvenida(string numero, string nombre) =>
            CrearMensaje(numero, "mensaje_bienvenida",
            [
                new Component
                {
                    type = "body",
                    parameters =
                    [
                        new Parameter { type = "text", text = nombre }
                    ]
                },
                CrearBoton("0", "iniciar_jornada")
            ]);

        public static string MensajeErrorLocalizacion(string numero) =>
            CrearMensaje(numero, "soliciar_localizacion", []);

        public static string MensajeUbicacionCompartida(string numero) =>
            CrearMensaje(numero.ToString(), "ubicacion_compartida",
            [
                CrearBoton("0", "iniciar_jornada")
            ]);

        public static string MensajeUbicacionCompartidaFinalizada(string numero) =>
            CrearMensaje(numero.ToString(), "ubicacion_compartida_finalizada",
            [
                CrearBoton("0", "finalizar_jornada")
            ]);

        // Métodos auxiliares
        private static Component CrearBodyConHora() =>
            new Component
            {
                type = "body",
                parameters =
                [
                    new Parameter { type = "text", text = DateTime.Now.ToString("HH:mm") }
                ]
            };

        private static Component CrearBoton(string index, string payload) =>
            new Component
            {
                type = "button",
                sub_type = "quick_reply",
                index = index,
                parameters =
                [
                    new Parameter { type = "payload", payload = payload }
                ]
            };
    }
}
