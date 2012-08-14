using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SideNotes.OAuth
{
    public class YandexTokenManagerFactory : TokenManagerFactory
    {
        protected override string concumerIdKey { get { return "yandexAppId"; } }
        protected override string consumerSecretKey { get { return "yandexAppPassword"; } }
        protected override string tokenManagerKey { get { return "YandexTokenManager"; } }
    }
}