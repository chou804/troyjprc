using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroyLab.JRPC.Core
{
    public class AuthUser
    {
        public virtual string Account { get; set; }
        public virtual string Password { get; set; }
        /// <summary>
        /// admin擁有所有權限
        /// </summary>
        public virtual bool IsAdmin { get; set; }
        public virtual ICollection<string> RoleNames { get; set; } = new List<string>();
    }

    public class NullAuthUser : AuthUser
    {
        public NullAuthUser()
        {
            Account = nameof(NullAuthUser);
            IsAdmin = false;
        }

    }
}
