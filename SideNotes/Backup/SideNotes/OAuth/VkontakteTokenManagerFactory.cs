using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.OAuth
{
    public class VkontakteTokenManagerFactory : TokenManagerFactory
    {
        protected override string concumerIdKey { get { return "vkontakteAppID"; } }
        protected override string consumerSecretKey { get { return "vkontakteAppSecret"; } }
        protected override string tokenManagerKey { get { return "VkontakteTokenManager"; } }
    }
}