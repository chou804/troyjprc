using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC.Core;

namespace DemoApp.Data
{
    public class TokenKeyStorage : ITokenKeyStorage
    {
        const string KEY = "xkBuE+d4kOksnps76pujP3ZhC7zHWG++SvuASQF39GCO/Fnu+qzOzXxL5Pv/cgLgId5gRoFtUZ1TxUIkO/N43Q==";
        const string ISSUER = "DemoApp";

        public string GetIssuer()
        {
            return ISSUER;
        }

        public string GetSymmtricKey()
        {
            return KEY;
        }
    }
}
