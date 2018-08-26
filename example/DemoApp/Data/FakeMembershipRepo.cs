using DemoApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC.Core;

namespace DemoApp.Data
{
    public class FakeMembershipRepo : IMembershipRepo
    {
        readonly List<FakeUser> _users = new List<FakeUser>();
        readonly List<FakeRole> _roles = new List<FakeRole>();

        public FakeMembershipRepo()
        {
            var adminRole = new FakeRole
            {
                RoleId = 1,
                RoleName = "Admin"
            };
            var adminUser = new FakeUser
            {
                UserId = 1,
                Account = "admin",
                IsAdmin = true,
                Password = CryptoKit.MD5ThenHashPassword("1234"),
                RoleNames = new string[] { adminRole.RoleName }
            };

            UpsertRole(adminRole);
            UpsertUser(adminUser);

            var mangerRole = new FakeRole
            {
                RoleId = 2,
                RoleName = "Manager",
                Permissoins = new AccessPermission
                {
                    { FakeResources.ProductMaintain, new string[] { AuthAction.Create.Id, AuthAction.Read.Id, AuthAction.Update.Id, AuthAction.Approve.Id } },
                    { FakeResources.OrderQuery, new string[]{ AuthAction.Read.Id } },
                }
            };

            var mangerUser = new FakeUser
            {
                UserId = 2,
                Account = "managerA",
                Password = CryptoKit.MD5ThenHashPassword("1234"),
                RoleNames = new string[] { mangerRole.RoleName }
            };

            UpsertRole(mangerRole);
            UpsertUser(mangerUser);


            var staffRole = new FakeRole
            {
                RoleId = 2,
                RoleName = "Staff",
                Permissoins = new AccessPermission
                {
                    { FakeResources.ProductMaintain, new string[]{ AuthAction.Create.Id, AuthAction.Read.Id, AuthAction.Update.Id } },
                }
            };

            var staffUser = new FakeUser
            {
                UserId = 3,
                Account = "staffA",
                Password = CryptoKit.MD5ThenHashPassword("1234"),
                RoleNames = new string[] { staffRole.RoleName }
            };

            UpsertRole(staffRole);
            UpsertUser(staffUser);
        }

        public IEnumerable<AuthResource> GetAllResources()
        {
            return new List<AuthResource>
            {
                new AuthResource
                {
                    Id = FakeResources.ProductMaintain,
                    Text = "商品資料維謢",
                    Actions = new List<AuthAction>
                    {
                        AuthAction.Create, AuthAction.Read, AuthAction.Update, AuthAction.Approve
                    }
                },
                new AuthResource
                {
                    Id = FakeResources.OrderQuery,
                    Text = "訂單查詢",
                    Actions = new List<AuthAction>
                    {
                        AuthAction.Read
                    }
                },
            };
        }

        public IEnumerable<AuthRole> GetAllRoles()
        {
            return _roles;
        }

        public IEnumerable<AuthUser> GetAllUsers()
        {
            return _users;
        }

        public AuthRole GetRoleByName(string roleName)
        {
            return _roles.FirstOrDefault(t => t.RoleName == roleName);
        }

        public IEnumerable<AuthRole> GetRolesByAccount(string account)
        {
            var roleNames = GetUseByAccount(account).RoleNames;
            return _roles.Where(t => roleNames.Contains(t.RoleName));
        }

        public AuthUser GetUseByAccount(string account)
        {
            return _users.FirstOrDefault(t => t.Account == account);
        }

        public IEnumerable<AuthUser> GetUsersByRoleName(string roleName)
        {
            return _users.Where(t => t.RoleNames.Contains(roleName));
        }

        public object UpsertRole(AuthRole role)
        {
            if (GetRoleByName(role.RoleName) is FakeRole r)
            {
                _roles.Remove(r);
                _roles.Add(r);
                return r.RoleName;
            }
            else
            {
                _roles.Add(role as FakeRole);
                return role.RoleName;
            }
        }

        public object UpsertUser(AuthUser user)
        {
            if (GetUseByAccount(user.Account) is FakeUser u)
            {
                _users.Remove(u);
                _users.Add(u);
                return u;
            }
            else
            {
                _users.Add(user as FakeUser);
                return user.Account;
            }
        }
    }
}
