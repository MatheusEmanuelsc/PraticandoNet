namespace CashBank.Exception.ExceptionsBase;

public class ErrorOnValidationException : CashBankException
{
    public List<string>Errors { get; set; }
    public ErrorOnValidationException(List<string> errorMessages)
    {
        Errors = errorMessages;
    }
}