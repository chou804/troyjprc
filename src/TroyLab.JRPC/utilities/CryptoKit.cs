using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace TroyLab.JRPC
{
    public static class CryptoKit
    {
        #region 參考自 Microsoft.AspNet.Identity.Core 的 Crypto.cs

        private const int PBKDF2IterCount = 1000; // default for Rfc2898DeriveBytes
        private const int PBKDF2SubkeyLength = 256 / 8; // 256 bits
        private const int SaltSize = 128 / 8; // 128 bits

        /* =======================
         * HASHED PASSWORD FORMATS
         * =======================
         * 
         * Version 0:
         * PBKDF2 with HMAC-SHA1, 128-bit salt, 256-bit subkey, 1000 iterations.
         * (See also: SDL crypto guidelines v5.1, Part III)
         * Format: { 0x00, salt, subkey }
         */

        public static string MD5ThenHashPassword(string password)
        {
            return HashPassword(HashMD5(password));
        }

        public static string HashPassword(string password)
        {
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            // Produce a version 0 (see comment above) text hash.
            byte[] salt;
            byte[] subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, SaltSize, PBKDF2IterCount))
            {
                salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }

            var outputBytes = new byte[1 + SaltSize + PBKDF2SubkeyLength];
            Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize, PBKDF2SubkeyLength);
            return Convert.ToBase64String(outputBytes);
        }

        // hashedPassword must be of the format of HashWithPassword (salt + Hash(salt+input)
        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException(nameof(password));
            }

            password = password.ToUpper();

            var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            // Verify a version 0 (see comment above) text hash.

            if (hashedPasswordBytes.Length != (1 + SaltSize + PBKDF2SubkeyLength) || hashedPasswordBytes[0] != 0x00)
            {
                // Wrong length or version header.
                return false;
            }

            var salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SaltSize);
            var storedSubkey = new byte[PBKDF2SubkeyLength];
            Buffer.BlockCopy(hashedPasswordBytes, 1 + SaltSize, storedSubkey, 0, PBKDF2SubkeyLength);

            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, PBKDF2IterCount))
            {
                generatedSubkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }
            return ByteArraysEqual(storedSubkey, generatedSubkey);
        }

        // Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            var areSame = true;
            for (var i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        #endregion 參考自 Microsoft.AspNet.Identity.Core 的 Crypto.cs


        #region MD5

        /// <summary>
        /// 針對指定的字串，傳回使用 MD5 雜湊後再經過 16 進位編碼的字串
        /// </summary>
        /// <param name="phrase">要加密的字串</param>
        /// <returns></returns>
        public static string HashMD5(string phrase)
        {
            if (phrase == null)
                return null;

            var encoder = new UTF8Encoding();
            using (var hasher = new MD5CryptoServiceProvider())
            {
                var hashedBytes = hasher.ComputeHash(encoder.GetBytes(phrase));

                return ByteArrayToHexString(hashedBytes);
            }

            // 以上程式碼運算結果，等同如下程式碼
            //return System.Web.Helpers.Crypto.Hash(phrase, "md5");
        }

        #endregion MD5


        #region SHA

        /// <summary>
        /// 針對指定的字串，傳回使用 SHA1 雜湊後再經過 16 進位編碼的字串
        /// </summary>
        /// <param name="phrase">要加密的字串</param>
        /// <returns></returns>
        public static string HashSHA1(string phrase)
        {
            if (phrase == null)
                return null;

            var encoder = new UTF8Encoding();
            using (var hasher = new SHA1CryptoServiceProvider())
            {
                var hashedBytes = hasher.ComputeHash(encoder.GetBytes(phrase));

                return ByteArrayToHexString(hashedBytes);
            }

            // 以上程式碼運算結果，等同如下程式碼
            //return System.Web.Helpers.Crypto.Hash(phrase, "sha1");
        }


        /// <summary>
        /// 針對指定的字串，傳回使用 SHA256 雜湊後再經過 16 進位編碼的字串
        /// </summary>
        /// <param name="phrase">要加密的字串</param>
        /// <returns></returns>
        public static string HashSHA256(string phrase)
        {
            if (phrase == null)
                return null;

            var encoder = new UTF8Encoding();
            using (var hasher = new SHA256CryptoServiceProvider())
            {
                var hashedBytes = hasher.ComputeHash(encoder.GetBytes(phrase));

                return ByteArrayToHexString(hashedBytes);
            }

            // 以上程式碼運算結果，等同如下程式碼
            //return System.Web.Helpers.Crypto.Hash(phrase, "sha256");
        }


        /// <summary>
        /// 針對指定的字串，傳回使用 SHA512 雜湊後再經過 16 進位編碼的字串
        /// </summary>
        /// <param name="phrase">要加密的字串</param>
        /// <returns></returns>
        public static string HashSHA512(string phrase)
        {
            if (phrase == null)
                return null;

            var encoder = new UTF8Encoding();
            using (var hasher = new SHA512CryptoServiceProvider())
            {
                var hashedBytes = hasher.ComputeHash(encoder.GetBytes(phrase));

                return ByteArrayToHexString(hashedBytes);
            }

            // 以上程式碼運算結果，等同如下程式碼
            //return System.Web.Helpers.Crypto.Hash(phrase, "sha512");
        }

        #endregion SHA


        #region AES

        /// <summary>
        /// 針對指定的原始字串使用 AES 加密以傳回加密字串
        /// </summary>
        /// <param name="phrase">要加密的字串</param>
        /// <param name="key">金鑰字串(若為一般字串，則hashKey必須為true)</param>
        /// <param name="hashKey">金鑰字串是否要使用MD5雜湊</param>
        /// <returns></returns>
        public static string EncryptAES(string phrase, string key, bool hashKey = true)
        {
            if (phrase == null || key == null)
                return null;

            // 若金鑰是一般字串，則要利用 MD5 雜湊取得 16 進位字元組成的字串
            var hashedKey = hashKey ? HashMD5(key) : key;

            var keyArray = HexStringToByteArray(hashedKey);
            var toEncryptArray = Encoding.UTF8.GetBytes(phrase);
            byte[] resultArray;

            using (var aes = new AesCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {
                var cTransform = aes.CreateEncryptor();
                resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                aes.Clear();
            }

            return ByteArrayToHexString(resultArray);
        }


        /// <summary>
        /// 針對指定的加密字串使用 AES 解密以傳回原始字串
        /// </summary>
        /// <param name="hash">要解密的字串</param>
        /// <param name="key">金鑰字串(若為一般字串，則hashKey必須為true)</param>
        /// <param name="hashKey">金鑰字串是否要使用MD5雜湊</param>
        /// <returns></returns>
        public static string DecryptAES(string hash, string key, bool hashKey = true)
        {
            if (hash == null || key == null)
                return null;

            // 若金鑰是一般字串，則要利用 MD5 雜湊取得 16 進位字元組成的字串
            var hashedKey = hashKey ? HashMD5(key) : key;

            var keyArray = HexStringToByteArray(hashedKey);
            var toEncryptArray = HexStringToByteArray(hash);
            byte[] resultArray;

            using (var aes = new AesCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {
                var cTransform = aes.CreateDecryptor();
                resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                aes.Clear();
            }

            return Encoding.UTF8.GetString(resultArray);
        }

        #endregion AES


        #region 3DES

        /// <summary>
        /// 針對指定的原始字串使用 3DES 加密以傳回加密字串
        /// </summary>
        /// <param name="phrase">要加密的字串</param>
        /// <param name="key">金鑰字串(若為一般字串，則hashKey必須為true)</param>
        /// <param name="hashKey">金鑰字串是否要使用MD5雜湊</param>
        /// <returns></returns>
        public static string EncryptTripleDES(string phrase, string key, bool hashKey = true)
        {
            if (phrase == null || key == null)
                return null;

            // 若金鑰是一般字串，則要利用 MD5 雜湊取得 16 進位字元組成的字串
            var hashedKey = hashKey ? HashMD5(key) : key;

            var keyArray = HexStringToByteArray(hashedKey);
            var toEncryptArray = Encoding.UTF8.GetBytes(phrase);
            byte[] resultArray;

            using (var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {
                var cTransform = tdes.CreateEncryptor();
                resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();
            }

            return ByteArrayToHexString(resultArray);
        }


        /// <summary>
        /// 針對指定的加密字串使用 3DES 解密以傳回原始字串
        /// </summary>
        /// <param name="hash">要解密的字串</param>
        /// <param name="key">金鑰字串(若為一般字串，則hashKey必須為true)</param>
        /// <param name="hashKey">金鑰字串是否要使用MD5雜湊</param>
        /// <returns></returns>
        public static string DecryptTripleDES(string hash, string key, bool hashKey = true)
        {
            if (hash == null || key == null)
                return null;

            // 若金鑰是一般字串，則要利用 MD5 雜湊取得 16 進位字元組成的字串
            var hashedKey = hashKey ? HashMD5(key) : key;

            var keyArray = HexStringToByteArray(hashedKey);
            var toEncryptArray = HexStringToByteArray(hash);
            byte[] resultArray;

            using (var tdes = new TripleDESCryptoServiceProvider
            {
                Key = keyArray,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            })
            {
                var cTransform = tdes.CreateDecryptor();
                resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                tdes.Clear();
            }

            return Encoding.UTF8.GetString(resultArray);
        }

        #endregion 3DES


        #region Helpers

        /// <summary>
        /// 將陣列內的8位元不帶正負號的整數轉換成16進位字元組成的字串
        /// </summary>
        /// <param name="inputArray">8位元不帶正負號的整數陣列</param>
        /// <returns></returns>
        internal static string ByteArrayToHexString(byte[] inputArray)
        {
            if (inputArray == null)
                return null;

            var o = new StringBuilder("");

            for (var i = 0; i < inputArray.Length; i++)
                o.Append(inputArray[i].ToString("X2"));

            return o.ToString();
        }


        /// <summary>
        /// 將 16 進位字元組成的字串轉換為相等的8位元不帶正負號的整數陣列
        /// </summary>
        /// <param name="inputString">16 進位字元組成的字串</param>
        /// <returns></returns>
        internal static byte[] HexStringToByteArray(string inputString)
        {
            if (inputString == null)
                return null;

            if (inputString.Length == 0)
                return new byte[0];

            if (inputString.Length % 2 != 0)
                throw new Exception("Hex strings have an even number of characters and you have got an odd number of characters!");

            var num = inputString.Length / 2;
            var bytes = new byte[num];

            for (var i = 0; i < num; i++)
            {
                var x = inputString.Substring(i * 2, 2);
                try
                {
                    bytes[i] = Convert.ToByte(x, 16);
                }
                catch (Exception ex)
                {
                    throw new Exception("Part of your \"hex\" string contains a non-hex value.", ex);
                }
            }

            return bytes;
        }

        #endregion Helpers

        //public static string BoxingPassword(string hash, string type, int UpdatePasswordTimes = 0, DateTime? PasswordTokenIssuedOn = null)
        //{
        //    var resetPasswordTime = "";
        //    var today = DateTime.Now;

        //    switch (type)
        //    {
        //        case "create":
        //            resetPasswordTime = ConfigStore.Get("POS/resetPasswordTime/create");
        //            break;
        //        case "update":
        //            resetPasswordTime = ConfigStore.Get("POS/resetPasswordTime/update");
        //            break;
        //        case "findClassUpdate":
        //            resetPasswordTime = ConfigStore.Get("FindClass/resetPasswordTime/update");
        //            break;
        //    }

        //    double time = 10;
        //    if (double.TryParse(resetPasswordTime, out time))
        //    {
        //        time = double.Parse(resetPasswordTime);
        //    }

        //    if (UpdatePasswordTimes > 0 && PasswordTokenIssuedOn != null)
        //    {
        //        if (PasswordTokenIssuedOn > today.AddMinutes(-time))
        //        {
        //            return null;
        //        }
        //    }

        //    today = today.AddMinutes(time);

        //    return $"{hash},{today}";
        //}

        //public static string[] UnBoxingPassword(string hash)
        //{
        //    var result = hash.Split(',');

        //    var date = DateTime.Now;
        //    if (string.IsNullOrWhiteSpace(result[0]) || string.IsNullOrWhiteSpace(result[1]) || !DateTime.TryParse(result[1], out date) || DateTime.Parse(result[1]) < DateTime.Now)
        //    {
        //        result = null;
        //    }

        //    return result;
        //}
    }
}
