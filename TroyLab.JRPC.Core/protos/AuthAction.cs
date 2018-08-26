using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroyLab.JRPC.Core
{
    /// <summary>
    /// 系統權限資源的可執行動作
    /// </summary>
    public class AuthAction
    {
        /// <summary>
        /// 可執行的動作, e.g. Read, Write, Delete
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// 描述 e.g. 讀取, 寫入, 刪除
        /// </summary>
        public virtual string Text { get; set; }


        public AuthAction() { }
        public AuthAction(string token, string text)
        {
            Id = token;
            Text = text;
        }


        // 預設常用 Actions
        public static readonly AuthAction Create = new AuthAction("create", "新增");
        public static readonly AuthAction Read = new AuthAction("read", "讀取");
        public static readonly AuthAction Update = new AuthAction("update", "更新");
        public static readonly AuthAction Delete = new AuthAction("delete", "刪除");
        public static readonly AuthAction Approve = new AuthAction("approve", "審核");
    }
}
