namespace ApiWhatsapp.Entities
{
    public class JsonMensajeBienvenida
    {
        public string messaging_product { get; set; } = "whatsapp";
        public string to { get; set; }
        public string type { get; set; } = "template";
        public Template template { get; set; }
    }

    public class Template
    {
        public string name { get; set; } = "mensaje_bienvenida";
        public Language language { get; set; }
        public List<Component> components { get; set; }
    }

    public class Language
    {
        public string code { get; set; }
    }

    public class Component
    {
        public string type { get; set; } = "body";
        public string? sub_type { get; set; }
        public string? index { get; set; }
        public List<Parameter> parameters { get; set; }
    }

    public class Parameter
    {
        public string type { get; set; } = "text";
        public string text { get; set; }
        public string? payload { get; set; }
    }
}
