using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC.Core
{
    public interface ITokenKeyStorage
    {
        #region Command
        //void SavePublicAndPriveKey(string audience, string keys);
        void RevokeToken(string token);
        #endregion

        #region Query
        string GetSymmtricKey();
        string GetIssuer();
        bool IsTokenRevoked(string token);
        //string GetPublicKey(string audience);
        //string GetPrivateAndPublicKey(string audience);
        #endregion
    }
}
