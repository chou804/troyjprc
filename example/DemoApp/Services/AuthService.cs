using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TroyLab.JRPC;

namespace DemoApp.Services
{
    public class AuthService
    {
        IAuthentication _authentication;
        public AuthService(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        [JRPCMethod(nameof(Login))]
        public async Task<LoginView> Login(Login input)
        {
            var tokenstring = _authentication.Login(input.Account, input.Password);

            var v = new LoginView
            {
                access_token = tokenstring
            };

            return await Task.FromResult(v);
        }

        [JRPCMethod(nameof(Logout))]
        [JRPCAuthorized]
        public async Task Logout(RPCRequest req)
        {
            _authentication.Logout(req.AccessToken);

            Console.WriteLine(req.Account);
        }

        [JRPCMethod("GetProd")]
        [JRPCAuthorized("PRODUCT")]
        public async Task<string> GetProd(string prodId)
        {
            return $"{prodId} OK";
        }
    }

    public class Login
    {
        public string Account { get; set; }
        public string Password { get; set; }
    }
    public class LoginView
    {
        public string access_token { get; set; }
    }
}
