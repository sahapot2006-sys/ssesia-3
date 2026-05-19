using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using api.Entities;

namespace api.Controllers
{
    [RoutePrefix("api/recipes")]
    public class recipesController : ApiController
    {
        private terEntities db = new terEntities();
        public recipesController()
        {
            db.Configuration.LazyLoadingEnabled = false;
        }
        // GET: api/recipes - все рецептуры
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetRecipes(string status = null)
        {
            var query = db.recipes.Include(r => r.recipe_components).AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(r => r.status == status);

            var recipes = query.OrderByDescending(r => r.created_at).ToList();
            return Ok(recipes);
        }

        // GET: api/recipes/5 - рецептура по ID
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetRecipe(int id)
        {
            var recipe = db.recipes
                .Include(r => r.recipe_components)
                .Include(r => r.products)
                .FirstOrDefault(r => r.id == id);

            if (recipe == null)
                return NotFound();  // Исправлено: убрали аргумент

            return Ok(recipe);
        }

        // POST: api/recipes - создание рецептуры
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateRecipe([FromBody] recipes recipe)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Проверяем существование продукта
            var product = db.products.Find(recipe.product_id);
            if (product == null)
                return BadRequest("Product not found");

            // Определяем следующую версию
            var maxVersion = db.recipes
                .Where(r => r.product_id == recipe.product_id)
                .Max(r => (int?)r.version) ?? 0;
            recipe.version = maxVersion + 1;

            recipe.created_at = DateTime.Now;
            recipe.updated_at = DateTime.Now;
            recipe.status = "draft";

            db.recipes.Add(recipe);
            db.SaveChanges();

            // Логируем статус
            LogStatusChange("recipes", recipe.id, null, "draft", "Recipe created");

            return Ok(new { message = "Recipe created", recipe_id = recipe.id, version = recipe.version });
        }

        // PUT: api/recipes/5/approve - утверждение рецептуры
        [HttpPut]
        [Route("{id}/approve")]
        public IHttpActionResult ApproveRecipe(int id, [FromBody] ApproveModel model)
        {
            var recipe = db.recipes.Find(id);
            if (recipe == null)
                return NotFound();  // Исправлено: убрали аргумент

            var oldStatus = recipe.status;
            recipe.status = "approved";
            recipe.approved_at = DateTime.Now;
            recipe.approved_by = model.approved_by;
            recipe.updated_at = DateTime.Now;

            db.SaveChanges();

            // Логируем изменение статуса
            LogStatusChange("recipes", id, oldStatus, "approved", model.comment);

            return Ok(new { message = "Recipe approved" });
        }

        // POST: api/recipes/{id}/components - добавление компонента
        [HttpPost]
        [Route("{id}/components")]
        public IHttpActionResult AddComponent(int id, [FromBody] recipe_components component)
        {
            var recipe = db.recipes.Find(id);
            if (recipe == null)
                return NotFound();  // Исправлено: убрали аргумент

            var rawMaterial = db.raw_materials.Find(component.raw_material_id);
            if (rawMaterial == null)
                return BadRequest("Raw material not found");

            component.recipe_id = id;
            component.created_at = DateTime.Now;

            db.recipe_components.Add(component);
            db.SaveChanges();

            // Пересчитываем total_percentage
            var totalPercentage = db.recipe_components
                .Where(c => c.recipe_id == id)
                .Sum(c => c.percentage);
            recipe.total_percentage = totalPercentage;
            db.SaveChanges();

            return Ok(new { message = "Component added", component_id = component.id });
        }

        // DELETE: api/recipes/5 - удаление рецептуры
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteRecipe(int id)
        {
            var recipe = db.recipes.Find(id);
            if (recipe == null)
                return NotFound();  // Исправлено: убрали аргумент

            recipe.status = "inactive";
            recipe.updated_at = DateTime.Now;
            db.SaveChanges();

            return Ok(new { message = "Recipe deactivated" });
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

    public class ApproveModel
    {
        public int approved_by { get; set; }
        public string comment { get; set; }
    }
}