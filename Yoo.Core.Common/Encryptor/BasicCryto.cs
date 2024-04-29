using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Yoo.Core.Common.Encryptor
{
    public class BasicCryto
    {
        public static string Base64Encode(string text)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(plainTextBytes);
		}

		public static string Hmacsha256Encrypt(string plaintext , string secretkey)
		{
			byte[] keyBytes = Encoding.UTF8.GetBytes(secretkey);
			byte[] messageBytes = Encoding.UTF8.GetBytes(plaintext);

			using (HMACSHA256 hmac = new HMACSHA256(keyBytes))
			{
				byte[] hashBytes = hmac.ComputeHash(messageBytes);
				string hash = BitConverter.ToString(hashBytes).Replace("-", "");
				return hash;
			}
		}
	}
}
