using System.ComponentModel;
using Dapper.Contrib.Extensions;

namespace VincYonetim.Data.DataModel
{
    [Table("cars")]
    public class Cars
    {
        [Key]
        public int Id { get; set; }
        public string PlateNumber { get; set; } = string.Empty;
    }

    [Table("accounts")]
    public class AccountModel
    {
        [Key]
        public int Id { get; set; }
        public string TaxOffice { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string IntegrationNumber { get; set; } = string.Empty;
    }

    [Table("agreement")]
    public class Agreements
    {
        [Key]
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string AgreementNumber { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;   // Sunucudaki dosya adı (yol değil)
        public int AccountId { get; set; }
        public int OperatorId { get; set; }
        public int CarId { get; set; }
        public int StatusCode { get; set; }
    }

    [Table("users")]
    public class Users
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;  // PBKDF2 hash; düz metin tutulmaz
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public DateTime LastLoginDate { get; set; }
        public bool IsActive { get; set; }
        public UserTypeCode UserType { get; set; }
    }

    public enum UserTypeCode
    {
        [Description("Hepsi")] All = -1,
        [Description("Administrator")] Administrator = 1,
        [Description("Operator")] Operator = 2,
        [Description("Muhasebe")] Accountant = 3
    }
}
