using System;

namespace baking1.errors
{
    public class SpringHeroTransactionException: Exception
    {
        public SpringHeroTransactionException(string message) : base(message)
        {
        }
    }
}