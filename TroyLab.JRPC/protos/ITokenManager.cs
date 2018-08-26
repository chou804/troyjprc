using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public interface ITokenManager
    {
        string CreateToken(string username, TokenPayload payloads = null);

        string ValidateToken(string token);

        void RevokeToken(string token);

        TokenPayload GetPayload(string token);
    }
}
