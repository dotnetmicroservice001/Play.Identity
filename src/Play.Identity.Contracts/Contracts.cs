namespace Play.Identity.Contracts;


    // common type message 
    public record DebitGil(Guid UserId, decimal Gil, Guid CorrelationId);
    
    // event type message 
    public record GilDebited(Guid CorrelationId);

    public record UserUpdated(
        Guid UserId,
        string Email, 
        decimal newTotalGil); 
    