using System.ComponentModel.DataAnnotations;

namespace SimpleBank.Models.Entitys
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }
        public string State { get; set; }=string.Empty;
        public string City { get; set; }= string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public int CustomerId { get; set; }
        public Customer Customer { get; set; }
    }
}
