using Mango.Service.EmailAPI.Data;
using Mango.Service.EmailAPI.Models;
using Mango.Service.EmailAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Mango.Service.EmailAPI.Service;

public class EmailService : IEmailService
{
    private DbContextOptions<AppDbContext> _dbOptions;

    public EmailService(DbContextOptions<AppDbContext> dbOptions)
    {
        this._dbOptions = dbOptions;
    }

    public async Task EmailCartAndLog(CartDto cartDto)
    {
        StringBuilder message = new();

        message.AppendLine("<br/>Cart Email Requested ");
        message.AppendLine("<br/>Total " + cartDto.CartHeader.CartTotal);
        message.AppendLine("<br/>");
        message.AppendLine("<ul/>");

        foreach (var item in cartDto.CartDetails)
        {
            message.AppendLine("<li/>");
            message.AppendLine(item.Product.Name + " x " + item.Count);
            message.AppendLine("<li/>");
        }

        message.AppendLine("</ul>");

        await LogAndEmail(message.ToString(), cartDto.CartHeader.Email);
    }

    private async Task<bool> LogAndEmail(string message, string email)
    {
        try
        {
            EmailLogger emailLogger = new()
            {
                Email = email,
                EmailSent = DateTime.Now,
                Message = message
            };

            await using var _db = new AppDbContext(_dbOptions);
            await _db.EmailLoggers.AddAsync(emailLogger);
            await _db.SaveChangesAsync();   
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
