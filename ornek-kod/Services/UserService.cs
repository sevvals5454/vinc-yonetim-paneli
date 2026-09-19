using Dapper;
using Microsoft.AspNetCore.Identity;
using VincYonetim.Data;
using VincYonetim.Data.DataModel;

namespace VincYonetim.Services
{
    // Giriş doğrulama: parametreli sorgu + PBKDF2 şifre hash'i + pasif kullanıcı kontrolü.
    public class UserService : BaseService
    {
        private readonly PasswordHasher<Users> _hasher = new();

        public UserService(IDBConnector dbConnector) : base(dbConnector) { }

        public Users? Login(string username, string password)
        {
            var user = Connection.QueryFirstOrDefault<Users>(
                "SELECT * FROM users WHERE username = @username", new { username });

            if (user == null || !user.IsActive)
                return null;

            var result = _hasher.VerifyHashedPassword(user, user.Password, password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                user.Password = _hasher.HashPassword(user, password);
                Connection.Execute("UPDATE users SET password = @Password WHERE id = @Id", user);
            }
            return user;
        }

        public bool AddUser(Users user, string plainPassword)
        {
            user.Password = _hasher.HashPassword(user, plainPassword);
            return Connection.Execute(
                @"INSERT INTO users (username, password, firstname, lastname, isactive, usertype)
                  VALUES (@Username, @Password, @Firstname, @Lastname, @IsActive, @UserType)", user) > 0;
        }

        public List<Users> GetOperators() =>
            Connection.Query<Users>(
                "SELECT id, username, firstname, lastname FROM users WHERE usertype = @type AND isactive = 1",
                new { type = (int)UserTypeCode.Operator }).ToList();
    }
}
