// Controllers/productsController.cs
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using api.Entities;

namespace api.Controllers
{
    [RoutePrefix("api/products")]
    public class productsController : ApiController
    {
        private terEntities db = new terEntities();
        public productsController()
        {
            db.Configuration.LazyLoadingEnabled = false;
        }
        // GET: api/products - все продукты
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetProducts(string status = "active")
        {
            var products = db.products.Where(p => p.status == status).ToList();
            return Ok(products);
        }

        // GET: api/products/5 - продукт по ID
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetProduct(int id)
        {
            var product = db.products.Find(id);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // GET: api/products/code/XXX - продукт по коду
        [HttpGet]
        [Route("code/{code}")]
        public IHttpActionResult GetProductByCode(string code)
        {
            var product = db.products.FirstOrDefault(p => p.code == code);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        // GET: api/products/{id}/recipes - все рецептуры продукта
        [HttpGet]
        [Route("{id}/recipes")]
        public IHttpActionResult GetProductRecipes(int id)
        {
            var recipes = db.recipes
                .Include(r => r.recipe_components)
                .Where(r => r.product_id == id)
                .OrderByDescending(r => r.version)
                .ToList();
            return Ok(recipes);
        }

        // POST: api/products - создание продукта
        [HttpPost]
        [Route("")]
        public IHttpActionResult CreateProduct([FromBody] products product)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (db.products.Any(p => p.code == product.code))
                return BadRequest("Product code already exists");

            product.created_at = DateTime.Now;
            product.updated_at = DateTime.Now;
            product.status = "active";

            db.products.Add(product);
            db.SaveChanges();

            return Ok(new { message = "Product created", product_id = product.id });
        }

        // PUT: api/products/5 - обновление продукта
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult UpdateProduct(int id, [FromBody] products product)
        {
            var existing = db.products.Find(id);
            if (existing == null)
                return NotFound();

            existing.name = product.name;
            existing.product_type = product.product_type;
            existing.release_form = product.release_form;
            existing.unit_of_measure = product.unit_of_measure;
            existing.description = product.description;
            existing.updated_at = DateTime.Now;

            db.SaveChanges();

            return Ok(new { message = "Product updated" });
        }

        // DELETE: api/products/5 - мягкое удаление
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteProduct(int id)
        {
            var product = db.products.Find(id);
            if (product == null)
                return NotFound();

            product.status = "inactive";
            product.updated_at = DateTime.Now;
            db.SaveChanges();

            return Ok(new { message = "Product deactivated" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                db.Dispose();
            base.Dispose(disposing);
        }
    }
}