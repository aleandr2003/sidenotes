using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.OAuth
{
    public class TwitterTokenManagerFactory : TokenManagerFactory
    {
        protected override string concumerIdKey { get { return "twitterAppKey"; } }
        protected override string consumerSecretKey { get { return "twitterAppSecret"; } }
        protected override string tokenManagerKey { get { return "TwitterTokenManager"; } }
    }
}