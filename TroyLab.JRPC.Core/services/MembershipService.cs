using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TroyLab.JRPC.Core
{
    public class MembershipService : IMembership
    {
        private readonly IMembershipRepo _repo;

        public MembershipService(IMembershipRepo membershipRepo)
        {
            _repo = membershipRepo;
        }

        public void AddUserToRoles(string account, IEnumerable<string> roleNames)
        {
            Ensure.ArgumentNotEmpty(account, nameof(account));
            Ensure.ArgumentNotEmpty(roleNames, nameof(roleNames));

            var u = _repo.GetUseByAccount(account);

            var existsRoleNames = new HashSet<string>(u.RoleNames);

            u.RoleNames = existsRoleNames.Union(new HashSet<string>(roleNames)).ToList();
            SaveUser(u);
        }

        public IEnumerable<AuthResource> GetAllResources()
        {
            return _repo.GetAllResources();
        }

        public IEnumerable<AuthRole> GetAllRoles()
        {
            return _repo.GetAllRoles();
        }

        public IEnumerable<string> GetAllRoleNames()
        {
            var roles = _repo.GetAllRoles();
            return roles.Select(t => t.RoleName);
        }

        public IEnumerable<AuthRole> GetRolesByAccount(string account)
        {
            Ensure.ArgumentNotEmpty(account, nameof(account));
            return _repo.GetRolesByAccount(account);
        }

        public AuthUser GetUserByAccount(string account)
        {
            Ensure.ArgumentNotEmpty(account, nameof(account));

            return _repo.GetUseByAccount(account) ?? new NullAuthUser();
        }

        public IEnumerable<AuthUser> GetUsersByRoleName(string roleName)
        {
            Ensure.ArgumentNotEmpty(roleName, nameof(roleName));
            return _repo.GetUsersByRoleName(roleName);
        }

        public AuthRole GetRoleByName(string roleName)
        {
            Ensure.ArgumentNotEmpty(roleName, nameof(roleName));
            return _repo.GetAllRoles().FirstOrDefault(t => t.RoleName == roleName) ?? new NullAuthRole();
        }

        public IEnumerable<AuthResource> GetRolePermissionSetting(string roleName)
        {
            Ensure.ArgumentNotEmpty(roleName, nameof(roleName));

            // 取得一分系統完整的 resource
            var resources = GetAllResources().ToList();

            //　聯集 role 現有 resource　permissions
            var currentRolePermission = GetRoleByName(roleName).Permissoins;

            foreach (var r in resources)
            {
                if (currentRolePermission.TryGetValue(r.Id, out ICollection<string> actionValues))
                {
                    var actionViews = new List<AuthActionView>();

                    foreach (var a in r.Actions)
                    {
                        if (actionValues.Contains(a.Id))
                        { // role 有這個 aciton
                            actionViews.Add(new AuthActionView { Text = a.Text, Id = a.Id, Checked = true });
                        }
                        else
                        {
                            actionViews.Add(new AuthActionView { Text = a.Text, Id = a.Id, Checked = false });
                        }
                    }

                    r.Actions.Clear();
                    actionViews.ForEach(t => r.Actions.Add(t));
                }
            }

            return resources;
        }

        public bool IsGranted(AuthRole role, string resource)
        {
            Ensure.ArgumentNotEmpty(role, nameof(role));
            Ensure.ArgumentNotEmpty(resource, nameof(resource));

            return role.Permissoins.ContainsKey(resource) ? true : false;
        }

        public bool IsGranted(AuthRole role, string resource, string action)
        {
            Ensure.ArgumentNotEmpty(role, nameof(role));
            Ensure.ArgumentNotEmpty(resource, nameof(resource));
            Ensure.ArgumentNotEmpty(action, nameof(action));

            return role.Permissoins.TryGetValue(resource, out ICollection<string> actions) ? actions.Contains(action) : false;
        }

        public bool IsGranted(AuthUser user, string resource, string action)
        {
            Ensure.ArgumentNotEmpty(user, nameof(user));
            Ensure.ArgumentNotEmpty(resource, nameof(resource));
            Ensure.ArgumentNotEmpty(action, nameof(action));

            foreach (var roleName in user.RoleNames)
            {
                var role = _repo.GetRoleByName(roleName);
                if (IsGranted(role, resource, action))
                    return true;
            }
            return false;
        }

        public bool IsGranted(string account, string resource, string action)
        {
            Ensure.ArgumentNotEmpty(account, nameof(account));
            Ensure.ArgumentNotEmpty(resource, nameof(resource));
            Ensure.ArgumentNotEmpty(action, nameof(action));

            var user = _repo.GetUseByAccount(account);
            foreach (var roleName in user.RoleNames)
            {
                var role = _repo.GetRoleByName(roleName);
                if (IsGranted(role, resource, action))
                    return true;
            }
            return false;
        }

        public bool IsGranted(string account, string resource)
        {
            Ensure.ArgumentNotEmpty(account, nameof(account));
            Ensure.ArgumentNotEmpty(resource, nameof(resource));

            var user = _repo.GetUseByAccount(account);
            foreach (var roleName in user.RoleNames)
            {
                var role = _repo.GetRoleByName(roleName);
                if (IsGranted(role, resource))
                    return true;
            }
            return false;
        }

        public void SaveRole(AuthRole role)
        {
            Ensure.ArgumentNotEmpty(role, nameof(role));

            _repo.UpsertRole(role);
        }

        public void SaveUser(AuthUser user)
        {
            Ensure.ArgumentNotEmpty(user, nameof(user));

            _repo.UpsertUser(user);
        }
    }
}
