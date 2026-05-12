using System;

namespace Technolog.Models
{
    public class ProcessCard
    {
        public int process_card_id { get; set; }
        public int product_id { get; set; }
        public int recipe_id { get; set; }
        public int version { get; set; }
        public string status { get; set; }
        public DateTime? approved_at { get; set; }
    }

    public class ProcessCardModel
    {
        public int product_id { get; set; }
        public int recipe_id { get; set; }
        public int version { get; set; }
    }
}