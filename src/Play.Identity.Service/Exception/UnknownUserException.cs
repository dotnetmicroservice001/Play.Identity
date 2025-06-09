using System;

namespace Play.Identity.Service.Exception;

public class UnknownUserException : System.Exception 
{
        public UnknownUserException(Guid messageUserId)
        {
            throw new NotImplementedException();
        }
}