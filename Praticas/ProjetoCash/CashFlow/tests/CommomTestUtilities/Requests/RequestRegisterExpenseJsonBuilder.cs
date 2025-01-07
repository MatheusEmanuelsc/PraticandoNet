using Bogus;
using CashFlow.Communication.Requests;

namespace CommomTestUtilities.Requests;

public class RequestRegisterExpenseJsonBuilder
{
    public static RequestRegisterExpensesJson Build()
    {
        var faker = new Faker<RequestRegisterExpensesJson>()
            .RuleFor(r => r.Amount, f => f.Finance.Amount()) // Gera valor aleatório para Amount
            .RuleFor(r => r.Date, f => f.Date.Past()) // Gera uma data no passado
            .RuleFor(r => r.Description, f => f.Lorem.Word()) // Gera uma palavra para Description
            .RuleFor(r => r.Title, f => f.Lorem.Word()) // Gera uma palavra para Title
            .RuleFor(r => r.PaymentType, f => f.PickRandom<CashFlow.Communication.Enums.PaymentType>()); // Seleciona um valor aleatório do enum

        return faker.Generate();
        
        
        // var faker = new Faker();

        // return new RequestRegisterExpensesJson()
        // {
        //     Amount = faker.Finance.Amount(), // Valor aleatório
        //     Date = faker.Date.Past(), // Data no passado
        //     Description = faker.Lorem.Word(), // Palavra aleatória
        //     Title = faker.Lorem.Word(), // Palavra aleatória
        //     PaymentType = faker.PickRandom<CashFlow.Communication.Enums.PaymentType>() // Valor aleatório do enum
        // };
    }
}