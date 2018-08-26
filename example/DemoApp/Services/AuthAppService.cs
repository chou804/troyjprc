using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC;

namespace DemoApp.Services
{
    public class AuthAppService
    {
        IAuthentication _authentication;
        public AuthAppService(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        [JRPCMethod]
        public async Task<AUTH.LoginView> Login(AUTH.Login input)
        {
            var tokenstring = _authentication.Login(input.Account, input.Password);
            //JRPCEnsure.ThrowIf(string.IsNullOrWhiteSpace(tokenstring), "帳號密碼錯誤");

            var v = new AUTH.LoginView
            {
                access_token = tokenstring
            };

            return await Task.FromResult(v);
        }

        [JRPCMethod]
        [JRPCAuthorized]
        public async Task<AUTH.LogoutView> Logout(AUTH.Logout input, RPCRequest req)
        {
            _authentication.Logout(req.AccessToken);

            Console.WriteLine(req.Account);

            var v = new AUTH.LogoutView();

            return await Task.FromResult(v);
        }
    }

    public class AUTH
    {
        public class Login
        {
            public string Account { get; set; }
            public string Password { get; set; }
        }
        public class LoginView
        {
            public string access_token { get; set; }
        }

        public class Logout
        {
        }
        public class LogoutView
        {
        }
    }
}
