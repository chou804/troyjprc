using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TroyLab.JRPC
{
    /// <summary>
    /// 記錄某個角色可使用的權限,  ResourceId,  action strings
    /// </summary>
    public class AccessPermission : Dictionary<string, ICollection<string>>
    {
        public AccessPermission() { }
        public AccessPermission(AccessPermission keyValuePairs) : base(keyValuePairs) { }

        /// <summary>
        /// 空的 ICollection&lt;string&gt;
        /// </summary>
        public static ICollection<string> EmtpyActions { get { return new string[0]; } }
    }
}
