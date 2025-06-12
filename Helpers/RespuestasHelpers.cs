using ApiWhatsapp.Controller;
using ApiWhatsapp.Entities;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApiWhatsapp.Helpers
{
    public class RespuestasHelpers
    {

        public static string RespuestaIniciarJornada(string numero)
        {
            var mensaje = JsonConvert.SerializeObject(new JsonMensajeBienvenida
            {
                to = numero,
                template = new Template
                {
                    name = "succes_inicio_jornada",
                    language = new Language { code = "es" },
                    components =
                    [
                        new Component
                           {
                               type = "body",
                               parameters =
                               [
                                   new Parameter { type = "text", text = DateTime.Now.ToString("HH:mm")}
                               ]
                           },
                           new Component
                           {
                               type = "button",
                               sub_type = "quick_reply",
                               index = "0",
                               parameters = new List<Parameter>
                               {
                                   new Parameter { type = "payload", payload = "pausar_jornada" }
                               }
                           },
                           new Component
                           {
                               type = "button",
                               sub_type = "quick_reply",
                               index = "1",
                               parameters = new List<Parameter>
                               {
                                   new Parameter { type = "payload", payload = "reaunudar_jornada" }
                               }
                           },
                           new Component
                           {
                               type = "button",
                               sub_type = "quick_reply",
                               index = "2",
                               parameters = new List<Parameter>
                               {
                                   new Parameter { type = "payload", payload = "finalizar_jornada" }
                               }
                           }
                       ]
                }
            }, Formatting.Indented);

            return mensaje;
        }

        public static string RespuestaPausarJornada(string numero)
        {
            var mensaje = JsonConvert.SerializeObject(new JsonMensajeBienvenida
            {
                to = numero,
                template = new Template
                {
                    name = "succes_pausa_jornada",
                    language = new Language { code = "es" },
                    components =
                    [
                        new Component
                           {
                               type = "body",
                               parameters =
                               [
                                   new Parameter { type = "text", text = DateTime.Now.ToString("HH:mm")}
                               ]
                           },
                           new Component
                           {
                               type = "button",
                               sub_type = "quick_reply",
                               index = "0",
                               parameters = new List<Parameter>
                               {
                                   new Parameter { type = "payload", payload = "reaunudar_jornada" }
                               }
                           },
                           new Component
                           {
                               type = "button",
                               sub_type = "quick_reply",
                               index = "1",
                               parameters = new List<Parameter>
                               {
                                   new Parameter { type = "payload", payload = "finalizar_jornada" }
                               }
                           }
                       ]
                }
            }, Formatting.Indented);

            return mensaje;
        }

        public static string RespuestaReaunudarJornada(string numero)
        {
            var mensaje = JsonConvert.SerializeObject(new JsonMensajeBienvenida
            {
                to = numero,
                template = new Template
                {
                    name = "succes_reaunudar_jornada",
                    language = new Language { code = "es" },
                    components =
                    [
                        new Component
                           {
                               type = "body",
                               parameters =
                               [
                                   new Parameter { type = "text", text = DateTime.Now.ToString("HH:mm")}
                               ]
                           },
                           new Component
                           {
                               type = "button",
                               sub_type = "quick_reply",
                               index = "0",
                               parameters = new List<Parameter>
                               {
                                   new Parameter { type = "payload", payload = "pausar_jornada" }
                               }
                           },
                           new Component
                           {
                               type = "button",
                               sub_type = "quick_reply",
                               index = "1",
                               parameters = new List<Parameter>
                               {
                                   new Parameter { type = "payload", payload = "finalizar_jornada" }
                               }
                           }
                       ]
                }
            }, Formatting.Indented);

            return mensaje;
        }

        public static string RespuestaFinalizarJornada(string numero)
        {
            var mensaje = JsonConvert.SerializeObject(new JsonMensajeBienvenida
            {
                to = numero,
                template = new Template
                {
                    name = "succes_finalizar_jornada",
                    language = new Language { code = "es" },
                    components =
                    [
                        new Component
                           {
                               type = "body",
                               parameters =
                               [
                                   new Parameter { type = "text", text = DateTime.Now.ToString("HH:mm")}
                               ]
                           },
                           new Component
                           {
                               type = "button",
                               sub_type = "quick_reply",
                               index = "0",
                               parameters = new List<Parameter>
                               {
                                   new Parameter { type = "payload", payload = "iniciar_jornada" }
                               }
                           }
                       ]
                }
            }, Formatting.Indented);

            return mensaje;
        }

        public static string RespuestaError(string numero, string error)
        {
            var mensaje = JsonConvert.SerializeObject(new JsonMensajeBienvenida
            {
                to = numero.ToString(),
                template = new Template
                {
                    name = "error_control_presencia",
                    language = new Language { code = "es" },
                    components =
                        [
                            new Component
                            {
                                type = "body",
                                parameters =
                                [
                                    new Parameter { type = "text", text = error},
                                    new Parameter { type = "text", text = DateTime.Now.ToString("HH:mm")}
                                ]
                            },
                            new Component
                            {
                                type = "button",
                                sub_type = "quick_reply",
                                index = "0",
                                parameters = new List<Parameter>
                                {
                                    new Parameter { type = "payload", payload = "iniciar_jornada" }
                                }
                            },
                            new Component
                            {
                                type = "button",
                                sub_type = "quick_reply",
                                index = "1",
                                parameters = new List<Parameter>
                                {
                                    new Parameter { type = "payload", payload = "pausar_jornada" }
                                }
                            },
                            new Component
                            {
                                type = "button",
                                sub_type = "quick_reply",
                                index = "2",
                                parameters = new List<Parameter>
                                {
                                    new Parameter { type = "payload", payload = "reaunudar_jornada" }
                                }
                            },
                            new Component
                            {
                                type = "button",
                                sub_type = "quick_reply",
                                index = "3",
                                parameters = new List<Parameter>
                                {
                                    new Parameter { type = "payload", payload = "finalizar_jornada" }
                                }
                            },
                        ]
                }
            },
                Formatting.Indented
            );

            return mensaje;
        }
    }
}
