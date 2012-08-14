using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.OAuth
{
    public class FacebookTokenManagerFactory : TokenManagerFactory
    {
        protected override string concumerIdKey { get { return "facebookAppID"; } }
        protected override string consumerSecretKey { get { return "facebookAppSecret"; } }
        protected override string tokenManagerKey { get { return "FacebookTokenManager"; } }
    }
}