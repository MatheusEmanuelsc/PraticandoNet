using SimpleBank.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleBank.Models.Entitys
{
    public abstract class Account
    {
        [Key]
        public int AccountId { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public DateTime OpenDate { get; set; } = DateTime.Now;
        public decimal Balance { get; set; }

        public AccountType AccountType { get; set; }
        public AccountStatus AccountStatus { get; set; }


        
    }
}
