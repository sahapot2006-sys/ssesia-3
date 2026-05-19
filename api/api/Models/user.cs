using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace api.Models
{
    public class user
    {
        public user(api.Models.user user)
        { }
             public int id { get; set; }
        public string username { get; set; }
        public string password_hash { get; set; }
        public string full_name { get; set; }
        public string role { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public Nullable<bool> is_active { get; set; }
        public Nullable<System.DateTime> last_login { get; set; }
        public Nullable<System.DateTime> created_at { get; set; }
        public string department { get; set; }
    
    
    }

}

   

    

