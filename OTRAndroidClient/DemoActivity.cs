using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AspNet.SignalR.Client;
using Android;


using EntityFrameworkWithXamarin.Core;
using System.Threading.Tasks;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.RLWE;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.RLWE.Arithmetic;

using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Interfaces;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.NTRU;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.NTRU.Arithmetic;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.NTRU.Polynomial;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.NTRU.Curve;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.NTRU.Encode;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Sign.GMSS.Utility;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Sign.GMSS.Arithmetic;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Sign.GMSS;
using VTDev.Libraries.CEXEngine.Crypto.Common;
using VTDev.Libraries.CEXEngine.Crypto.Enumeration;
using VTDev.Libraries.CEXEngine.Crypto.Prng;
using VTDev.Libraries.CEXEngine.CryptoException;
using VTDev.Libraries.CEXEngine.Crypto.Digest;
using VTDev.Libraries.CEXEngine.Crypto.Kdf;
using VTDev.Libraries.CEXEngine.Crypto.Mac;
using VTDev.Libraries.CEXEngine.Crypto.Generator;
using System.IO;
using VTDev.Libraries.CEXEngine.Utility;
using Android.Views.InputMethods;
using Newtonsoft.Json;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Symmetric.Block.Mode;
using System.Security.Cryptography;
using System.Timers;

namespace OTRAndroidClient
{

    //public class User
    //{
    //    public int randomInt { get; set; }
    //    public RLWEParameters Parameters { get; set; }
    //    public RLWEKeyGenerator KeyGen { get; set; }

    //    public NTT512 NT512 { get; set; }
    //    public RLWEPrivateKey PriKey { get; set; }
    //    public RLWEPublicKey PubKey { get; set; }
    //    public RLWEPublicKey UserPubKey { get; set; }

    //    //Signing
    //    public GMSSParameters gmssencParams { get; set; }
    //    public GMSSKeyGenerator gmssgen { get; set; }
    //    public IAsymmetricKeyPair gmsskeyPair { get; set; }
    //    public GMSSPublicKey gmssreceiverPubKey { get; set; }

    //    public string keySignature { get; set; }
    //    public string keyMAC { get; set; }
    //}

    public class User
    {
        public string name { get; set; }
        public int randomInt { get; set; }
        public RLWEParameters Parameters { get; set; }
        public RLWEKeyGenerator KeyGen { get; set; }

        public NTT512 NT512 { get; set; }
        public RLWEPrivateKey PriKey { get; set; }
        public RLWEPublicKey PubKey { get; set; }
        public RLWEPublicKey otherPubKey { get; set; }
        public uint[] priR2 { get; set; }
        public uint[] pubA { get; set; }
        public uint[] pubP { get; set; }

        //Signing
        public GMSSParameters gmssencParams { get; set; }
        public GMSSKeyGenerator gmssgen { get; set; }
        public IAsymmetricKeyPair gmsskeyPair { get; set; }
        public GMSSPublicKey gmssreceiverPubKey { get; set; }
        public string keySignature { get; set; }
        public string keyMAC { get; set; }
    }

    public class Message
    {
        public string MAC { get; set; }
        public string message { get; set; }
    }



    [Activity(Label = "DemoActivity")]
    public class DemoActivity : Activity
    {


        User receiver;
        User initiator;

        TextView MessageText;
        List<string> messages;

        int milliseconds = 1, secs = 0, mins = 0;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.DemoLayout);
            MessageText = FindViewById<TextView>(Resource.Id.MessageTextView);
            MessageText.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();
            receiver = new User();
            receiver.name = "Bob";
            initiator = new User();
            initiator.name = "Alice";
            messages = new List<string>();

            Timer timer = new Timer();
            timer.Interval = 1; // 1 milliseconds  
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            InitializeGMSS(receiver);
            timer.Stop();
            timer.Dispose();
            MessageText.Append("Initialize GMSS Keys Time: " + String.Format("{0}:{1:00}:{2:000}", mins, secs, milliseconds));
            resetTime();
            InitializeGMSS(initiator);
            exchangePubKeys(initiator, receiver);

            messages.Add("Message 1");
            messages.Add("Message 2");
            messages.Add("Message 3");
            messages.Add("Message 5");
            messages.Add("Message 6");
            messages.Add("Message 7");

            initOTR(initiator);
            receiverOTR(initiator, receiver);
            endOTR(initiator, receiver);

            for (int i = 0; i < messages.Count; i++)
            {
                if ( i % 2 == 0)
                {
                    MessageText.Append("\n\n[*] Message: " + i);
                    Message sendingMessage = sendMessage(initiator, receiver, messages[i]);
                    MessageText.Append("\n[*] Alice sent Bob:" + sendingMessage.message);
                    Message receivingMessage = receiveMessage(initiator, receiver, sendingMessage);
                    MessageText.Append("\n[*] Bob decrypted:" + receivingMessage.message);
                    initReKey(initiator);
                    receiveReKeying(initiator, receiver);
                    endReKey(initiator, receiver);
                }
                else
                {
                    MessageText.Append("\n\n[*] Message: " + i);
                    Message sendingMessage = sendMessage(receiver, initiator, messages[i]);
                    MessageText.Append("\n[*] Bob sent Alice:" + sendingMessage.message);
                    Message receivingMessage = receiveMessage(receiver, initiator, sendingMessage);
                    MessageText.Append("\n[*] Alice decrypted:" + receivingMessage.message);
                    initReKey(receiver);
                    receiveReKeying(receiver, initiator);
                    endReKey(receiver, initiator);

                } 
            }



        }

        public Message sendMessage(User user, User receiveUser, string message)
        {
            Timer timer = new Timer();
            timer.Interval = 1; // 1 milliseconds  
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            string enc = encryptData(message, user.PriKey);
            timer.Stop();
            timer.Dispose();
            MessageText.Append("Encryption Time: " + String.Format("{0}:{1:00}:{2:000}", mins, secs, milliseconds));
            resetTime();
            string MAC = createHMAC(message, user.PriKey);

            Message messageObj = new Message();
            messageObj.message = enc;
            messageObj.MAC = MAC;


            return messageObj;
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            milliseconds++;
            if (milliseconds >= 1000)
            {
                secs++;
                milliseconds = 0;
            }
            if (secs == 59)
            {
                mins++;
                secs = 0;
            }
        }

        private void resetTime()
        {
            milliseconds = 1;
            secs = 0;
            mins = 0;
        }

            public Message receiveMessage(User user, User receiveUser, Message messageobj)
        {
            Timer timer = new Timer();
            timer.Interval = 1; // 1 milliseconds  
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            string dec = decryptData(messageobj.message, user.PriKey);
            timer.Stop();
            timer.Dispose();
            MessageText.Append("Decryption Time: " + String.Format("{0}:{1:00}:{2:000}", mins, secs, milliseconds));
            resetTime();
            bool MAC = checkHMAC(dec, user.PriKey, messageobj.MAC);

            if (MAC == true)
            {
                MessageText.Append("\n[*] " + receiveUser.name + "Verified Incoming Message MAC");
                Message messageObj = new Message();
                messageObj.message = dec;
                messageObj.MAC = messageobj.MAC;
                return messageObj;
            }
            else
            {
                MessageText.Append("\n[*] " + user.name + "Message MAC Failed");
                return null;
            }
        }


        public void InitializeGMSS(User user)
        {
            user.gmssencParams = (GMSSParameters)GMSSParamSets.GMSSN2P10.DeepCopy();
            user.gmssgen = new GMSSKeyGenerator(user.gmssencParams);
            user.gmsskeyPair = user.gmssgen.GenerateKeyPair();
            MessageText.Append("\n[*] " +user.name +"Generated GMSS Keys");

        }


        public void exchangePubKeys(User initiator, User receiver)
        {
            initiator.gmssreceiverPubKey = (GMSSPublicKey)receiver.gmsskeyPair.PublicKey;
            receiver.gmssreceiverPubKey = (GMSSPublicKey)initiator.gmsskeyPair.PublicKey;
        }


        public void initOTR(User user)
        {
            //Start Timer
            Timer timer = new Timer();
            timer.Interval = 1; // 1 milliseconds  
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            
            
            int M = 512;
            uint[] pubA = new uint[M];
            uint[] pubP = new uint[M];
            uint[] priR2 = new uint[M];

            user.Parameters = RLWEParamSets.RLWEN512Q12289;
            user.KeyGen = new RLWEKeyGenerator(user.Parameters);
            user.NT512 = new NTT512(user.KeyGen.m_rndEngine);

            user.randomInt = (int)user.NT512.GetRand();
            user.NT512.KeyGen(pubA, pubP, priR2, user.randomInt);
            user.priR2 = priR2;
            user.pubA = pubA;
            user.pubP = priR2;
            //Send random value r and parameters and public key.

            user.PriKey = new RLWEPrivateKey(M, user.NT512.Convert32To8(user.priR2));
            user.PubKey = new RLWEPublicKey(M, user.NT512.Convert32To8(user.pubA), user.NT512.Convert32To8(user.pubP));

            MessageText.Append("\n[*] Initiate OTR Keying");
            MessageText.Append("\n[*] " + user.name + " Generated Params and Keys");
            //Stop Timer
            timer.Stop();
            timer.Dispose();
            MessageText.Append("RLWE InitKey Generation Time: " + String.Format("{0}:{1:00}:{2:000}", mins, secs, milliseconds));
            resetTime();

            String hashPubKey = SHA512Hash(Encoding.ASCII.GetString(user.PubKey.ToBytes()));
            user.keySignature = GMSSSignature(user.gmssencParams, user.gmsskeyPair, hashPubKey);
            

        }

        public void receiverOTR(User user, User receiveUser)
        {
            int M = 512;
            uint[] recpubA = new uint[M];
            uint[] recpubP = new uint[M];
            uint[] recpriR2 = new uint[M];
            byte[] bytePS = user.Parameters.ToBytes();
            receiveUser.Parameters = new RLWEParameters(bytePS);
            receiveUser.KeyGen = new RLWEKeyGenerator(receiveUser.Parameters);
            receiveUser.NT512 = user.NT512;
            receiveUser.randomInt = user.randomInt;
            receiveUser.priR2 = user.priR2;
            receiveUser.pubA = user.pubA;
            receiveUser.pubP = user.priR2;
            //receiveUser.NT512.KeyGen(recpubA, recpubP, recpriR2, receiveUser.randomInt);
            //Send random value r and parameters and public key.
            receiveUser.PriKey = new RLWEPrivateKey(M, receiveUser.NT512.Convert32To8(receiveUser.priR2));
            receiveUser.PubKey = new RLWEPublicKey(M, receiveUser.NT512.Convert32To8(receiveUser.pubA), receiveUser.NT512.Convert32To8(receiveUser.pubP));
            receiveUser.otherPubKey = user.PubKey;

            String checkhashPubKey = SHA512Hash(Encoding.ASCII.GetString(user.PubKey.ToBytes()));
            string checkSignature = GMSSSignature(receiveUser.gmssencParams, user.gmsskeyPair, checkhashPubKey);

            if (checkSignature == user.keySignature)
            {
                MessageText.Append("\n[*] " + receiveUser.name + " Verified " + user.name + "'s Signature");
            }
            else
                MessageText.Append("\n[*] " + user.name + "'s Signature Failed");


            //if (VerifyGMSSSignature(receiveUser.gmssencParams, user.gmsskeyPair, checkhashPubKey, user.keySignature) == true)
            //{
            //    MessageText.Append("\n[*] " + receiveUser.name + " Verified " + user.name + "'s Signature");
            //}
            //else
            //    MessageText.Append("\n[*] " + user.name + "'s Signature Failed");

            if (receiveUser.PriKey.Equals(user.PriKey))
            {
                bool validSharedKeys = true;
            }


            String hashPubKey = SHA512Hash(Encoding.ASCII.GetString(receiveUser.PubKey.ToBytes()));
            receiveUser.keySignature = GMSSSignature(receiveUser.gmssencParams, receiveUser.gmsskeyPair, hashPubKey);

            MessageText.Append("\n[*] " + receiveUser.name + " Generated Params and Keys");
            MessageText.Append("\n[*] " + receiveUser.name + " Received" + user.name + "'s Public Key");
        }


        public void endOTR(User user, User receiveUser)
        {
            user.otherPubKey = receiveUser.PubKey;

            String checkhashPubKey = SHA512Hash(Encoding.ASCII.GetString(receiveUser.PubKey.ToBytes()));
            string checkSignature = GMSSSignature(user.gmssencParams, receiveUser.gmsskeyPair, checkhashPubKey);

            if (checkSignature == receiveUser.keySignature)
            {
                MessageText.Append("\n[*] " + user.name + " Verified " + receiveUser.name + "'s Signature");
            }
            else
                MessageText.Append("\n[*] " + receiveUser.name + "'s Signature Failed");

            //if (VerifyGMSSSignature(user.gmssencParams, receiveUser.gmsskeyPair, checkhashPubKey, receiveUser.keySignature) == true)
            //{
            //    MessageText.Append("\n[*] " + user.name + " Verified " + receiveUser.name + "'s Signature");
            //}
            //else
            //    MessageText.Append("\n[*] " + receiveUser.name + "'s Signature Failed");

            MessageText.Append("\n[*] " + user.name + " Received " + receiveUser.name + "'s Public Key");
            MessageText.Append("\n[*] End Initial OTR Keying");
        }




        public void initReKey(User user)
        {
            int M = 512;
            uint[] recpubA = new uint[M];
            uint[] recpubP = new uint[M];
            uint[] recpriR2 = new uint[M];

            user.randomInt = (int)user.NT512.GetRand();
            user.NT512.KeyGen(recpubA, recpubP, recpriR2, user.randomInt);
            user.priR2 = recpriR2;
            user.pubA = recpubA;
            user.pubP = recpubP;
            //Send random value r and parameters and public key.
            user.PriKey = new RLWEPrivateKey(M, user.NT512.Convert32To8(user.priR2));
            user.PubKey = new RLWEPublicKey(M, user.NT512.Convert32To8(user.pubA), user.NT512.Convert32To8(user.pubP));

            user.keyMAC = createHMAC(Encoding.ASCII.GetString(user.PubKey.ToBytes()), user.PriKey);
            MessageText.Append("\n[*] " + user.name + " Initiated Re-Keying");
        }
        public void receiveReKeying(User user, User receiveUser)
        {
            int M = 512;
            uint[] recpubA = new uint[M];
            uint[] recpubP = new uint[M];
            uint[] recpriR2 = new uint[M];
            byte[] bytePS = user.Parameters.ToBytes();


            receiveUser.randomInt = user.randomInt;
            receiveUser.NT512.KeyGen(recpubA, recpubP, recpriR2, receiveUser.randomInt);
            receiveUser.priR2 = user.priR2;
            receiveUser.pubA = user.pubA;
            receiveUser.pubP = user.priR2;

            //Send random value r and parameters and public key.
            receiveUser.PriKey = new RLWEPrivateKey(M, receiveUser.NT512.Convert32To8(receiveUser.priR2));
            receiveUser.PubKey = new RLWEPublicKey(M, receiveUser.NT512.Convert32To8(receiveUser.pubA), receiveUser.NT512.Convert32To8(receiveUser.pubP));

            receiveUser.keyMAC = createHMAC(Encoding.ASCII.GetString(receiveUser.PubKey.ToBytes()), receiveUser.PriKey);

            string checkMAC = createHMAC(Encoding.ASCII.GetString(user.PubKey.ToBytes()), receiveUser.PriKey);

            if (checkHMAC(Encoding.ASCII.GetString(user.PubKey.ToBytes()), receiveUser.PriKey, checkMAC) == true)
            {
                MessageText.Append("\n[*] " + receiveUser.name + " Verified " + user.name + "'s HMAC");
            }
            else
                MessageText.Append("\n[*] " + user.name + "'s HMAC Failed");

            MessageText.Append("\n[*] " + receiveUser.name + " Received Rekeying information from " + user.name + "");
        }

        public void endReKey(User user, User receiveUser)
        {
            user.otherPubKey = receiveUser.PubKey;


            string checkMAC = createHMAC(Encoding.ASCII.GetString(receiveUser.PubKey.ToBytes()), user.PriKey);
            if (checkHMAC(Encoding.ASCII.GetString(user.otherPubKey.ToBytes()), user.PriKey, checkMAC) == true)
            {
                MessageText.Append("\n[*] " + user.name + " Verified " + receiveUser.name + "'s HMAC");
            }
            else
                MessageText.Append("\n[*] " + receiveUser.name + "'s HMAC Failed");


            MessageText.Append("\n[*] " + user.name + " Received " + receiveUser.name + "'s Public Key");
            MessageText.Append("\n[*] End Rekey");
        }

        public string SHA512Hash(string data)
        {
            byte[] toBytes = Encoding.ASCII.GetBytes(data);
            byte[] hashData;

            //encrypt an array
            using (IDigest hash = new VTDev.Libraries.CEXEngine.Crypto.Digest.SHA512())
            {
                hashData = hash.ComputeHash(toBytes);
            }

            return Encoding.ASCII.GetString(hashData);
        }

        public string createHMAC(string data, RLWEPrivateKey privKey)
        {
            byte[] keyBytes = privKey.ToBytes();
            byte[] Output;
            using (IMac mac = new VTDev.Libraries.CEXEngine.Crypto.Mac.HMAC(new VTDev.Libraries.CEXEngine.Crypto.Digest.SHA256()))
            {
                // initialize
                mac.Initialize(keyBytes, null);
                // get mac
                Output = mac.ComputeMac(Encoding.ASCII.GetBytes(data));
            }
            return Encoding.ASCII.GetString(Output);
        }

        public bool checkHMAC(string data, RLWEPrivateKey privKey, string MAC)
        {
            byte[] keyBytes = privKey.ToBytes();
            byte[] Output;
            bool valid = false;
            using (IMac mac = new VTDev.Libraries.CEXEngine.Crypto.Mac.HMAC(new VTDev.Libraries.CEXEngine.Crypto.Digest.SHA256()))
            {
                // initialize
                mac.Initialize(keyBytes, null);
                // get mac
                Output = mac.ComputeMac(Encoding.ASCII.GetBytes(data));
                
            }
            if (Encoding.ASCII.GetString(Output) == MAC)
            {
                valid = true;
            }

            return valid;
        }

        public string GMSSSignature(GMSSParameters encParams, IAsymmetricKeyPair keyPair, string data)
        {

            byte[] code;
            byte[] input = Encoding.ASCII.GetBytes(data);
            // get the message code for an array of bytes
            using (GMSSSign sign = new GMSSSign(encParams))
            {
                sign.Initialize(keyPair.PrivateKey);
                code = sign.Sign(input, 0, input.Length);
            }

            return Encoding.ASCII.GetString(code);
        }

        public bool VerifyGMSSSignature(GMSSParameters encParams, IAsymmetricKeyPair keyPair, string data, string code)
        {

            byte[] codeBytes = Encoding.ASCII.GetBytes(code);
            byte[] input = Encoding.ASCII.GetBytes(data);
            bool valid = false;
            // get the message code for an array of bytes
            // test the message for validity
            using (GMSSSign sign = new GMSSSign(encParams))
            {
                sign.Initialize(keyPair.PublicKey);
                valid = sign.Verify(input, 0, input.Length, codeBytes);
            }

            return valid;
        }




        private static byte[] CreateKey(string password, int keyBytes = 32)
        {
            byte[] salt = new byte[] { 80, 70, 60, 50, 40, 30, 20, 10 };
            int iterations = 300;
            var keyGenerator = new Rfc2898DeriveBytes(password, salt, iterations);
            return keyGenerator.GetBytes(keyBytes);
        }

        private static byte[] AesEncryptStringToBytes(string plainText, byte[] key, byte[] iv)
        {
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException($"{nameof(plainText)}");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException($"{nameof(key)}");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException($"{nameof(iv)}");

            byte[] encrypted;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                    encrypted = memoryStream.ToArray();
                }
            }
            return encrypted;
        }

        //private String ReadCPUinfo()
        //{
        //    Java.Lang.ProcessBuilder cmd;
        //    String result = "";

        //    try
        //    {
        //        String[] args = { "/system/bin/cat", "/proc/cpuinfo" };
        //        cmd = new Java.Lang.ProcessBuilder(args);

        //        Java.Lang.Process process = cmd.Start();
        //        var input = process.InputStream;
        //        byte[] re = new byte[1024];
        //        while (input.Read(re, 0, re.Length) != -1){
        //            MessageText.Append("\n\n[*] CPU: " + re);
        //            System.out.println(new String(re));
        //            result = result + new String(re);
        //        }
        //    in.close();
        //    }
        //    catch (IOException ex)
        //    {
        //        ex.printStackTrace();
        //    }
        //    return result;
        //}


        public static string Encrypt(string clearValue, string encryptionKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = CreateKey(encryptionKey);

                byte[] encrypted = AesEncryptStringToBytes(clearValue, aes.Key, aes.IV);
                return Convert.ToBase64String(encrypted) + ";" + Convert.ToBase64String(aes.IV);
            }
        }

        public static string Decrypt(string encryptedValue, string encryptionKey)
        {
            string iv = encryptedValue.Substring(encryptedValue.IndexOf(';') + 1, encryptedValue.Length - encryptedValue.IndexOf(';') - 1);
            encryptedValue = encryptedValue.Substring(0, encryptedValue.IndexOf(';'));

            return AesDecryptStringFromBytes(Convert.FromBase64String(encryptedValue), CreateKey(encryptionKey), Convert.FromBase64String(iv));
        }

        private static string AesDecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException($"{nameof(cipherText)}");
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException($"{nameof(key)}");
            if (iv == null || iv.Length <= 0)
                throw new ArgumentNullException($"{nameof(iv)}");

            string plaintext = null;

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (MemoryStream memoryStream = new MemoryStream(cipherText))
                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream))
                    plaintext = streamReader.ReadToEnd();

            }
            return plaintext;
        }


        public string encryptData(string data, RLWEPrivateKey privateKey)
        {
            string Key = Encoding.ASCII.GetString(privateKey.ToBytes());
            String encryptedText = Encrypt(data, Key);
            return encryptedText;
        }

        public string decryptData(string encData, RLWEPrivateKey privateKey)
        {
            string Key = Encoding.ASCII.GetString(privateKey.ToBytes());
            String encryptedText = Decrypt(encData, Key);
            return encryptedText;
        }

    }
}