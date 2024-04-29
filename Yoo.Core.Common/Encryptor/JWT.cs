using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Globalization;
using Jose;
using Newtonsoft.Json;

namespace Yoo.Core.Common.Encryptor
{
    public class JWT
    {

        public static string calcHmac(Dictionary<string, object> header, Dictionary<string, object> payload, string apikey)
        {
            byte[] key = Encoding.ASCII.GetBytes(apikey);
            return Jose.JWT.Encode(payload, key, JwsAlgorithm.HS256, extraHeaders: header);
        }

    }
}
