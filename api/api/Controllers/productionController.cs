// Controllers/productionController.cs
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using api.Entities;
using api.Models;  // Добавлено для ResponseBatch

namespace api.Controllers
{
    [RoutePrefix("api/production")]
    public class productionController : ApiController
    {
        private terEntities db = new terEntities();

        // GET: api/production/orders - все заказы
        [HttpGet]
        [Route("orders")]
        public IHttpActionResult GetOrders(string status = null)
        {
            var query = db.production_orders
                .Include(o => o.products)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(o => o.status == status);

            var orders = query.OrderByDescending(o => o.created_at).ToList();
            return Ok(orders);
        }

        // POST: api/production/orders - создание заказа
        [HttpPost]
        [Route("orders")]
        public IHttpActionResult CreateOrder([FromBody] production_orders order)  // исправлено: production_orders
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Генерируем номер заказа
            order.order_number = GenerateOrderNumber();
            order.created_at = DateTime.Now;
            order.updated_at = DateTime.Now;
            order.status = "planned";

            db.production_orders.Add(order);
            db.SaveChanges();

            // Создаем событие
            CreateEvent("order_created", "production_order", order.id,
                $"Production order {order.order_number} created", "info");

            return Ok(new { message = "Order created", order_id = order.id, order_number = order.order_number });
        }

        // GET: api/production/batches - все партии
        [HttpGet]
        [Route("batches")]
        public IHttpActionResult GetBatches(string status = null)
        {
            var query = db.production_batches
                .Include(b => b.products)
                .Include(b => b.production_orders)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(b => b.status == status);

            var batches = query.OrderByDescending(b => b.created_at).ToList();
            var response = batches.Select(b => new ResponseBatch(b, b.products?.name)).ToList();

            return Ok(response);
        }

        // GET: api/production/batches/5 - партия по ID
        [HttpGet]
        [Route("batches/{id}")]
        public IHttpActionResult GetBatch(int id)
        {
            var batch = db.production_batches
                .Include(b => b.products)
                .Include(b => b.batch_step_execution)
                .Include(b => b.batch_raw_material_usage)
                .Include(b => b.laboratory_tests)
                .FirstOrDefault(b => b.id == id);

            if (batch == null)
                return NotFound();  // Исправлено: без аргументов

            return Ok(batch);
        }

        // POST: api/production/batches - создание партии из заказа
        [HttpPost]
        [Route("batches")]
        public IHttpActionResult CreateBatch([FromBody] CreateBatchModel model)
        {
            var order = db.production_orders.Find(model.order_id);
            if (order == null)
                return BadRequest("Order not found");

            // Генерируем номер партии
            var batchNumber = GenerateBatchNumber();

            var batch = new production_batches
            {
                batch_number = batchNumber,
                order_id = model.order_id,
                product_id = order.product_id,
                recipe_id = order.recipe_id,
                technology_map_id = order.technology_map_id,
                planned_quantity_kg = order.planned_quantity_kg,
                status = "planned",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            db.production_batches.Add(batch);
            db.SaveChanges();

            // Создаем шаги выполнения из технологической карты
            CreateBatchSteps(batch.id, batch.technology_map_id);

            CreateEvent("batch_created", "production_batch", batch.id,
                $"Production batch {batchNumber} created from order {order.order_number}", "info");

            return Ok(new { message = "Batch created", batch_id = batch.id, batch_number = batchNumber });
        }

        // PUT: api/production/batches/{id}/start - запуск партии
        [HttpPut]
        [Route("batches/{id}/start")]
        public IHttpActionResult StartBatch(int id, [FromBody] StartBatchModel model)
        {
            var batch = db.production_batches.Find(id);
            if (batch == null)
                return NotFound();

            var oldStatus = batch.status;
            batch.status = "in_progress";
            batch.start_time = DateTime.Now;
            batch.started_by = model.started_by;
            batch.updated_at = DateTime.Now;

            db.SaveChanges();

            // Логируем статус
            LogStatusChange("production_batch", id, oldStatus, "in_progress", "Batch started");

            CreateEvent("batch_started", "production_batch", id,
                $"Batch {batch.batch_number} started by user {model.started_by}", "info");

            return Ok(new { message = "Batch started" });
        }

        // PUT: api/production/batches/{id}/complete - завершение партии
        [HttpPut]
        [Route("batches/{id}/complete")]
        public IHttpActionResult CompleteBatch(int id, [FromBody] CompleteBatchModel model)
        {
            var batch = db.production_batches.Find(id);
            if (batch == null)
                return NotFound();

            var oldStatus = batch.status;
            batch.status = "completed";
            batch.end_time = DateTime.Now;
            batch.actual_quantity_kg = model.actual_quantity_kg;
            batch.completed_by = model.completed_by;
            batch.updated_at = DateTime.Now;

            db.SaveChanges();

            // Обновляем статус заказа
            UpdateOrderStatus(batch.order_id);

            // Проверяем отклонения
            CheckForDeviations(batch.id);

            LogStatusChange("production_batch", id, oldStatus, "completed", "Batch completed");

            CreateEvent("batch_completed", "production_batch", id,
                $"Batch {batch.batch_number} completed. Yield: {model.actual_quantity_kg / batch.planned_quantity_kg:P}", "success");

            return Ok(new { message = "Batch completed" });
        }

        // POST: api/production/batches/{id}/steps/{stepId}/complete - завершение шага
        [HttpPut]
        [Route("batches/{id}/steps/{stepId}/complete")]
        public IHttpActionResult CompleteStep(int id, int stepId, [FromBody] StepCompletionModel model)
        {
            var stepExecution = db.batch_step_execution
                .FirstOrDefault(s => s.batch_id == id && s.step_id == stepId);

            if (stepExecution == null)
                return NotFound();

            stepExecution.end_time = DateTime.Now;
            stepExecution.status = "completed";
            stepExecution.actual_temp_c = model.actual_temp_c;
            stepExecution.actual_pressure_bar = model.actual_pressure_bar;
            stepExecution.actual_duration_min = model.actual_duration_min;
            stepExecution.operator_comment = model.operator_comment;
            stepExecution.completed_by = model.completed_by;
            stepExecution.updated_at = DateTime.Now;

            // Проверяем отклонения
            var step = db.technology_steps.Find(stepId);
            if (step != null)
            {
                if (step.planned_temp_c.HasValue && model.actual_temp_c.HasValue)
                {
                    var tempDiff = Math.Abs(model.actual_temp_c.Value - step.planned_temp_c.Value);
                    var tolerance = step.tolerance_temp_c ?? 5;
                    if (tempDiff > tolerance)
                    {
                        stepExecution.deviation_flag = true;
                        CreateDeviation(batch_id: id, step_execution_id: stepExecution.id,
                            deviation_type: "Temperature deviation",
                            parameter_name: "Temperature",
                            planned_value: step.planned_temp_c.ToString(),
                            actual_value: model.actual_temp_c.ToString(),
                            severity: tempDiff > tolerance * 2 ? "high" : "warning");
                    }
                }
            }

            db.SaveChanges();

            return Ok(new { message = "Step completed", deviation = stepExecution.deviation_flag });
        }

        // POST: api/production/batches/{id}/materials - использование сырья
        [HttpPost]
        [Route("batches/{id}/materials")]
        public IHttpActionResult AddMaterialUsage(int id, [FromBody] MaterialUsageModel model)
        {
            var batch = db.production_batches.Find(id);
            if (batch == null)
                return NotFound();

            var rawMaterialBatch = db.raw_material_batches.Find(model.raw_material_batch_id);
            if (rawMaterialBatch == null)
                return BadRequest("Raw material batch not found");

            var usage = new batch_raw_material_usage
            {
                batch_id = id,
                raw_material_batch_id = model.raw_material_batch_id,
                used_quantity_kg = model.used_quantity_kg,
                used_at = DateTime.Now
            };

            db.batch_raw_material_usage.Add(usage);
            db.SaveChanges();

            // Обновляем остаток сырья
            rawMaterialBatch.quantity -= model.used_quantity_kg;
            db.SaveChanges();

            return Ok(new { message = "Material usage recorded" });
        }

        private string GenerateOrderNumber()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = db.production_orders.Count(o => o.order_number.Contains(date)) + 1;
            return $"PO-{date}-{count:D4}";
        }

        private string GenerateBatchNumber()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = db.production_batches.Count(b => b.batch_number.Contains(date)) + 1;
            return $"BATCH-{date}-{count:D4}";
        }

        private void CreateBatchSteps(int batchId, int technologyMapId)
        {
            var steps = db.technology_steps
                .Where(s => s.technology_map_id == technologyMapId)
                .OrderBy(s => s.step_order)
                .ToList();

            foreach (var step in steps)
            {
                var stepExecution = new batch_step_execution
                {
                    batch_id = batchId,
                    step_id = step.id,
                    step_order = step.step_order,
                    step_name = step.step_name,
                    status = "pending",
                    created_at = DateTime.Now,
                    updated_at = DateTime.Now
                };
                db.batch_step_execution.Add(stepExecution);
            }
            db.SaveChanges();
        }

        private void UpdateOrderStatus(int orderId)
        {
            var order = db.production_orders.Find(orderId);
            if (order == null) return;

            var allBatches = db.production_batches.Where(b => b.order_id == orderId).ToList();
            var completedBatches = allBatches.Where(b => b.status == "completed").Count();
            var totalBatches = allBatches.Count();

            if (completedBatches == totalBatches && totalBatches > 0)
            {
                order.status = "completed";
                order.actual_end_date = DateTime.Now;
            }
            else if (completedBatches > 0)
            {
                order.status = "partially_completed";
            }

            db.SaveChanges();
        }

        private void CheckForDeviations(int batchId)
        {
            var deviations = db.deviations.Count(d => d.batch_id == batchId && d.status == "open");
            if (deviations > 0)
            {
                CreateEvent("deviations_detected", "production_batch", batchId,
                    $"{deviations} open deviation(s) detected for batch", "warning");
            }
        }

        private void CreateDeviation(int? batch_id, int? step_execution_id, string deviation_type,
            string parameter_name, string planned_value, string actual_value, string severity)
        {
            var deviation = new deviations
            {
                batch_id = batch_id,
                step_execution_id = step_execution_id,
                deviation_type = deviation_type,
                parameter_name = parameter_name,
                planned_value = planned_value,
                actual_value = actual_value,
                severity = severity,
                status = "open",
                detected_at = DateTime.Now
            };
            db.deviations.Add(deviation);
            db.SaveChanges();
        }

        private void CreateEvent(string eventType, string sourceType, int sourceId, string message, string severity)
        {
            var evt = new events
            {
                event_type = eventType,
                source_type = sourceType,
                source_id = sourceId,
                message = message,
                severity = severity,
                created_at = DateTime.Now,
                is_read = false
            };
            db.events.Add(evt);
            db.SaveChanges();
        }

        private void LogStatusChange(string entityType, int entityId, string oldStatus, string newStatus, string comment)
        {
            var history = new status_history
            {
                entity_type = entityType,
                entity_id = entityId,
                old_status = oldStatus,
                new_status = newStatus,
                changed_at = DateTime.Now,
                comment = comment
            };
            db.status_history.Add(history);
            db.SaveChanges();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }

    public class CreateBatchModel
    {
        public int order_id { get; set; }
    }

    public class StartBatchModel
    {
        public int started_by { get; set; }
    }

    public class CompleteBatchModel
    {
        public decimal actual_quantity_kg { get; set; }
        public int completed_by { get; set; }
    }

    public class StepCompletionModel
    {
        public decimal? actual_temp_c { get; set; }
        public decimal? actual_pressure_bar { get; set; }
        public int? actual_duration_min { get; set; }
        public string operator_comment { get; set; }
        public int completed_by { get; set; }
    }

    public class MaterialUsageModel
    {
        public int raw_material_batch_id { get; set; }
        public decimal used_quantity_kg { get; set; }
    }
}