using System;

namespace AncestryWeb.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public int Activated { get; set; }

        public string Captcha { get; set; }
    }


    public class AmazonUserModel
    {
        public string Email { get; set; }

    }
}