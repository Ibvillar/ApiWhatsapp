namespace ApiWhatsapp.Entities
{
    public class WebhookPayload
    {
        public List<Entry> entry { get; set; }
        public string @object { get; set; }
    }

    public class Entry
    {
        public string id { get; set; }
        public List<Change> changes { get; set; }
    }

    public class Change
    {
        public Value value { get; set; }
        public string field { get; set; }
    }

    public class Value
    {
        public Metadata metadata { get; set; }
        public List<MessageWebhook> messages { get; set; }
        public List<Contact> contacts { get; set; }
    }

    public class Contact
    {
        public Profile profile { get; set; }
        public string wa_id { get; set; }
    }

    public class Profile
    {
        public string name { get; set; }
    }

    public class Metadata
    {
        public string display_phone_number { get; set; }
        public string phone_number_id { get; set; }
    }

    public class MessageWebhook
    {
        public string from { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public MessageText text { get; set; }
        public MessageImage image { get; set; }
        public MessageDocument document { get; set; }
        public Interactive interactive { get; set; }
        public Location location { get; set; }
        public Button button { get; set; }

    }

    public class MessageText
    {
        public string body { get; set; }
    }

    public class MessageImage
    {
        public string id { get; set; }
        public string mime_type { get; set; }
    }

    public class MessageDocument
    {
        public string id { get; set; }
        public string filename { get; set; }
        public string mime_type { get; set; }
    }

    public class Interactive
    {
        public string type { get; set; }
        public ButtonReply button_reply { get; set; }
    }

    public class ButtonReply
    {
        public string id { get; set; }
        public string title { get; set; }
    }

    public class Button
    {
        public string payload { get; set; }
        public string text { get; set; }
    }


    public class Location
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string name { get; set; }
        public string address { get; set; }
    }
}
