using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC.Core
{
    /// <summary>
    /// for 前端設定權限UI用
    /// </summary>
    public class AuthActionView : AuthAction
    {
        /// <summary>
        /// 有沒有勾選
        /// </summary>
        public bool Checked { get; set; }
    }
}
