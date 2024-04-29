
using System;
using System.Security;
using System.Security.Cryptography;
using System.IO;
using System.Text;


namespace Yoo.Core.Common.Encryptor
{
    public class Des3Crypto
    {
        private byte[] bKey;
        private byte[] bIV;
        System.Security.Cryptography.CipherMode mode;
        private SymmetricAlgorithm mCSP = new TripleDESCryptoServiceProvider();
        public Des3Crypto(byte[] key)
        {
            bKey = key;
        }
        public Des3Crypto(byte[] key, string type, bool is_CBC, byte[] iv = null)
        {
            bKey = key;
            bIV = iv;

            if (type == "AES")
            {
                mCSP = new AesCryptoServiceProvider();
            }
            else if (type == "DES3")
            {
                mCSP = new TripleDESCryptoServiceProvider();
            }

            if (is_CBC == true)
            {
                mode = System.Security.Cryptography.CipherMode.CBC;
            }
            else
            {
                mode = System.Security.Cryptography.CipherMode.ECB;
            }
        }

        public string EncryptString(string Value)
        {
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;
            mCSP.Key = bKey;
            if (bIV != null)
            {
                mCSP.IV = bIV;
            }
            mCSP.Mode = mode;
            mCSP.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            ct = mCSP.CreateEncryptor(mCSP.Key, mCSP.IV);
            byt = Encoding.UTF8.GetBytes(Value);
            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();
            cs.Close();
            return Convert.ToBase64String(ms.ToArray());
        }

        public string DecryptString(string Value)
        {
            ICryptoTransform ct;
            MemoryStream ms;
            CryptoStream cs;
            byte[] byt;
            mCSP.Key = bKey;
            mCSP.IV = bIV;
            mCSP.Mode = mode;
            mCSP.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            mCSP.FeedbackSize = 12;
            ct = mCSP.CreateDecryptor(mCSP.Key, mCSP.IV);
            byt = Convert.FromBase64String(Value);
            ms = new MemoryStream();
            cs = new CryptoStream(ms, ct, CryptoStreamMode.Write);
            cs.Write(byt, 0, byt.Length);
            cs.FlushFinalBlock();
            cs.Close();
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public class Hash
        {
            public static string StringMD5(string data)
            {
                return (System.BitConverter.ToString(System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(data))));
            }
            public static byte[] BytesMD5(string data)
            {
                return (System.Security.Cryptography.MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(data)));
            }
        }


    }
}


