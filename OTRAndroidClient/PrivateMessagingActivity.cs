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

using System.IO;
using VTDev.Libraries.CEXEngine.Utility;
using Android.Views.InputMethods;
using Newtonsoft.Json;

namespace OTRAndroidClient
{
    [Activity(Label = "PrivateMessaging")]
    public class PrivateMessagingActivity : Activity
    {

        String UserName;
        String Email;
        int userID;
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


            privChatUser.Text = UserName;

            privateadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem2, myListItems);
            messages.Adapter = privateadapter;

            hub = ChatActivity.hubConnection;
            proxy = ChatActivity.chatHubProxy;
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
                      return;
                  }
                  else
                  {
                      String encData = encryptData(input.Text.Trim());
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

        public string encryptData(string data)
        {
            byte[] toBytes = Encoding.ASCII.GetBytes(data);
            byte[] enc;
            // encrypt an array
            using (RLWEEncrypt cipher = new RLWEEncrypt(myParameters))
            {
                cipher.Initialize(receiverpubKey);
                enc = cipher.Encrypt(toBytes);
            }
            return Encoding.ASCII.GetString(enc);
        }

        public string decryptData(string data, int length)
        {
            byte[] toBytes = Encoding.ASCII.GetBytes(data);
            byte[] dec;
            // encrypt an array
            using (RLWEEncrypt cipher = new RLWEEncrypt(myParameters))
            {
                cipher.Initialize(priKey);
                dec = cipher.Encrypt(toBytes);
            }
            String decryptedData = Encoding.ASCII.GetString(dec);
            decryptedData = decryptedData.Substring(0, length);
            return decryptedData;
        }

        public async Task Connect()
        {
            try
            {
                proxy.On<string, string, List<ChatUserDetail>, List<ChatMessageDetail>>("onConnected", (connectionID, UserName, allUsers, messages) =>
                {
                    myconnectionID = connectionID;
                    myUsers = allUsers;
                    myMessages = messages;
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

                proxy.On<ChatUserDetail, RLWEPublicKey, RLWEParameters, int>("receiveParams", (fromUser, pubKey, parameters, random) =>
                {
                    receiveOTR(parameters, random, pubKey);
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
            proxy.Invoke("InitOTR", toconnectionID, myParameters, r, initiatorpubKey);
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
            proxy.Invoke("receiverSendOTR", toconnectionID, receiverpubKey);

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
            proxy.Invoke("initReKey", toconnectionID, r, mypubKey);
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

            proxy.Invoke("receiverReKey", toconnectionID, mypubKey);
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

            proxy.Invoke("sendGMSS", toconnectionID, receiverpubKey);
        }

        public void setReceiverGMSSPubKey(GMSSPublicKey pubKey)
        {
            gmssreceiverPubKey = pubKey;
            MessageText.Append("\n[*] Received receiver GMSS PubKey");
        }



    }
}