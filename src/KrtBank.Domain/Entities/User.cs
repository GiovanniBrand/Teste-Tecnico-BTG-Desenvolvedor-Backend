using KrtBank.Domain.Common;

namespace KrtBank.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; private set; }
        public string Email { get; private set; }
        public string Password { get; private set; }

        // Construtor vazio para o Entity Framework
        protected User() { }

        public User(string name, string email, string password)
        {
            Name = name;
            Email = email;
            Password = password;
        }
    }
}
