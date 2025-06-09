using System;

namespace Play.Identity.Service.Exception;

public class InsufficientFundException : System.Exception
{
    public InsufficientFundException(Guid UserId, decimal Gil) : 
        base($"Insufficient Gil to Debit {Gil} for {UserId}")
    {
       this.UserId = UserId;
       this.GilToDebit = Gil; 
    }
    
    public Guid UserId { get; }
    public decimal GilToDebit { get;  }
}