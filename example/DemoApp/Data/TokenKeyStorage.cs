using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC;

namespace DemoApp.Data
{
    public class TokenKeyStorage : ITokenKeyStorage
    {
        const string KEY = "xkBuE+d4kOksnps76pujP3ZhC7zHWG++SvuASQF39GCO/Fnu+qzOzXxL5Pv/cgLgId5gRoFtUZ1TxUIkO/N43Q==";
        const string ISSUER = "DemoApp";

        /// <summary>
        /// simple revoked token storage, not for production
        /// </summary>
        static ConcurrentDictionary<string, object> revokedDict = new ConcurrentDictionary<string, object>();

        public string GetIssuer()
        {
            return ISSUER;
        }

        public string GetSymmtricKey()
        {
            return KEY;
        }

        public void RevokeToken(string token)
        {
            revokedDict.TryAdd(token, null);
        }

        public bool IsTokenRevoked(string token)
        {
            if (revokedDict.ContainsKey(token))
                return true;

            return false;
        }
    }
}
