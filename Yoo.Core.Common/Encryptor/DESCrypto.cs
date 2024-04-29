using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Yoo.Core.Common.Encryptor
{
    public class DESCrypto
    {
        public static string EncryptByDES(string input, string type, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            try
            {
                //将字符转换为UTF - 8编码的字节序列               
                byte[] rgbKey = Encoding.UTF8.GetBytes(key);
                byte[] rgbIV = Encoding.UTF8.GetBytes(iv);
                byte[] inputByteArray = Encoding.UTF8.GetBytes(input);
                //用指定的密钥和初始化向量创建CBC模式的DES加密标准
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                dCSP.Mode = type == "CBC" ? CipherMode.CBC : CipherMode.ECB;// CipherMode.CBC;
                dCSP.Padding = PaddingMode.PKCS7;
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);//写入内存流
                cStream.FlushFinalBlock();//将缓冲区中的数据写入内存流，并清除缓冲区
                return Convert.ToBase64String(mStream.ToArray()); //将内存流转写入字节数组并转换为string字符
            }
            catch
            {
                return input;
            }

        }

        public static string DecryptByDES(string input, string type, string key, string iv)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            try
            {
                //将字符转换为UTF - 8编码的字节序列              
                byte[] rgbKey = Encoding.UTF8.GetBytes(key);
                byte[] rgbIV = Encoding.UTF8.GetBytes(iv);
                byte[] inputByteArray = Convert.FromBase64String(input);
                //用指定的密钥和初始化向量使用CBC模式的DES解密标准解密
                DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
                dCSP.Mode = type == "CBC" ? CipherMode.CBC : CipherMode.ECB;// CipherMode.CBC;
                dCSP.Padding = PaddingMode.PKCS7;
                MemoryStream mStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(mStream, dCSP.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write);
                cStream.Write(inputByteArray, 0, inputByteArray.Length);
                cStream.FlushFinalBlock();
                return Encoding.UTF8.GetString(mStream.ToArray());
            }
            catch
            {
                return input;
            }
        }

        public static string encrypt(string input, string key)
        {
            try
            {
                byte[] s = Encoding.UTF8.GetBytes(input);

                return (desEncrypt(s, key));

            }
            catch (Exception e)
            {
                return input;
            }
        }

        private static string desEncrypt(byte[] value, string key)
        {
            try
            {
                byte[] keyArray;
                byte[] toEncryptArray = value;

                System.Configuration.AppSettingsReader settingsReader = new AppSettingsReader();

                keyArray = UTF8Encoding.UTF8.GetBytes(key);

                DESCryptoServiceProvider tdes = new DESCryptoServiceProvider();

                tdes.Key = keyArray;

                tdes.Mode = CipherMode.ECB;

                tdes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = tdes.CreateEncryptor();

                byte[] resultArray = cTransform.TransformFinalBlock
                        (toEncryptArray, 0, toEncryptArray.Length);

                tdes.Clear();

                return Convert.ToBase64String(resultArray, 0, resultArray.Length);

            }
            catch (Exception e)
            {

                return "";
            }
        }

		/// <summary>
		///     Encrypt
		/// </summary>
		/// <remarks>
		///     out keyword , which lets you pass an argument to a method by reference rather than by value.
		///     see => https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/out
		/// </remarks>>
		public static string Encrypt(string jsonString, string desKey, string desIv)
		{
			try
			{
				using var des = DES.Create();

				des.Mode = CipherMode.CBC;
				des.Padding = PaddingMode.PKCS7;
				des.Key = Encoding.UTF8.GetBytes(desKey);
				des.IV = Encoding.UTF8.GetBytes(desIv);

				var jsonByteArray = Encoding.UTF8.GetBytes(jsonString);

				byte[] byteArray;
				using (var encryptor = des.CreateEncryptor())
				{
					byteArray = encryptor.TransformFinalBlock(jsonByteArray, 0, jsonByteArray.Length);
				}

				string result = Convert.ToBase64String(byteArray);
				return result;
			}
			catch
			{
				return jsonString;
			}
		}

		/// <summary>
		///     Decrypt
		/// </summary>
		/// <remarks>
		///     out keyword , which lets you pass an argument to a method by reference rather than by value.
		///     see => https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/out
		/// </remarks>
		public static string Decrypt(string encryptString, string desKey, string desIv)
		{
			try
			{
				using var des = DES.Create();

				des.Mode = CipherMode.CBC;
				des.Padding = PaddingMode.PKCS7;
				des.Key = Encoding.UTF8.GetBytes(desKey);
				des.IV = Encoding.UTF8.GetBytes(desIv);

				var cipherByteArray = Convert.FromBase64String(encryptString);
				byte[] plainByteArray;

				using (var decrypt = des.CreateDecryptor())
				{
					plainByteArray = decrypt.TransformFinalBlock(cipherByteArray, 0, cipherByteArray.Length);
				}

				//  decryptResult will return
				string decryptResult = Encoding.UTF8.GetString(plainByteArray);

				return decryptResult;
			}
			catch (Exception ex)
			{
				return encryptString;
			}

		}


	}
}
