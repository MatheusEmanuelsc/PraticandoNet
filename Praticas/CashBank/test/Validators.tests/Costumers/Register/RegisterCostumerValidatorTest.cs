using CashBank.Application.UsesCases.Costumers.Register;
using CashBank.Communication.Request;
using CashBank.Exception;
using CommonTestUtilities.Request;
using FluentAssertions;

namespace Validators.tests.Expenses.Register;

public class RegisterCostumerValidatorTest
{
    [Fact]
    public void SucessTest()
    {
        //Arrange
        var validator = new RegisterCostumerValidator();
        var request = RequestRegisterCostumerJsonbuilder.Build();
        
        //Act
        var result = validator.Validate(request);
        //Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Error_Name_Empty()
    {
        //Arrange
        var validator = new RegisterCostumerValidator();
        var request = RequestRegisterCostumerJsonbuilder.Build();
        request.Name = "";
        
        //Act
        var result = validator.Validate(request);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().And.Contain(a=>a.ErrorMessage.Equals(ResourceErrorMessages.NAME_REQUIRED));
    }
}