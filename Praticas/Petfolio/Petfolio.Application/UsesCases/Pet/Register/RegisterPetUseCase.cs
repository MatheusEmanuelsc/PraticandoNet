using Petfolio.Communication.Requests;
using Petfolio.Communication.Responses;

namespace Petfolio.Application.UsesCases.Pet.Register;

public class RegisterPetUseCase
{
    public ResponseRegisterPetJson Execute(RequestPetJson pet)
    {
        return new ResponseRegisterPetJson
        {
            Id = 1,
            Name = pet.Name,
        };
    }
}