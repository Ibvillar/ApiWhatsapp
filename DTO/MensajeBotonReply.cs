namespace ApiWhatsapp.DTO
{
    public class MensajeBotonReply: WhatsappMensajeBase
    {
        public InteractiveReply interactive { get; set; }
    }

    public class InteractiveReply
    {
        public string type { get; set; }
        public BodyReply body { get; set; }
        public ActionReply action { get; set; }
    }

    public class BodyReply
    {
        public string text { get; set; }
    }

    public class ActionReply
    {
        public ButtonReply[] buttons { get; set; }
    }

    public class ButtonReply
    {
        public string type { get; set; }
        public Reply reply { get; set; }
    }

    public class Reply
    {
        public string id { get; set; }
        public string title { get; set; }
    }
}
