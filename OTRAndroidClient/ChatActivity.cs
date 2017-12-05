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
        ListView myListView2;
        List<ChatUserDetail> myUsers;
        List<ChatMessageDetail> myMessages;
        List<String> messageList;
        List<PrivateChatMessage> myPrivateMessages;
        ArrayAdapter<string> adapter;
        EditText toUser;
        // Connect to the server
        public static HubConnection hubConnection;
        // Create a proxy to the 'ChatHub' SignalR Hub
        public static IHubProxy chatHubProxy;

        EditText input;
        ListView messages;
        TextView privChatUser;
        InputMethodManager inputManager;
        ArrayAdapter privateadapter;

        bool isConnected;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Chat);
            myListView = FindViewById<ListView>(Resource.Id.myListView);
            myListView2 = FindViewById<ListView>(Resource.Id.myListView2);

            //Private Chat
            // privateadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem2, new List<string>());
            // input = FindViewById<EditText>(Resource.Id.Input);
            // messages = FindViewById<ListView>(Resource.Id.PrivateMessages);
            // privChatUser = FindViewById<TextView>(Resource.Id.PrivateChatUser);
            // toUser = FindViewById<EditText>(Resource.id.ToUser);

            myListItems = new List<string>();
            myListView.ItemClick += MyListView_ItemClick;
            // myListView2.ItemClick += MyListView_ItemClick;
            //InitializeGMSS();
            //InitializeNTRU();
            InitializePKE();

            Bundle bundler = Intent.GetBundleExtra("bundle");
            UserName = bundler.GetString("UserName");
            Email = bundler.GetString("Email");

            isConnected = false;
            // Connect to the server
            hubConnection = new HubConnection("http://offtherecordfinal.azurewebsites.net/");
            // Create a proxy to the 'ChatHub' SignalR Hub
            chatHubProxy = hubConnection.CreateHubProxy("ChatHub");
            String errors = "";
            hubConnection.Error += ex => errors = ex.Message;

            Connect();
            
        }
        public async Task updateListView()
        {
            if (isConnected = true)
            {
                adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myListItems);
                myListView.Adapter = adapter;
            }

            //adapter.NotifyDataSetChanged();
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
                        // if (allUsers[i].EmailID == Email && myconnectionID == allUsers[i].ConnectionID && UserName == allUsers[i].UserName)
                        //     myListItems.Add(allUsers[i].UserName);
                        AddUser(chatHubProxy, allUsers[i].ConnectionID, allUsers[i].UserName, allUsers[i].EmailID);
                    }

                    // Add Existing Messages
                    for (int i = 0; i < messages.Count; i++)
                    {
                        AddMessage(messages[i].UserName, messages[i].Message);
                    }
                    isConnected = true;

                    
                }


                //updateListView();

                );

                chatHubProxy.On<string, string, string>("onNewUserConnected", (connectionID, UserName, Email) =>
                {

                    for (int i = 0; i < myUsers.Count; i++)
                    {
                        if (myUsers[i].EmailID != Email && connectionID != myUsers[i].ConnectionID && UserName != myUsers[i].UserName)
                            AddUser(chatHubProxy, connectionID, UserName, Email);
                    }


                    //updateListView();
                    //adapter.Clear();

                    //for (int i = 0; i < myListItems.Count; i++)
                    //{
                    //    if (UserName == myListItems[i])
                    //    {
                    //        adapter.Insert(myListItems[i], i);
                    //    }
                    //}
                    //adapter.NotifyDataSetChanged();
                }
                );

                chatHubProxy.On<string, string>("onUserDisconnected", (connectionID, UserName) =>
                {
                    myListItems.Remove(UserName);
                    updateListView();
                    //adapter.Clear();
                    //foreach (var item in myListItems)
                    //{
                    //    adapter.Insert(item, adapter.Count);
                    //}
                    //RunOnUiThread(() => adapter.NotifyDataSetChanged());
                }
                );

                chatHubProxy.On<string, string>("onUserDisconnectedExisting", (connectionID, UserName) =>
                {
                    myListItems.Remove(UserName);
                    //adapter.Clear();
                    //foreach (var item in myListItems)
                    //{
                    //    adapter.Insert(item, adapter.Count);
                    //}
                    //RunOnUiThread(() => adapter.NotifyDataSetChanged());
                }
                );

                chatHubProxy.On<string, string>("messageReceived", (UserName, message) =>
                {
                    AddMessage(UserName, message);
                }
                );

                chatHubProxy.On<ChatUserDetail, ChatUserDetail, string, string>("sendPrivateMessage2", (fromUserDetails, toUserDetails, status, fromUserId) =>
                {
                    OpenPrivateMessageAsync(toUserDetails.ConnectionID, toUserDetails.UserName, toUserDetails.EmailID);
                    getPrivateMessages(fromUserDetails.ConnectionID, toUserDetails.EmailID);
                //privateadapter.NotifyDataSetChanged();
                }
                );

                //chatHubProxy.On<ChatUserDetail, RLWEParameters, int, RLWEPublicKey>("receiveParams", (fromUser, parameters, random, pubKey) =>
                //{
                //     for (int i = 0; i < myUsers.Count; i++)
                //     {
                //         if (myUsers[i].UserName == fromUser.UserName)
                //         {
                //             Bundle bundle = new Bundle();
                //             bundle.PutString("UserName", myUsers[i].UserName);
                //             bundle.PutString("Email", myUsers[i].EmailID);
                //             bundle.PutString("ConnectionID", myUsers[i].ConnectionID);
                //             bundle.PutString("OTRINIT", "TRUE");
                //             Intent n = new Intent(this, typeof(PrivateMessagingActivity));
                //             n.PutExtra("bundle", bundle);
                //             n.PutExtra("PubKey", JsonConvert.SerializeObject(pubKey));
                //             n.PutExtra("Random", random);
                //             n.PutExtra("Parameters", JsonConvert.SerializeObject(parameters));
                //             StartActivity(n);
                //             Finish();

                //         }
                //     }
                // }
                //);

                await hubConnection.Start();

                if (hubConnection.State == ConnectionState.Connected)
                {
                    await chatHubProxy.Invoke("Connect", UserName, Email);
                    await updateListView();

                }
            }



            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

        }

        public async Task OpenPrivateMessageAsync(string toID, string toUserName, string toEmail)
        {
            // SetContentView(Resource.Layout.PrivateMessage);
            // inputManager = (InputMethodManager)GetSystemService(InputMethodService);
            // privChatUser.Text = toUserName;
            // messages.Adapter = privateadapter;
            // await getPrivateMessages(toID, toEmail);
            // privateadapter.NotifyDataSetChanged();
            // //messages.Adapter = privateadapter;
            // SetContentView(Resource.Layout.PrivateMessage);

            // input.EditorAction +=
            //   delegate
            //   {
            //       inputManager.HideSoftInputFromWindow(input.WindowToken, HideSoftInputFlags.None);

            //       if (string.IsNullOrEmpty(input.Text))
            //           return;


            //       chatHubProxy.Invoke<List<PrivateChatMessage>>("SendPrivateMessage", toID, input.Text, "Click");
            //       //client.Send(input.Text);

            //       input.Text = "";
            //   };
        }

        public async Task getPrivateMessages(string toID, string toEmail)
        {
            // myPrivateMessages = await chatHubProxy.Invoke<List<PrivateChatMessage>>("GetPrivateMessage", toID, toEmail, 10);

            // for (int i = 0; i < myPrivateMessages.Count; i++)
            //     {
            //         privateadapter.Add(myPrivateMessages[i].message);
            //     }
            //privateadapter.NotifyDataSetChanged();

        }

        // Add User
        public void AddUser(IHubProxy chatHub, string id, string name, string email)
        {
            if (email == Email && myconnectionID == id && UserName == name)
            {
                //mySelf.ConnectionID = id;
                //mySelf.UserName = name;
                //mySelf.EmailID = email;
                RunOnUiThread(() => Toast.MakeText(this, "Welcome " + name, ToastLength.Long).Show());
            }
            else
            {
                myListItems.Add(name);
                //adapter.Clear();
                //foreach (var item in myListItems)
                //{
                //    adapter.Insert(item, adapter.Count);
                //}
                //adapter.NotifyDataSetChanged();
                //adapter.NotifyDataSetChanged();
                //RunOnUiThread(() => adapter.NotifyDataSetChanged());
                //adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myListItems);
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
                    Bundle bundle = new Bundle();
                    bundle.PutString("toUserName", myUsers[i].UserName);
                    bundle.PutString("toEmail", myUsers[i].EmailID);
                    bundle.PutString("toConnectionID", myUsers[i].ConnectionID);
                    bundle.PutString("UserName", UserName);
                    bundle.PutString("Email", Email);
                    bundle.PutString("ConnectionID", myconnectionID);

                    bundle.PutString("OTRINIT", "FALSE");
                    Intent n = new Intent(this, typeof(PrivateMessagingActivity));
                    n.PutExtra("bundle", bundle);
                    StartActivity(n);
                    Finish();

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
            //Send random value r and parameters.

            RLWEPrivateKey pri = new RLWEPrivateKey(M, ntt512Test.Convert32To8(priR2));
            RLWEPublicKey pub = new RLWEPublicKey(M, ntt512Test.Convert32To8(pubA), ntt512Test.Convert32To8(pubP));



            uint[] pubA2 = new uint[M];
            uint[] pubP2 = new uint[M];
            uint[] priR22 = new uint[M];
            ntt512Test.KeyGen(pubA2, pubP2, priR22, r);


            RLWEPrivateKey pri2 = new RLWEPrivateKey(M, ntt512Test.Convert32To8(priR22));
            RLWEPublicKey pub2 = new RLWEPublicKey(M, ntt512Test.Convert32To8(pubA2), ntt512Test.Convert32To8(pubP2));

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
            string test12 = Encoding.ASCII.GetString(toBytes2);
            string test22 = Encoding.ASCII.GetString(enc2);
            string test32 = Encoding.ASCII.GetString(dec2);
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
