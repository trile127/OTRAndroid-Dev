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

namespace OTRAndroidClient
{
    [Activity(Label = "PrivateMessaging")]
    public class PrivateMessagingActivity : Activity
    {

        String UserName;
        String Email;
        String myUserName;
        String myEmail;
        String myconnectionID;
        String toconnectionID;
        String otrInit;
        HubConnection hub;
        IHubProxy proxy;

        List<ChatUserDetail> myUsers;
        List<ChatMessageDetail> myMessages;
        List<String> messageList;
        List<PrivateChatMessage> myPrivateMessages;

        EditText input;
        TextView MessageText;
        ListView messages;
        TextView privChatUser;
        Button StartOTR;

        InputMethodManager inputManager;
        ArrayAdapter privateadapter;
        List<string> myListItems;


        //RLWE
        int r;
        RLWEParameters myParameters;
        RLWEKeyGenerator myKeyGen;
        NTT512 ntt512Test;
        RLWEPrivateKey priKey;
        RLWEPublicKey initiatorpubKey;
        RLWEPublicKey receiverpubKey;
        RLWEPublicKey mypubKey;

        //Signing
        GMSSParameters gmssencParams;
        GMSSKeyGenerator gmssgen;
        IAsymmetricKeyPair gmsskeyPair;
        GMSSPublicKey gmssreceiverPubKey;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PrivateMessage);

            //Private Chat
            input = FindViewById<EditText>(Resource.Id.Input);
            messages = FindViewById<ListView>(Resource.Id.PrivateMessages);
            privChatUser = FindViewById<TextView>(Resource.Id.PrivateChatUser);
            myListItems = new List<string>();
            MessageText = FindViewById<TextView>(Resource.Id.MessageTextView);
            MessageText.MovementMethod = new Android.Text.Method.ScrollingMovementMethod();
            StartOTR = FindViewById<Button>(Resource.Id.otrButton);
            StartOTR.Click += delegate {
                //Insert code
                InitializeGMSS();
                initOTR();
            };
            Bundle bundler = Intent.GetBundleExtra("bundle");
            UserName = bundler.GetString("UserName");
            Email = bundler.GetString("Email");
            toconnectionID = bundler.GetString("ConnectionID");
            otrInit = bundler.GetString("OTRINIT");

            myUserName = bundler.GetString("UserName");
            myEmail = bundler.GetString("Email");
            myconnectionID = bundler.GetString("ConnectionID");

            privChatUser.Text = UserName;

            privateadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem2, myListItems);
            messages.Adapter = privateadapter;

            //hub = ChatActivity.hubConnection;
            //proxy = ChatActivity.chatHubProxy;
            hub = new HubConnection("http://offtherecordfinal.azurewebsites.net/");
            proxy = hub.CreateHubProxy("ChatHub");
            hub.Error += ex => MessageText.Append("SignalR error: " + ex.Message);

            Connect();
            // Create your application here
            inputManager = (InputMethodManager)GetSystemService(InputMethodService);

            //getPrivateMessages(toconnectionID, Email);

            input.EditorAction +=
              delegate
              {
                  inputManager.HideSoftInputFromWindow(input.WindowToken, HideSoftInputFlags.None);

                  if (string.IsNullOrEmpty(input.Text))
                      return;
                  if (priKey == null)
                  {
                      MessageText.Append("OTR connection not completed yet!");
                      proxy.Invoke<List<PrivateChatMessage>>("SendPrivateMessage", toconnectionID, input.Text, "Click");
                  }
                  else
                  {
                      String encData = encryptData(input.Text.Trim());

                      String decData = decryptData(input.Text.Trim(), input.Text.Trim().Length);
                      MessageText.Append(UserName + ": " + input.Text.Trim());
                      proxy.Invoke("SendEncryptedPrivateMessage", toconnectionID, encData, "Click", input.Text.Trim().Length);
                      initReKey();
                  }
                  
                  //proxy.Invoke<List<PrivateChatMessage>>("SendPrivateMessage", toconnectionID, input.Text, "Click");
                  //client.Send(input.Text);

                  input.Text = "";
              };

            if (otrInit == "TRUE")
            {
                int randomNum = Intent.GetIntExtra("Random", 0);
                var publicKey = JsonConvert.DeserializeObject<RLWEPublicKey>(Intent.GetStringExtra("PubKey") ?? null);
                var parameters = JsonConvert.DeserializeObject<RLWEParameters>(Intent.GetStringExtra("Parameterss") ?? null);
                receiveOTR(parameters, randomNum, publicKey);
            }
        }

        public override void OnBackPressed()
        {
            Bundle bundler = new Bundle();
            bundler.PutString("UserName", "Tri Le");
            bundler.PutString("Email", "trixuanle@gmail.com");
            Intent n = new Intent(this, typeof(ChatActivity));
            n.PutExtra("bundle", bundler);
            StartActivity(n);
            Finish();

        }

        public async Task updateListView()
        {
                privateadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem2, myListItems);
                messages.Adapter = privateadapter;
            //adapter.NotifyDataSetChanged();
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


        public string encryptData(string data)
        {
            byte[] toBytes = Encoding.ASCII.GetBytes(data);
            byte[] enc = new byte[2048];
            byte[] IV;
            byte[] hashedKey;


            
            //encrypt an array
            using (IDigest hash = new VTDev.Libraries.CEXEngine.Crypto.Digest.SHA512())
            {
                // compute a hash
                //IV = hash.ComputeHash(mypubKey.ToBytes());
                hashedKey = hash.ComputeHash(mypubKey.ToBytes());
            }

            using (IDigest hash = new VTDev.Libraries.CEXEngine.Crypto.Digest.SHA256())
            {
                IV = hash.ComputeHash(mypubKey.ToBytes());
            }

            String encryptedText = Encrypt(data, Encoding.ASCII.GetString(hashedKey));

            String decryptedText = Decrypt(encryptedText, Encoding.ASCII.GetString(hashedKey));

            //using (ICipherMode cipher = new CTR(BlockCiphers.RHX))
            //{

            //    KeyParams keyParameters = new KeyParams(hashedKey, IV);
            //    var sevenItems = new byte[] { 0x00 };
            //    keyParameters.IKM = sevenItems;
            //    // initialize for encryption
            //    cipher.Initialize(true, keyParameters);
            //    // encrypt a block
            //    cipher.Transform(toBytes, 0, enc, 0);
            //}

            //using (IDigest hash = new SHA512())
            //{
            //    // compute a hash
            //    IV = hash.ComputeHash(mypubKey.ToBytes());
            //    hashedKey = hash.ComputeHash(mypubKey.ToBytes());
            //}


            //using (ICipherMode cipher = new CTR(BlockCiphers.RHX))
            //{
            //    // initialize for encryption
            //    cipher.Initialize(true, new KeyParams(hashedKey, IV));
            //    // encrypt a block
            //    cipher.Transform(toBytes, 0, enc, 0);
            //}

            //using (RLWEEncrypt cipher = new RLWEEncrypt(myParameters))
            //{
            //    cipher.Initialize(receiverpubKey);
            //    enc = cipher.Encrypt(toBytes);
            //}
            String encryptedData = Encoding.ASCII.GetString(enc);
            return encryptedText;
        }



        public string decryptData(string data, int length)
        {
            byte[] toBytes = Encoding.ASCII.GetBytes(data);
            byte[] dec = new byte[2048];
            byte[] hashedKey;
            byte[] IV;
            using (IDigest hash = new VTDev.Libraries.CEXEngine.Crypto.Digest.SHA512())
            {
                // compute a hash
                IV = hash.ComputeHash(mypubKey.ToBytes());
                hashedKey = hash.ComputeHash(mypubKey.ToBytes());
            }

            String decryptedText = Decrypt(data, Encoding.ASCII.GetString(hashedKey));


            //using (ICipherMode cipher = new CTR(BlockCiphers.RHX))
            //{
            //    cipher.Initialize(false, new KeyParams(hashedKey, IV));
            //    // decrypt a block
            //    cipher.Transform(toBytes, 0, dec, 0);
            //}


            //// encrypt an array
            //using (RLWEEncrypt cipher = new RLWEEncrypt(myParameters))
            //{
            //    cipher.Initialize(priKey);
            //    dec = cipher.Encrypt(toBytes);
            //}
            String decryptedData = Encoding.ASCII.GetString(dec);
            decryptedData = decryptedData.Substring(0, length);
            return decryptedText;
        }

        public async Task Connect()
        {
            try
            {
                proxy.On<string, string, List<ChatUserDetail>, List<ChatMessageDetail>>("onConnected", (connectionID, UserName, allUsers, messages) =>
                {
                    if (myconnectionID != connectionID)
                    {
                        myconnectionID = connectionID;
                    }
                    myUsers = allUsers;
                    myMessages = messages;
                }
                );

                proxy.On<string, string, string>("onNewUserConnected", (connectionID, UserName, Email) =>
                {
                    MessageText.Append("\nNew User Connected: " + connectionID + " Name: " + UserName);


                    //updateListView();
                    //adapter.Clear();

                }
              );


                proxy.On<ChatUserDetail, ChatUserDetail, string, string>("sendPrivateMessage2", (fromUserDetails, toUserDetails, status, fromUserId) =>
                {
                    //OpenPrivateMessageAsync(toUserDetails.ConnectionID, toUserDetails.UserName, toUserDetails.EmailID);
                    getPrivateMessages(fromUserDetails.EmailID, toUserDetails.EmailID);
                }
                );

                //Receive messages
                proxy.On<ChatUserDetail, ChatUserDetail, string, int>("sendEncryptedPrivateMessage", (fromUserDetails, toUserDetails, message, length) =>
                {
                    string decryptedText = decryptData(message, length);
                    myListItems.Add(fromUserDetails.UserName + ": " + decryptedText);
                    //privateadapter.Add(myPrivateMessages[i].message);
                    MessageText.Append("\n" + fromUserDetails.UserName + ": " + decryptedText);

                    privateadapter.Clear();
                    foreach (var item in myListItems)
                    {
                        privateadapter.Insert(item, privateadapter.Count);
                    }
                    RunOnUiThread(() => privateadapter.NotifyDataSetChanged());

                }
                );

                proxy.On<ChatUserDetail, RLWEParameters, int, RLWEPublicKey>("receiveParams", (fromUser, parameters, random, pubKey) =>
                {
                    receiveOTR(parameters, random, pubKey);
                }
                );

                proxy.On<ChatUserDetail, RLWEParameters, int, RLWEPublicKey>("calledInitOTR", (fromUser, parameters, random, pubKey) =>
                {
                    MessageText.Append("\nCalledInitOTR" + fromUser.UserName + ": " + random);
                }
               );

                

                proxy.On<RLWEPublicKey>("endOTR", (pubKey) =>
                {
                    endOTR(pubKey);
                }
                );

                proxy.On<int, RLWEPublicKey>("receiveReKeying", (random, pubKey) =>
                {
                    receiveReKeying(random, pubKey);
                }
                );

                proxy.On<RLWEPublicKey>("endReKey", (pubKey) =>
                {
                    endReKey(pubKey);
                }
                );

                proxy.On<GMSSPublicKey>("receiveGMSS", (pubKey) =>
                {
                    setReceiverGMSSPubKey(pubKey);
                }
                );

                await hub.Start();

                if (hub.State == ConnectionState.Connected)
                {

                    await proxy.Invoke("Connect", UserName, Email);

                }


            }
            //proxy.Invoke<List<PrivateChatMessage>>("SendPrivateMessage", toconnectionID, input.Text, "Click");
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }
        }

        public async Task initOTR()
        {
            int M = 512;
            uint[] pubA = new uint[M];
            uint[] pubP = new uint[M];
            uint[] priR2 = new uint[M];
            myParameters = RLWEParamSets.RLWEN512Q12289;
            myKeyGen = new RLWEKeyGenerator(myParameters);
            ntt512Test = new NTT512(myKeyGen.m_rndEngine);

            r = (int)ntt512Test.GetRand();
            ntt512Test.KeyGen(pubA, pubP, priR2, r);
            //Send random value r and parameters and public key.
            priKey = new RLWEPrivateKey(M, ntt512Test.Convert32To8(priR2));
            initiatorpubKey = new RLWEPublicKey(M, ntt512Test.Convert32To8(pubA), ntt512Test.Convert32To8(pubP));
            mypubKey = initiatorpubKey;
            MessageText.Append("\n[*] Initiator Generated Params and Keys:");
            sentInitOTR();
        }

        public async Task sentInitOTR()
        {

            try
            {
                proxy.Invoke("InitOTR", toconnectionID, myParameters, r, initiatorpubKey);

            }
            catch (Exception ex)
            {
                MessageText.Append("Error invoking InitOTR: " + ex.Message);
            }
        }

        public async Task receiveOTR(RLWEParameters parameters, int random, RLWEPublicKey initPubKey)
        {
            int M = 512;
            uint[] recpubA = new uint[M];
            uint[] recpubP = new uint[M];
            uint[] recpriR2 = new uint[M];
            byte[] bytePS = parameters.ToBytes();
            myParameters = new RLWEParameters(bytePS);
            myKeyGen = new RLWEKeyGenerator(myParameters);
            ntt512Test = new NTT512(myKeyGen.m_rndEngine);

            r = random;
            ntt512Test.KeyGen(recpubA, recpubP, recpriR2, r);
            //Send random value r and parameters and public key.
            priKey = new RLWEPrivateKey(M, ntt512Test.Convert32To8(recpriR2));
            receiverpubKey = new RLWEPublicKey(M, ntt512Test.Convert32To8(recpubA), ntt512Test.Convert32To8(recpubP));
            mypubKey = receiverpubKey;
            initiatorpubKey = initPubKey;
            MessageText.Append("\n[*] Receiver Generated Params and Keys:");
            MessageText.Append("\n[*] Receiver Received Initiators's Public Key:");

            sentReceiveOTR();
        }

        public async Task sentReceiveOTR()
        {
            try
            {
                proxy.Invoke("receiverSendOTR", toconnectionID, receiverpubKey);

            }
            catch (Exception ex)
            {
                MessageText.Append("Error invoking receiverSendOTR: " + ex.Message);
            }

        }

        public void endOTR(RLWEPublicKey recPubKey)
        {
            receiverpubKey = recPubKey;
            MessageText.Append("\n[*] Initiator Received Receiver's Public Key:");
        }

        public async Task initReKey()
        {
            int M = 512;
            uint[] recpubA = new uint[M];
            uint[] recpubP = new uint[M];
            uint[] recpriR2 = new uint[M];

            r = (int)ntt512Test.GetRand();
            ntt512Test.KeyGen(recpubA, recpubP, recpriR2, r);
            //Send random value r and parameters and public key.
            priKey = new RLWEPrivateKey(M, ntt512Test.Convert32To8(recpriR2));
            mypubKey = new RLWEPublicKey(M, ntt512Test.Convert32To8(recpubA), ntt512Test.Convert32To8(recpubP));
            MessageText.Append("\n[*] Rekey Protocol Initiated!");

            try
            {
                proxy.Invoke("initReKey", toconnectionID, r, mypubKey);

            }
            catch (Exception ex)
            {
                MessageText.Append("Error invoking initReKey: " + ex.Message);
            }
        }

        public async Task receiveReKeying(int random, RLWEPublicKey recPubKey)
        {
            int M = 512;
            uint[] recpubA = new uint[M];
            uint[] recpubP = new uint[M];
            uint[] recpriR2 = new uint[M];

            r = random;
            ntt512Test.KeyGen(recpubA, recpubP, recpriR2, r);
            //Send random value r and parameters and public key.
            priKey = new RLWEPrivateKey(M, ntt512Test.Convert32To8(recpriR2));
            mypubKey = new RLWEPublicKey(M, ntt512Test.Convert32To8(recpubA), ntt512Test.Convert32To8(recpubP));
            initiatorpubKey = recPubKey;
            MessageText.Append("\n[*] Receiver Received Rekeying information from Initiator:");

            try
            {
                proxy.Invoke("receiverReKey", toconnectionID, mypubKey);

            }
            catch (Exception ex)
            {
                MessageText.Append("Error invoking receiverReKey: " + ex.Message);
            }
        }

        public void endReKey(RLWEPublicKey recPubKey)
        {
            receiverpubKey = recPubKey;
            MessageText.Append("\n[*] Initiator Received Receiver's Public Re-Key:");
        }

        public async Task getPrivateMessages(string toID, string toEmail)
        {
            myPrivateMessages = await proxy.Invoke<List<PrivateChatMessage>>("GetPrivateMessage", toID, toEmail, 15);

            for (int i = 0; i < myPrivateMessages.Count; i++)
            {
                myListItems.Add(toID + ": " + myPrivateMessages[i].message);
                //privateadapter.Add(myPrivateMessages[i].message);
                MessageText.Append("\n" + toID + ": " + myPrivateMessages[i].message);
            }
            //myListItems.Add("TEst GetPrivateMEssage Update");
            //privateadapter.Clear();
            //foreach (var item in myListItems)
            //{
            //    privateadapter.Insert(item, privateadapter.Count);
            //}
            //RunOnUiThread(() => privateadapter.NotifyDataSetChanged());
        }

        public async Task InitializeGMSS()
        {
            gmssencParams = (GMSSParameters)GMSSParamSets.GMSSN2P10.DeepCopy();
            gmssgen = new GMSSKeyGenerator(gmssencParams);
            gmsskeyPair = gmssgen.GenerateKeyPair();
            MessageText.Append("\n[*] Generated GMSS Keys");


            try
            {
                proxy.Invoke("sendGMSS", toconnectionID, gmsskeyPair.PublicKey);

            }
            catch (Exception ex)
            {
                MessageText.Append("Error invoking sendGMSS: " + ex.Message);
            }
        }

        public void setReceiverGMSSPubKey(GMSSPublicKey pubKey)
        {
            gmssreceiverPubKey = pubKey;
            MessageText.Append("\n[*] Received receiver GMSS PubKey");
        }




    }
}