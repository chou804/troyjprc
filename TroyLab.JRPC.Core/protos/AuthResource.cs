using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroyLab.JRPC.Core
{
    /// <summary>
    /// 系統權限資源
    /// </summary>
    public class AuthResource
    {
        /// <summary>
        /// 被保護的資源 e.g. SaleReport
        /// </summary>
        public virtual string Id { get; set; }

        /// <summary>
        /// 資源描述 e.g. 銷售報表
        /// </summary>
        public virtual string Text { get; set; }

        /// <summary>
        /// 該項資源所有可被執行的動作
        /// </summary>
        public virtual ICollection<AuthAction> Actions { get; set; }
    }
}
