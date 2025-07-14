using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Play.Identity.Contracts;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Exception;


namespace Play.Identity.Service.Consumers;

public class DebitGilConsumer : IConsumer<DebitGil>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<DebitGilConsumer> _logger;

    public DebitGilConsumer(UserManager<ApplicationUser> userManager, ILogger<DebitGilConsumer> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<DebitGil> context)
    {
        DebitGil message = context.Message;
        _logger.LogInformation("Debit Gil amount {Gil} for  user Id: {UserId} with correlation id {CorrelationId}",
            message.Gil, message.UserId, message.CorrelationId);
        
        var user = await _userManager.FindByIdAsync(message.UserId.ToString());
        if (user == null)
        {
            throw new UnknownUserException(message.UserId);
        }

        if (user.MessageIds.Contains(context.MessageId.Value))
        {
            await context.Publish(new GilDebited(message.CorrelationId));
            return;
        }
        
        // reduce the maount of gil 
        user.Gil -= message.Gil;
        
        if (user.Gil < 0)
        {
            _logger.LogError("Insufficient Funds for user {UserId} for amount gil {Gil} with correlation id {CorrelationId}",
                message.UserId, message.Gil, message.CorrelationId);
            throw new InsufficientFundException(message.UserId, message.Gil);
           
        }
        user.MessageIds.Add(context.MessageId.Value);
        
        await _userManager.UpdateAsync(user);
        var gilDebitedTask = context.Publish(new GilDebited(message.CorrelationId)); 
        var userUpdatedTask = context.Publish(new UserUpdated(
               user.Id, user.Email, user.Gil));
        
        await Task.WhenAll(gilDebitedTask, userUpdatedTask);
    }
}



