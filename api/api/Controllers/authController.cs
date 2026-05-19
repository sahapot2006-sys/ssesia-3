// Controllers/authController.cs
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using api.Entities;

namespace api.Controllers
{
    [RoutePrefix("api/auth")]
    public class authController : ApiController
    {
        private terEntities db = new terEntities();

        [HttpPost]
        [Route("register")]
        public IHttpActionResult Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (db.users.Any(u => u.username == model.username))
                return BadRequest("Username already exists");

            if (db.users.Any(u => u.email == model.email))
                return BadRequest("Email already exists");

            var user = new users
            {
                username = model.username,
                password_hash = HashPassword(model.password),
                full_name = model.full_name,
                email = model.email,
                role = model.role ?? "Operator",
                department = model.department,
                is_active = true,
                created_at = DateTime.Now
            };

            db.users.Add(user);
            db.SaveChanges();

            // Логируем действие
            LogUserAction(user.id, "register", "users", user.id, $"User {user.username} registered");

            return Ok(new { message = "Registration successful", user_id = user.id });
        }

        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login([FromBody] LoginModel model)
        {
            var user = db.users.FirstOrDefault(u => u.username == model.username && u.is_active == true);

            if (user == null || !VerifyPassword(model.password, user.password_hash))
                return Unauthorized();

            // Обновляем время последнего входа
            user.last_login = DateTime.Now;
            db.SaveChanges();

            // Генерируем простой токен (в реальном проекте используйте JWT)
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.id}:{user.username}:{DateTime.Now.AddDays(7)}"));

            // Логируем действие
            LogUserAction(user.id, "login", "users", user.id, $"User {user.username} logged in");

            return Ok(new { token, user.id, user.username, user.full_name, user.role });
        }

        [HttpPost]
        [Route("logout")]
        public IHttpActionResult Logout([FromBody] LogoutModel model)
        {
            // Логируем выход
            LogUserAction(model.user_id, "logout", "users", model.user_id, "User logged out");
            return Ok(new { message = "Logged out successfully" });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }

        private void LogUserAction(int userId, string actionType, string entityType, int? entityId, string details)
        {
            var userAction = new user_actions
            {
                user_id = userId,
                action_type = actionType,
                entity_type = entityType,
                entity_id = entityId,
                details = details,
                created_at = DateTime.Now
            };
            db.user_actions.Add(userAction);
            db.SaveChanges();
        }
    }

    public class RegisterModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public string full_name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
        public string department { get; set; }
    }

    public class LoginModel
    {
        public string username { get; set; }
        public string password { get; set; }
    }

    public class LogoutModel
    {
        public int user_id { get; set; }
    }
}