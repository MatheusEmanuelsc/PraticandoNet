using SimpleBank.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleBank.Models.Entitys
{
    public class Transaction
    {
        [Key]
        public int TransactionId { get; set; }
        public TypeTransaction TypeTransaction { get; set; }
        public DateTime DateTransaction { get; set; } = DateTime.Now;
        public string TransactionDescription { get; set; }= string.Empty;

        public string AccountNumber { get; set; }

    }
}
