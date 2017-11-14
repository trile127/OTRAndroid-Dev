using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OTRAndroid.Crypto
{
    public sealed class DigitalSignature
    {
        RSAParameters publicKey;
        RSAParameters privateKey;



        public void GenerateNewRSAKeys()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                RSAParameters publicKey;
                RSAParameters privateKey;
                publicKey = rsa.ExportParameters(false);
                privateKey = rsa.ExportParameters(true);
                
            }
        }
        public RSAParameters getRSAPrivKey()
        {
            return privateKey;
        }

        public RSAParameters getRSAPubKey()
        {
            return publicKey;
        }


        public byte[] SignData(RSAParameters RSAPrivKey, byte[] hashOfDataToSign)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.PersistKeyInCsp = false;
                rsa.ImportParameters(RSAPrivKey);

                var rsaFormatter = new RSAPKCS1SignatureFormatter(rsa);
                rsaFormatter.SetHashAlgorithm("SHA256");

                return rsaFormatter.CreateSignature(hashOfDataToSign);
            }
        }

        public bool VerifySignature(RSAParameters RSAPubKey, byte[] hashOfDataToSign, byte[] signature)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.ImportParameters(RSAPubKey);

                var rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                rsaDeformatter.SetHashAlgorithm("SHA256");

                return rsaDeformatter.VerifySignature(hashOfDataToSign, signature);
            }
        }
    }
}