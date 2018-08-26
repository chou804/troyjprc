using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TroyLab.JRPC.Core;

namespace TroyLab.AspNetCore.JRPC
{
    public class TokenManager : ITokenManager
    {
        readonly SymmetricSecurityKey secretKey;
        readonly SigningCredentials credentials;
        readonly JwtSecurityTokenHandler handler;
        readonly string issuer;

        private readonly ITokenKeyStorage _tokenKeyStorage;
        public TokenManager(ITokenKeyStorage tokenKeyStorage)
        {
            _tokenKeyStorage = tokenKeyStorage;

            secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenKeyStorage.GetSymmtricKey()));
            credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256Signature);
            handler = new JwtSecurityTokenHandler();
            issuer = _tokenKeyStorage.GetIssuer();
        }

        public string CreateToken(string username, TokenPayload payloads = null)
        {
            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username)
                }),
                IssuedAt = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddDays(365),
                SigningCredentials = credentials
            };

            var secToken = handler.CreateJwtSecurityToken(descriptor);

            if (payloads != null)
            {
                foreach (var kv in payloads)
                    secToken.Payload[kv.Key] = kv.Value;
            }

            var tokenString = handler.WriteToken(secToken);

            return tokenString;
        }

        public TokenPayload GetPayload(string token)
        {
            var payload = new TokenPayload();

            var principal = GetPrincipal(token);
            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
                foreach (var claim in identity.Claims)
                {
                    payload[claim.Type] = claim.Value;
                }
            }
            catch (NullReferenceException)
            {
                return null;
            };

            return payload;
        }

        public void RevokeToken(string token)
        {
            _tokenKeyStorage.RevokeToken(token);
        }

        public string ValidateToken(string token)
        {
            if (_tokenKeyStorage.IsTokenRevoked(token))
                return "";

            string username = null;
            var principal = GetPrincipal(token);
            if (principal == null)
                return null;
            ClaimsIdentity identity = null;
            try
            {
                identity = (ClaimsIdentity)principal.Identity;
            }
            catch (NullReferenceException)
            {
                return null;
            }
            var usernameClaim = identity.FindFirst(ClaimTypes.Name);
            username = usernameClaim.Value;
            return username;
        }

        ClaimsPrincipal GetPrincipal(string token)
        {
            try
            {
                var jwtToken = (JwtSecurityToken)handler.ReadToken(token);
                if (jwtToken == null)
                    return null;

                var parameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = secretKey
                };
                var principal = handler.ValidateToken(token, parameters, out SecurityToken securityToken);
                return principal;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
