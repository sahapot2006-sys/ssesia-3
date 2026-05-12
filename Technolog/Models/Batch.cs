using System;

namespace Technolog.Models
{
    public class Batch
    {
        public int batch_id { get; set; }
        public string batch_number { get; set; }
        public int order_id { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string status { get; set; }
        public decimal? actual_quantity { get; set; }
    }

    public class BatchModel
    {
        public string batch_number { get; set; }
        public int order_id { get; set; }
        public int user_id { get; set; }
    }
}