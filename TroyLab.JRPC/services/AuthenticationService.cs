using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public class AuthenticationService : IAuthentication
    {
        readonly IMembershipStore _membershipRepo;
        readonly ITokenManager _tokenManager;

        public AuthenticationService(IMembershipStore membershipRepo, ITokenManager tokenManager)
        {
            _membershipRepo = membershipRepo;
            _tokenManager = tokenManager;
        }

        public string Login(string account, string password)
        {
            var u = _membershipRepo.GetUseByAccount(account);
            if (u == null) return null;

            if (CryptoKit.VerifyHashedPassword(u.Password, password))
            {
                return _tokenManager.CreateToken(account);
            }
            else
            {
                return null;
            }
        }

        public void Logout(string token)
        {
            _tokenManager.RevokeToken(token);
        }
    }
}
