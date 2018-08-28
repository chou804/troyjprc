using System;
using System.Collections.Generic;
using System.Text;

namespace TroyLab.JRPC
{
    public class Ensure
    {
        public static void ThrowIf(bool condition, string msg, Exception ex = null)
        {
            if (condition)
            {
                if (ex != null)
                    throw new JServiceException(msg, ex);
                else
                    throw new JServiceException(msg);
            }
        }

        /// <summary>
        /// 若檢查傳入的參數為空白，則拋 ArgumentException
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="argumentName"></param>
        public static void ArgumentNotEmpty(object arg, string argumentName)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            else if (arg is string argString && string.IsNullOrWhiteSpace(argString))
            {
                throw new ArgumentException($"{argumentName} cannot be empty");
            }
            else if (arg is System.Collections.ICollection elems && elems.Count == 0)
            {
                throw new ArgumentException($"{argumentName} cannot be empty");
            }
        }

        public static void NotEmpty(string str, string msg, Exception ex = null)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                if (ex != null)
                    throw new JServiceException(msg, ex);
                else
                    throw new JServiceException(msg);
            }
        }
    }
}
