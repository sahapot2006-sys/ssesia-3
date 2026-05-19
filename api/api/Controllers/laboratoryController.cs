// Controllers/laboratoryController.cs
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using api.Entities;

namespace api.Controllers
{
    [RoutePrefix("api/laboratory")]
    public class laboratoryController : ApiController
    {
        private terEntities db = new terEntities();

        // GET: api/laboratory/tests - все испытания
        [HttpGet]
        [Route("tests")]
        public IHttpActionResult GetTests(int? batch_id = null, int? raw_material_batch_id = null)
        {
            var query = db.laboratory_tests
                .Include(t => t.test_parameters)
                .AsQueryable();

            if (batch_id.HasValue)
                query = query.Where(t => t.batch_id == batch_id);
            if (raw_material_batch_id.HasValue)
                query = query.Where(t => t.raw_material_batch_id == raw_material_batch_id);

            var tests = query.OrderByDescending(t => t.created_at).ToList();
            return Ok(tests);
        }

        // GET: api/laboratory/tests/5 - испытание по ID
        [HttpGet]
        [Route("tests/{id}")]
        public IHttpActionResult GetTest(int id)
        {
            var test = db.laboratory_tests
                .Include(t => t.test_parameters)
                .FirstOrDefault(t => t.id == id);

            if (test == null)
                return NotFound();  // Исправлено: без аргументов

            return Ok(test);
        }

        // POST: api/laboratory/tests - создание испытания
        [HttpPost]
        [Route("tests")]
        public IHttpActionResult CreateTest([FromBody] CreateTestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var testNumber = GenerateTestNumber();

            var test = new laboratory_tests
            {
                batch_id = model.batch_id,
                raw_material_batch_id = model.raw_material_batch_id,
                test_type = model.test_type,
                test_number = testNumber,
                analysis_date = DateTime.Now,
                analyst_id = model.analyst_id,
                status = "in_progress",
                priority = model.priority ?? "normal",
                created_at = DateTime.Now,
                updated_at = DateTime.Now
            };

            db.laboratory_tests.Add(test);
            db.SaveChanges();

            return Ok(new { message = "Test created", test_id = test.id, test_number = testNumber });
        }

        // POST: api/laboratory/tests/{id}/parameters - добавление параметров
        [HttpPost]
        [Route("tests/{id}/parameters")]
        public IHttpActionResult AddParameter(int id, [FromBody] TestParameterModel model)
        {
            var test = db.laboratory_tests.Find(id);
            if (test == null)
                return NotFound();

            var parameter = new test_parameters
            {
                test_id = id,
                parameter_name = model.parameter_name,
                measured_value = model.measured_value,
                standard_value_min = model.standard_value_min,
                standard_value_max = model.standard_value_max,
                unit = model.unit,
                analyst_comment = model.analyst_comment,
                created_at = DateTime.Now
            };

            // Определяем результат
            if (parameter.measured_value.HasValue && parameter.standard_value_min.HasValue && parameter.standard_value_max.HasValue)
            {
                parameter.result = (parameter.measured_value >= parameter.standard_value_min &&
                                    parameter.measured_value <= parameter.standard_value_max) ? "pass" : "fail";
            }

            db.test_parameters.Add(parameter);
            db.SaveChanges();

            return Ok(new { message = "Parameter added", result = parameter.result });
        }

        // PUT: api/laboratory/tests/{id}/complete - завершение испытания
        [HttpPut]
        [Route("tests/{id}/complete")]
        public IHttpActionResult CompleteTest(int id, [FromBody] CompleteTestModel model)
        {
            var test = db.laboratory_tests.Find(id);
            if (test == null)
                return NotFound();

            // Проверяем все параметры
            var parameters = db.test_parameters.Where(p => p.test_id == id).ToList();
            var failedParameters = parameters.Count(p => p.result == "fail");

            test.status = "completed";
            test.decision = failedParameters == 0 ? "approved" : "rejected";
            test.decision_comment = model.decision_comment;
            test.decision_date = DateTime.Now;
            test.decided_by = model.decided_by;
            test.updated_at = DateTime.Now;

            db.SaveChanges();

            // Если испытание для партии и оно провалено - создаем отклонение
            if (test.batch_id.HasValue && test.decision == "rejected")
            {
                CreateDeviation(test.batch_id.Value, test.id, test.test_type, failedParameters);
            }

            return Ok(new { message = "Test completed", decision = test.decision });
        }

        private string GenerateTestNumber()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var count = db.laboratory_tests.Count(t => t.test_number.Contains(date)) + 1;
            return $"LAB-{date}-{count:D4}";
        }

        private void CreateDeviation(int batchId, int testId, string testType, int failedCount)
        {
            var deviation = new deviations
            {
                batch_id = batchId,
                deviation_type = "Laboratory test failed",
                parameter_name = testType,
                description = $"Test {testType} failed with {failedCount} parameters out of specification",
                severity = "high",
                status = "open",
                detected_at = DateTime.Now
            };
            db.deviations.Add(deviation);
            db.SaveChanges();
        }
    }

    public class CreateTestModel
    {
        public int? batch_id { get; set; }
        public int? raw_material_batch_id { get; set; }
        public string test_type { get; set; }
        public int analyst_id { get; set; }
        public string priority { get; set; }
    }

    public class TestParameterModel
    {
        public string parameter_name { get; set; }
        public decimal? measured_value { get; set; }
        public decimal? standard_value_min { get; set; }
        public decimal? standard_value_max { get; set; }
        public string unit { get; set; }
        public string analyst_comment { get; set; }
    }

    public class CompleteTestModel
    {
        public int decided_by { get; set; }
        public string decision_comment { get; set; }
    }
}