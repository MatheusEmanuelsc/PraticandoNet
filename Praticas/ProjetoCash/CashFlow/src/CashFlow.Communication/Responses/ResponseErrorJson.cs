namespace CashFlow.Communication.Responses;

public class ResponseErrorJson
{
    // public required string ErrorMessage { get; set; } = string.Empty;
    public  string ErrorMessage { get; set; } 

    public ResponseErrorJson(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }
}