using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    /// <summary>
    /// 處理使用者角色及權限相關的資料存取
    /// </summary>
    public interface IMembershipStore
    {
        #region Command
        object UpsertUser(AuthUser user);
        object UpsertRole(AuthRole role);
        #endregion

        #region Query
        AuthUser GetUseByAccount(string account);
        IEnumerable<AuthUser> GetUsersByRoleName(string roleName);
        IEnumerable<AuthUser> GetAllUsers();
        AuthRole GetRoleByName(string roleName);
        IEnumerable<AuthRole> GetAllRoles();
        IEnumerable<AuthRole> GetRolesByAccount(string account);
        /// <summary>
        /// 取得系統所有權限的Resousce及可允許執行的動作
        /// </summary>
        /// <returns></returns>
        IEnumerable<AuthResource> GetAllResources();
        #endregion
    }
}
