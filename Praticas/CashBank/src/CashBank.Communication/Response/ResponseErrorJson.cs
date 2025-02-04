namespace CashBank.Communication.Response;

public class ResponseErrorJson
{
    public  List<string> ErrorMessage { get; set; } 

    public ResponseErrorJson(string errorMessage)
    {
        ErrorMessage = new List<string>(){errorMessage};
    }

    public ResponseErrorJson(List<string> errorMessage)
    {
       ErrorMessage = errorMessage;
    }

}