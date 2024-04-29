using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using System;
using System.IO;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;
using System.Text;
using System.Reflection;

namespace Yoo.Core.Common.Encryptor
{
    public class RSACrypto
    {

        private static RSA ReadKeyFromFilePublicKey(string filename)
        {
            string pemContents = System.IO.File.ReadAllText(filename);
            const string RsaPublicKeyHeader = "-----BEGIN PUBLIC KEY-----";
            const string RsaPublicKeyFooter = "-----END PUBLIC KEY-----";
          
            if (pemContents.StartsWith(RsaPublicKeyHeader))
            {
                int endIdx = pemContents.IndexOf(
                    RsaPublicKeyFooter,
                    RsaPublicKeyHeader.Length,
                    StringComparison.Ordinal);

                string base64 = pemContents.Substring(
                    RsaPublicKeyHeader.Length,
                    endIdx - RsaPublicKeyHeader.Length);

                byte[] der = Convert.FromBase64String(base64);
                RSA rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(der, out _);
                return rsa;
            }

            // "BEGIN PRIVATE KEY" (ImportPkcs8PrivateKey),
            // "BEGIN ENCRYPTED PRIVATE KEY" (ImportEncryptedPkcs8PrivateKey),
            // "BEGIN PUBLIC KEY" (ImportSubjectPublicKeyInfo),
            // "BEGIN RSA PUBLIC KEY" (ImportRSAPublicKey)
            // could any/all be handled here.
            throw new InvalidOperationException();
        }

        // used for mwg
        private static string ReadPubPriKeyFromFile(string filePath)
        {
            // Read the key from the file
            string publicKey = File.ReadAllText(filePath);

            // Remove unnecessary whitespace or line breaks from the key
            publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "")
                                 .Replace("-----END PUBLIC KEY-----", "")
                                 .Replace("-----BEGIN PRIVATE KEY-----", "")
                                 .Replace("-----END PRIVATE KEY-----", "")
                                 .Replace("\n", "")
                                 .Replace("\r", "");

            return publicKey;
        }

        private static RSA ReadKeyFromFilePrivateKey(string filename)
        {
            string pemContents = System.IO.File.ReadAllText(filename);
            const string RsaPrivateKeyHeader = "-----BEGIN PRIVATE KEY-----";
            const string RsaPrivateKeyFooter = "-----END PRIVATE KEY-----";

            if (pemContents.StartsWith(RsaPrivateKeyHeader))
            {
                int endIdx = pemContents.IndexOf(
                    RsaPrivateKeyFooter,
                    RsaPrivateKeyHeader.Length,
                    StringComparison.Ordinal);

                string base64 = pemContents.Substring(
                    RsaPrivateKeyHeader.Length,
                    endIdx - RsaPrivateKeyHeader.Length);

                byte[] der = Convert.FromBase64String(base64);
                RSA rsa = RSA.Create();
                rsa.ImportPkcs8PrivateKey(der, out _);
                return rsa;
            }

            // "BEGIN PRIVATE KEY" (ImportPkcs8PrivateKey),
            // "BEGIN ENCRYPTED PRIVATE KEY" (ImportEncryptedPkcs8PrivateKey),
            // "BEGIN PUBLIC KEY" (ImportSubjectPublicKeyInfo),
            // "BEGIN RSA PUBLIC KEY" (ImportRSAPublicKey)
            // could any/all be handled here.
            throw new InvalidOperationException();
        }

        public static string RSAEncrypt(string publicKey, string privateKey, string strVal)
        {
            byte[] CypherTextBArray;
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(strVal);
            string root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            RSA rsa = ReadKeyFromFilePrivateKey(root + "/Controllers/" + privateKey);
            CypherTextBArray = rsa.SignData(dataToEncrypt, HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);
            string result = Convert.ToBase64String(CypherTextBArray);

            return result;

        }

        // used for mwg
        public static string RSAPubPriEncrypt(string publicKey, string privateKey, string strVal)
        {
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(strVal);
            string root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string pubpriKeyMwPkcs1 = ReadPubPriKeyFromFile(root + "/Controllers/" + publicKey);
            byte[] encryptedData = EncryptData(dataToEncrypt, pubpriKeyMwPkcs1);
            //CypherTextBArray = rsa.SignData(dataToEncrypt, HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);
            string result = Convert.ToBase64String(encryptedData);

            return result;

        }
        // used for mwg
        private static byte[] EncryptData(byte[] data, string publicKeyMwPkcs1)
        {
            using (var rsa = RSA.Create())
            {
                // Import the MwPkcs1PublicKey format public key from the provided string
                rsa.ImportSubjectPublicKeyInfo(Convert.FromBase64String(publicKeyMwPkcs1), out _);

                // Encrypt the data
                byte[] encryptedData = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);

                return encryptedData;
            }
        }

        public static string RSADecrypt(string privateKey, string encryptedDataString)
        {
            try
            {
                string root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string pubpriKeyMwPkcs1 = root + "/Controllers/" + privateKey;
                string pemKey = File.ReadAllText(pubpriKeyMwPkcs1);

                // Initialize RSA with the private key
                using (var rsa = RSA.Create())
                {
                    rsa.ImportFromPem(pemKey.ToCharArray());

                    // Decrypt the data
                    var decryptedData = rsa.Decrypt(Convert.FromBase64String(encryptedDataString), RSAEncryptionPadding.Pkcs1);

                    // Convert the decrypted data to a string (assuming it was originally a string)
                    string decryptedText = Encoding.UTF8.GetString(decryptedData);

                    return decryptedText;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }


        public static bool VerifySignature(string text, string signature, string publicKey)
        {
            string root = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            // Import public key           
            RSA rsa = (ReadKeyFromFilePublicKey(root + "/Controllers/" + publicKey));
            
            return rsa.VerifyData(Encoding.UTF8.GetBytes(text), System.Convert.FromBase64String(signature), HashAlgorithmName.MD5, RSASignaturePadding.Pkcs1);

        }

    }
}