using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC.Core
{
    public interface ITokenKeyStorage
    {
        //#region Command
        //void SavePublicAndPriveKey(string audience, string keys);
        //#endregion

        #region Query
        string GetSymmtricKey();
        string GetIssuer();
        //string GetPublicKey(string audience);
        //string GetPrivateAndPublicKey(string audience);
        #endregion
    }
}
