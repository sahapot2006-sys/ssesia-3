// Models/ResponseBatch.cs
using api.Entities;
using System;

namespace api.Models
{
    public class ResponseBatch
    {
        public int id { get; set; }
        public string batch_number { get; set; }
        public int order_id { get; set; }
        public string product_name { get; set; }
        public decimal planned_quantity_kg { get; set; }
        public decimal? actual_quantity_kg { get; set; }
        public string status { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string quality_decision { get; set; }
        public decimal yield_percentage { get; set; }

        public ResponseBatch(production_batches batch, string productName = null)
        {
            id = batch.id;
            batch_number = batch.batch_number;
            order_id = batch.order_id;
            product_name = productName ?? batch.product_id.ToString();
            planned_quantity_kg = batch.planned_quantity_kg;
            actual_quantity_kg = batch.actual_quantity_kg;
            status = batch.status;
            start_time = batch.start_time;
            end_time = batch.end_time;
            quality_decision = batch.quality_decision;
            yield_percentage = batch.planned_quantity_kg > 0 && batch.actual_quantity_kg.HasValue
                ? (batch.actual_quantity_kg.Value / batch.planned_quantity_kg) * 100
                : 0;
        }
    }
}