using System;

namespace Technolog.Models
{
    public class ExtruderProgram
    {
        public int program_id { get; set; }
        public string program_name { get; set; }
        public int product_id { get; set; }
        public string product_name { get; set; }
        public string temperature_zones { get; set; }
        public int screw_speed { get; set; }
        public decimal feeder_rate { get; set; }
        public DateTime created_at { get; set; }
    }

    public class ExtruderProgramModel
    {
        public string program_name { get; set; }
        public int product_id { get; set; }
        public string temperature_zones { get; set; }
        public int screw_speed { get; set; }
        public decimal feeder_rate { get; set; }
    }
}