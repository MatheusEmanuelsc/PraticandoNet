using CashFlow.Application.UseCases.Expenses.Register;
using CashFlow.Communication.Enums;
using CashFlow.Exception;
using CommomTestUtilities.Requests;
using FluentAssertions;

namespace Validators.Tests.Expenses.Register;

public class RegisterExpenseValidatorTests
{
    [Fact]
    public void Success()
    {
        //Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseJsonBuilder.Build();
        //Act
        var result = validator.Validate(request);
        //Assert
        result.IsValid.Should().BeTrue();
    }
    
    [Fact]
    public void Error_Title_Empty()
    {
        //Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseJsonBuilder.Build();
        request.Title = string.Empty;
        //Act
        var result = validator.Validate(request);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().And
            .Contain(e=> e.ErrorMessage.Equals(ResourcesErrorMessages.TITLE_IS_REQUIRED));
    }
    
    [Fact]
    public void Error_Date_Future()
    {
        //Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseJsonBuilder.Build();
        request.Date = DateTime.UtcNow.AddDays(1);
        //Act
        var result = validator.Validate(request);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().And
            .Contain(e=> e.ErrorMessage.Equals(ResourcesErrorMessages.DATE_MUST_BE_GREATER_THAN_TODAY));
    }
    
    [Fact]
    public void Error_Payment_Type_Invalid()
    {
        //Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseJsonBuilder.Build();
        request.PaymentType = (PaymentType)700;
        //Act
        var result = validator.Validate(request);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().And
            .Contain(e=> e.ErrorMessage.Equals(ResourcesErrorMessages.PAYMENT_TYPE_IS_REQUIRED));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-2)]
    [InlineData(-5)]
    public void Error_Amount_Invalid(decimal amount)
    {
        //Arrange
        var validator = new RegisterExpenseValidator();
        var request = RequestRegisterExpenseJsonBuilder.Build();
        request.Amount = amount;
        //Act
        var result = validator.Validate(request);
        //Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().And
            .Contain(e=> e.ErrorMessage.Equals(ResourcesErrorMessages.AMOUNT_MUST_BE_GREATER_THAN_0));
    }
}