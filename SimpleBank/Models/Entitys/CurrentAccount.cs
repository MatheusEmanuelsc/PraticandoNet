namespace SimpleBank.Models.Entitys
{
    public class CurrentAccount : Account
    {
        public decimal YieldRate { get; set; }


        public decimal CalculeYield(decimal amount)
        {
            return amount;
        }
    }
}
