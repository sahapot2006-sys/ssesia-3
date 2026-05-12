using System;

namespace Technolog.Models
{
    public class ProductionOrder
    {
        public int order_id { get; set; }
        public string order_number { get; set; }
        public int recipe_id { get; set; }
        public decimal planned_quantity { get; set; }
        public string status { get; set; }
        public DateTime? planned_start_date { get; set; }
    }

    public class OrderModel
    {
        public string order_number { get; set; }
        public int recipe_id { get; set; }
        public decimal planned_quantity { get; set; }
        public DateTime planned_start_date { get; set; }
    }
}