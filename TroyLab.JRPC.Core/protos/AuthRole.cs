using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroyLab.JRPC.Core
{
    public class AuthRole
    {
        /// <summary>
        /// role name, unique
        /// </summary>
        public virtual string RoleName { get; set; }
        /// <summary>
        /// role's permissions
        /// </summary>
        public virtual AccessPermission Permissoins { get; set; } = new AccessPermission();
    }

    public class NullAuthRole : AuthRole
    {
        public NullAuthRole()
        {
            this.RoleName = nameof(NullAuthRole);
        }
    }
}
