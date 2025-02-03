namespace CashBank.Communication.Response;

public class ResponseErrorJson
{
    public  string ErrorMessage { get; set; } 

    public ResponseErrorJson(string errorMessage)
    {
        ErrorMessage = errorMessage;
    }

}