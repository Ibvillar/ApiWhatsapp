using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiWhatsapp.Entitties
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
        public List<MessageWeebhook> messages { get; set; }
    }

    public class Metadata
    {
        public string display_phone_number { get; set; }
        public string phone_number_id { get; set; }
    }

    public class MessageWeebhook
    {
        public string from { get; set; }
        public string id { get; set; }
        public MessageText text { get; set; }
        public string type { get; set; }
    }

    public class MessageText
    {
        public string body { get; set; }
    }
}
