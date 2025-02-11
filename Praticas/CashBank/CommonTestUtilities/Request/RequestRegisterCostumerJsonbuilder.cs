using Bogus;
using CashBank.Communication.Request;

namespace CommonTestUtilities.Request;

public static class RequestRegisterCostumerJsonbuilder
{
    public static RequestRegisterCostumerJson Build()
    {
        var faker = new Faker<RequestRegisterCostumerJson>()
            .RuleFor(r => r.Name, f => f.Name.FullName()) // Gera um nome aleatório
            .RuleFor(r => r.Email, f => f.Internet.Email()) // Gera um email aleatório
            .RuleFor(r => r.PhoneNumber, f => f.Phone.PhoneNumber()); // Gera um telefone aleatório

        return faker.Generate();
    }
}