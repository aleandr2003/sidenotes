using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace SideNotes.OAuth
{
    public abstract class TokenManagerFactory
    {
        protected abstract string concumerIdKey { get; }
        protected abstract string consumerSecretKey { get; }
        protected abstract string tokenManagerKey { get; }

        public InMemoryTokenManager GetTokenManager()
        {
            var tokenManager = (InMemoryTokenManager)HttpContext.Current.Application[tokenManagerKey];
            if (tokenManager == null)
            {
                string consumerKey = ConfigurationManager.AppSettings[concumerIdKey];
                string consumerSecret = ConfigurationManager.AppSettings[consumerSecretKey];
                if (!string.IsNullOrEmpty(consumerKey))
                {
                    tokenManager = new InMemoryTokenManager(consumerKey, consumerSecret);
                    HttpContext.Current.Application[tokenManagerKey] = tokenManager;
                }
            }

            return tokenManager;
        }
    }
}