using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Play.Identity.Contracts;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Exception;


namespace Play.Identity.Service.Consumers;

public class DebitGilConsumer : IConsumer
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DebitGilConsumer(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Consume(ConsumeContext<DebitGil> context)
    {
        var message = context.Message;
        var user = await _userManager.FindByIdAsync(message.UserId.ToString());
        if (user == null)
        {
            throw new UnknownUserException(message.UserId);
        }
    }
}

