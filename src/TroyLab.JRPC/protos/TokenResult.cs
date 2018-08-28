using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public class TokenResult
    {
        public virtual string access_token { get; set; }
        public virtual string expire { get; set; }
    }
}
