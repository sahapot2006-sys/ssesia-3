using api.Entities;

namespace api.Controllers
{
    internal class ResponseUser
    {
        private users p;

        public ResponseUser(users p)
        {
            this.p = p;
        }
    }
}