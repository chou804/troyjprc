using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TroyLab.JRPC.Core
{
    public class RPCPacker
    {
        static JsonSerializerSettings ignoreNullHandler = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        /// <summary>
        /// Convert object to JSON string
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="ignoreNull">ignore null properties if true</param>
        /// <returns></returns>
        public static string Pack(object obj, bool ignoreNull = false)
        {
            if (ignoreNull)
            {
                return JsonConvert.SerializeObject(obj, Formatting.None, ignoreNullHandler);
            }
            else
            {
                return JsonConvert.SerializeObject(obj);
            }
        }

        /// <summary>
        /// 將物件轉為json字串，但是用單引號
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string SPack(object obj)
        {
            if (obj == null) return "";

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.QuoteChar = '\'';

                try
                {
                    JsonSerializer ser = new JsonSerializer();
                    ser.Serialize(writer, obj);
                }
                catch (Exception ex)
                {
                    sb.Append("SPack執行錯誤,");
                    sb.Append(ex.Message);
                }
            }
            //return JsonConvert.SerializeObject(obj);
            return sb.ToString();
        }


        public static T Unpack<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
