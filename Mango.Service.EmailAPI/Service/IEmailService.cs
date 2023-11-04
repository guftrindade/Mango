using Mango.Service.EmailAPI.Models.Dto;

namespace Mango.Service.EmailAPI.Service;

public interface IEmailService
{
    Task EmailCartAndLog(CartDto cartDto);
    Task RegisterUserEmailAndLog(string email);
}
