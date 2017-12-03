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

namespace OTRAndroidClient
{
    public class PrivateChatMessage
    {
        public string userName { get; set; }
        public string message { get; set; }
    }

    [Activity(Label = "ChatActivity")]
    public class ChatActivity : Activity
    {
        ChatUserDetail mySelf;
        String UserName;
        String Email;
        int userID;
        String myconnectionID;
        List<string> myListItems;
        ListView myListView;
        List<ChatUserDetail> myUsers;
        List<ChatMessageDetail> myMessages;
        List<String> messageList;
        List<PrivateChatMessage> myPrivateMessages;
        ArrayAdapter<string> adapter;
        // Connect to the server
        HubConnection hubConnection;

        // Create a proxy to the 'ChatHub' SignalR Hub
        IHubProxy chatHubProxy;


        EditText input;
        ListView messages;
        TextView privChatUser;
        InputMethodManager inputManager;
        ArrayAdapter privateadapter;


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Chat);
            myListView = FindViewById<ListView>(Resource.Id.myListView);

            input = FindViewById<EditText>(Resource.Id.Input);
            messages = FindViewById<ListView>(Resource.Id.PrivateMessages);
            privChatUser = FindViewById<TextView>(Resource.Id.PrivateChatUser);
            
            myListItems = new List<string>();

            myListView.ItemClick += MyListView_ItemClick;
            InitializeGMSS();
            InitializeNTRU();
            InitializePKE();

            Bundle bundler = Intent.GetBundleExtra("bundle");
            UserName = bundler.GetString("UserName");
            Email = bundler.GetString("Email");

            // Connect to the server
            hubConnection = new HubConnection("http://signalrchat.azurewebsites.net/");

            // Create a proxy to the 'ChatHub' SignalR Hub
            chatHubProxy = hubConnection.CreateHubProxy("ChatHub");

            //chatHubProxy.On<int, string, List<ChatUserDetail>, List<ChatMessageDetail>>("onConnected", (userID, UserName, allUsers, messages) =>
            //{
            //    Email = bundler.GetString("Email");
            //    for (int i = 0; i < allUsers.Count; i++)
            //    {
            //        AddUser(chatHubProxy, allUsers[i].ConnectionID, allUsers[i].UserName, allUsers[i].EmailID);
            //    };
            //    // Add Existing Messages
            //    for (int i = 0; i < messages.Count; i++)
            //    {
            //        AddMessage(messages[i].UserName, messages[i].Message);
            //    };
            //}
            //);


            Connect();
            ////Start the connection
            //await hubConnection.Start();

            //// Invoke the 'UpdateNick' method on the server
            //await chatHubProxy.Invoke("Connect", UserName, Email);

        }

        public async Task Connect()
        {
            try
            {
                chatHubProxy.On<string, string, List<ChatUserDetail>, List<ChatMessageDetail>>("onConnected", (connectionID, UserName, allUsers, messages) =>
                {
                    myconnectionID = connectionID;
                    myUsers = allUsers;
                    myMessages = messages;
                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        AddUser(chatHubProxy, allUsers[i].ConnectionID, allUsers[i].UserName, allUsers[i].EmailID);
                    };
                    // Add Existing Messages
                    for (int i = 0; i < messages.Count; i++)
                    {
                        AddMessage(messages[i].UserName, messages[i].Message);
                    };
                }
                );

                chatHubProxy.On<string, string, string>("onNewUserConnected", (connectionID, UserName, Email) =>
                {
                    AddUser(chatHubProxy, connectionID, UserName, Email);
                }
                );

                chatHubProxy.On<string, string>("onUserDisconnected", (connectionID, UserName) =>
                {
                    myListItems.Remove(UserName);
                    adapter.NotifyDataSetChanged();
                }
                );

                chatHubProxy.On<string, string>("onUserDisconnectedExisting", (connectionID, UserName) =>
                {
                    myListItems.Remove(UserName);
                    adapter.NotifyDataSetChanged();
                }
                );

                chatHubProxy.On<string, string>("messageReceived", (UserName, message) =>
                {
                    AddMessage(UserName, message);
                }
                );

                chatHubProxy.On<ChatUserDetail, ChatUserDetail, string, string>("sendPrivateMessage", (fromUserDetails, toUserDetails, status, fromUserId) =>
                {
                    OpenPrivateMessageAsync(fromUserDetails, toUserDetails);
                    getPrivateMessages(fromUserDetails, toUserDetails);
                }
                );

                await hubConnection.Start();
                

                if (hubConnection.State == ConnectionState.Connected)
                {
                    await chatHubProxy.Invoke("Connect", UserName, Email);
                    adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myListItems);
                    myListView.Adapter = adapter;
                    myListView.ItemClick += MyListView_ItemClick;
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

        }

        public async Task OpenPrivateMessageAsync(ChatUserDetail fromUser, ChatUserDetail toUser)
        {
            inputManager = (InputMethodManager)GetSystemService(InputMethodService);
            privateadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem2, new List<string>());
            privChatUser.Text = fromUser.UserName;
            messages.Adapter = privateadapter;


            input.EditorAction +=
              delegate
              {
                  inputManager.HideSoftInputFromWindow(input.WindowToken, HideSoftInputFlags.None);

                  if (string.IsNullOrEmpty(input.Text))
                      return;


               
                  //client.Send(input.Text);

                  input.Text = "";
              };
            myPrivateMessages = await chatHubProxy.Invoke<List<PrivateChatMessage>>("GetPrivateMessage", fromUser.ConnectionID, toUser.ConnectionID, 10);
            RunOnUiThread(() =>
            {
                for (int i = 0; i < myPrivateMessages.Count; i++)
                {
                    privateadapter.Add(myPrivateMessages[i].message);
                }
                adapter.NotifyDataSetChanged();
            }
            );

        }

        public async Task getPrivateMessages(ChatUserDetail fromUser, ChatUserDetail toUser)
        {
            myPrivateMessages = await chatHubProxy.Invoke<List<PrivateChatMessage>>("GetPrivateMessage", fromUser.ConnectionID, toUser.ConnectionID, 10);
            RunOnUiThread(() =>
            {
                for (int i = 0; i < myPrivateMessages.Count; i++)
                {
                    privateadapter.Add(myPrivateMessages[i].message);
                }
                adapter.NotifyDataSetChanged();
            }
            );
        }





        // Add User
        public void AddUser(IHubProxy chatHub, string id, string name, string email)
        {
            if (email == Email && myconnectionID == id)
            {
                mySelf.ConnectionID = id;
                mySelf.UserName = name;
                mySelf.EmailID = email;
                RunOnUiThread(() => Toast.MakeText(this, "Welcome " + name, ToastLength.Long).Show());

            }
            else
            {
                myListItems.Add(name);
                adapter.NotifyDataSetChanged();
            }
            
        }

        // Add Message
        public void AddMessage(string userName, string message)
        {
            messageList.Add(UserName + ": " + message);
        }



        public void MyListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {

            for (int i = 0; i < myUsers.Count; i++)
            {

                if (myListItems[e.Position] == myUsers[i].UserName)
                {
                    RunOnUiThread(() => Toast.MakeText(this, myUsers[i].UserName, ToastLength.Short).Show());
                    SetContentView(Resource.Layout.PrivateMessage);
                    OpenPrivateMessageAsync(mySelf, myUsers[i]);
                }

            }
            //SetContentView(Resource.Layout.Chat);


        }

        public void InitializeNTRU()
        {


            NTRUParameters prm = NTRUParamSets.APR2011743FAST;
            IAsymmetricKeyPair keyPair;
            byte[] enc, dec;
            byte[] data = new byte[64];

            // generate a key pair
            using (NTRUKeyGenerator gen = new NTRUKeyGenerator(prm))
            {

                keyPair = gen.GenerateKeyPair();
            }


            // encrypt a message
            using (NTRUEncrypt ntru = new NTRUEncrypt(prm))
            {
                // initialize with public key for encryption
                ntru.Initialize(keyPair.PublicKey);
                // encrypt using public key
                enc = ntru.Encrypt(data);
            }

            // decrypt a message
            using (NTRUEncrypt ntru = new NTRUEncrypt(prm))
            {
                // initialize with both keys for decryption
                ntru.Initialize(keyPair);
                // decrypt using key pair
                dec = ntru.Decrypt(enc);
            }
        }


        public void InitializePKE()
        {
            int M = 512;
            uint[] pubA = new uint[M];
            uint[] pubP = new uint[M];
            uint[] priR2 = new uint[M];
            RLWEParameters parameterTest = RLWEParamSets.RLWEN512Q12289;
            RLWEKeyGenerator genTest = new RLWEKeyGenerator(parameterTest);
            NTT512 ntt512Test = new NTT512(genTest.m_rndEngine);

            int r = (int)ntt512Test.GetRand();
            ntt512Test.KeyGen(pubA, pubP, priR2, r);


            RLWEPrivateKey pri = new RLWEPrivateKey(M, ntt512Test.Convert32To8(priR2));
            RLWEPublicKey pub = new RLWEPublicKey(M, ntt512Test.Convert32To8(pubA), ntt512Test.Convert32To8(pubP));



            uint[] pubA2 = new uint[M];
            uint[] pubP2 = new uint[M];
            uint[] priR22 = new uint[M];
            ntt512Test.KeyGen(pubA2, pubP2, priR22, r);


            RLWEPrivateKey pri2 = new RLWEPrivateKey(M, ntt512Test.Convert32To8(priR2));
            RLWEPublicKey pub2 = new RLWEPublicKey(M, ntt512Test.Convert32To8(pubA), ntt512Test.Convert32To8(pubP));

            bool Privtester = pri2.Equals(pri);
            bool pubTest = pub2.Equals(pub);


            byte[] toBytes2 = Encoding.UTF8.GetBytes("Test");
            byte[] enc2, dec2;
            // encrypt an array
            using (RLWEEncrypt cipher = new RLWEEncrypt(parameterTest))
            {
                cipher.Initialize(pub2);
                enc2 = cipher.Encrypt(toBytes2);
            }

            // decrypt the cipher text
            using (RLWEEncrypt cipher = new RLWEEncrypt(parameterTest))
            {
                cipher.Initialize(pri2);
                dec2 = cipher.Decrypt(enc2);
            }
            string test12 = Encoding.UTF8.GetString(toBytes2);
            string test22 = Encoding.UTF8.GetString(enc2);
            string test32 = Encoding.UTF8.GetString(dec2);
            test32 = test32.Substring(0, test12.Length);


            RLWEKeyPair keyPair = new RLWEKeyPair(pub, pri);
            // convert a key to a byte array
            byte[] rlwekeyArray = keyPair.PublicKey.ToBytes();
            // deserialize a key
            RLWEPublicKey rlwePubKey = new RLWEPublicKey(rlwekeyArray);


            //Initiator generate key pair
            // Public Key -- > Sends along with Digital signature of private key
            // Sends hash of private key

            //

            //// use a pre-defined parameter set

            // Initiator
            RLWEParameters ps = RLWEParamSets.RLWEN512Q12289;
            byte[] bytePS = ps.ToBytes();
            // initialze the key generator
            RLWEKeyGenerator gen = new RLWEKeyGenerator(ps);
            // create the key-pair
            IAsymmetricKeyPair kp = gen.GenerateKeyPair();

            // convert a key to a byte array
            byte[] keyArray = kp.PublicKey.ToBytes();
            // deserialize a key
            RLWEPublicKey npubk = new RLWEPublicKey(keyArray);


            //Receiver:
            RLWEParameters test = new RLWEParameters(bytePS);
            //RLWEParameters test = RLWEParameters.From(byteParams);
            bool tester = test.Equals(ps);

            RLWEKeyGenerator Bobgen = new RLWEKeyGenerator(test);
            // create the key-pair
            IAsymmetricKeyPair Bobps = Bobgen.GenerateKeyPair();

            //// convert a key to a stream
            //MemoryStream keyStream = kp.PrivateKey.ToStream(); 
            //// deserialize key
            //RLWEPrivateKey nprik = new RLWEPrivateKey(keyStream);

            byte[] toBytes = Encoding.ASCII.GetBytes("Test");
            byte[] enc, dec;
            // encrypt an array
            using (RLWEEncrypt cipher = new RLWEEncrypt(ps))
            {
                cipher.Initialize(Bobps.PublicKey);
                enc = cipher.Encrypt(toBytes);
            }

            // decrypt the cipher text
            using (RLWEEncrypt cipher = new RLWEEncrypt(test))
            {
                cipher.Initialize(Bobps.PrivateKey);
                dec = cipher.Decrypt(enc);
            }
            string test1 = Encoding.ASCII.GetString(toBytes);
            string test2 = Encoding.ASCII.GetString(enc);
            string test3 = Encoding.ASCII.GetString(dec);

            //myListItems.Add(test1);
            //myListItems.Add(test2.Substring(0, 64));
            //myListItems.Add(test3.Substring(0, test1.Length));
            

            //int intValue = 512;
            //byte[] intBytes = BitConverter.GetBytes(intValue);
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(intBytes);
            //byte[] result = intBytes;

        }

        void InitializeGMSS()
        {
            byte[] code;
            byte[] data = new byte[100];
            GMSSParameters encParams = (GMSSParameters)GMSSParamSets.GMSSN2P10.DeepCopy();
            GMSSKeyGenerator gen = new GMSSKeyGenerator(encParams);
            IAsymmetricKeyPair keyPair = gen.GenerateKeyPair();

            // get the message code for an array of bytes
            using (GMSSSign sign = new GMSSSign(encParams))
            {
                sign.Initialize(keyPair.PrivateKey);
                code = sign.Sign(data, 0, data.Length);
            }

            // test the message for validity
            using (GMSSSign sign = new GMSSSign(encParams))
            {
                sign.Initialize(keyPair.PublicKey);
                bool valid = sign.Verify(data, 0, data.Length, code);
            }

        }

    }
}
