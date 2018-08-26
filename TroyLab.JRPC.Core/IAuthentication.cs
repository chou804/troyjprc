using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC.Core
{
    public interface IAuthentication
    {
        /// <summary>
        /// issue a jwt token
        /// </summary>
        /// <param name="account"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        string Login(string account, string password);
        /// <summary>
        /// invalid a jwt token
        /// </summary>
        /// <param name="token"></param>
        void Logout(string token);
    }

    public interface ITokenManager
    {
        string CreateToken(string username, TokenPayload payloads = null);

        string ValidateToken(string token);

        void RevokeToken(string token);

        TokenPayload GetPayload(string token);
    }
}
