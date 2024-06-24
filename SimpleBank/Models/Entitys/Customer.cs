using SimpleBank.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SimpleBank.Models.Entitys
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; }=string.Empty;
        public TypePerson TypePerson { get; set; }
        public string Password { get; set; } = string.Empty;

        public Address? Address { get; set; }

    }
}
