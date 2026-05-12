using System;

namespace Technolog.Models
{
    public class Deviation
    {
        public int deviation_id { get; set; }
        public int batch_id { get; set; }
        public int? step_id { get; set; }
        public string type { get; set; }
        public string severity { get; set; }
        public string description { get; set; }
        public string planned_value { get; set; }
        public string actual_value { get; set; }
        public DateTime deviation_date { get; set; }
        public string resolution { get; set; }
    }

    public class EventLog
    {
        public int event_id { get; set; }
        public int user_id { get; set; }
        public string event_type { get; set; }
        public string entity { get; set; }
        public int entity_id { get; set; }
        public string description { get; set; }
        public DateTime event_date { get; set; }
    }
}