using System;

namespace Play.Identity.Service.Exception;

public class UnknownUserException : System.Exception 
{
        public UnknownUserException(Guid UserId)
        : base($"Unknown user: {UserId}")
        {
            this.UserId = UserId; 
        }
        
        public Guid UserId { get; }
        
       
}