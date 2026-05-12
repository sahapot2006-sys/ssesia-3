using System;

namespace Technolog.Models
{
    public class User
    {
        public int user_id { get; set; }
        public string login { get; set; }
        public string full_name { get; set; }
        public string role { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string department { get; set; }
        public bool is_active { get; set; }
        public DateTime? last_login { get; set; }
        public DateTime created_at { get; set; }
    }

    public class RegisterModel
    {
        public string login { get; set; }
        public string password { get; set; }
        public string full_name { get; set; }
        public string role { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string department { get; set; }
    }
}