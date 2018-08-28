using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
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
}
