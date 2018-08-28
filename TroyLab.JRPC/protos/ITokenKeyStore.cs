using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public interface ITokenKeyStore
    {
        #region Command
        //void SavePublicAndPriveKey(string audience, string keys);
        void RevokeToken(string token);
        #endregion

        #region Query
        DateTime DefaultExpires { get; }
        string SymmtricKey { get; }
        string Issuer { get; }
        bool IsTokenRevoked(string token);
        //string GetPublicKey(string audience);
        //string GetPrivateAndPublicKey(string audience);
        #endregion
    }
}
