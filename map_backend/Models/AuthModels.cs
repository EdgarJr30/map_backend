namespace map_backend.Models
{
    public class AuthModels
    {

        public class RegisterModel
        {
            public string email { get; set; }
            public string passwordHash { get; set; }
        }

        public class LoginModel
        {
            public int id { get; set; }
            public string username { get; set; }
            public string email { get; set; }
            public string passwordHash { get; set; }
            public int roleId { get; set; }
        }

    }
}
