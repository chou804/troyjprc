using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public interface IMembership
    {
        /// <summary>
        /// 取得使用者
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        AuthUser GetUserByAccount(string account);
        /// <summary>
        /// 取得該user的角色
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<AuthRole> GetRolesByAccount(string account);
        /// <summary>
        /// 取得所有的角色資料
        /// </summary>
        /// <returns></returns>
        IEnumerable<AuthRole> GetAllRoles();
        /// <summary>
        /// 取得某個角色的所有 user
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        IEnumerable<AuthUser> GetUsersByRoleName(string roleName);
        /// <summary>
        /// 將user加到一個或多個roles
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleIds"></param>
        void AddUserToRoles(string account, IEnumerable<string> roleNames);
        /// <summary>
        /// 儲存角色資訊
        /// </summary>
        /// <param name="role"></param>
        void SaveRole(AuthRole role);
        /// <summary>
        /// 儲存使用者
        /// </summary>
        /// <param name="user"></param>
        void SaveUser(AuthUser user);
        /// <summary>
        /// 某個角色是否有執行 action 的權限
        /// </summary>
        /// <param name="role"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        bool IsGranted(AuthRole role, string resource, string action);
        /// <summary>
        /// 某個角色是否有執行 resource 的所有權限
        /// </summary>
        /// <param name="role"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        bool IsGranted(AuthRole role, string resource);
        /// <summary>
        /// 某個User是否有執行 action 的權限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="resource"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        bool IsGranted(AuthUser user, string resource, string action);
        /// <summary>
        /// 某個account是否有執行 action 的權限
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="resource"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        bool IsGranted(string account, string resource, string action);
        /// <summary>
        /// 某個account是否有執行 resource 的其中一個權限
        /// </summary>
        /// <param name="account"></param>
        /// <param name="resource"></param>
        /// <returns></returns>
        bool IsGranted(string account, string resource);
        /// <summary>
        /// 取得系統的所有Resources
        /// </summary>
        /// <returns></returns>
        IEnumerable<AuthResource> GetAllResources();
    }
}
