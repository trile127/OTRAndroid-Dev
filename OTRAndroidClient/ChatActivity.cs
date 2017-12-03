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

using VTDev.Libraries.CEXEngine.Crypto.Common;
using VTDev.Libraries.CEXEngine.Crypto.Enumeration;
using VTDev.Libraries.CEXEngine.Crypto.Prng;
using VTDev.Libraries.CEXEngine.CryptoException;

using System.IO;
using VTDev.Libraries.CEXEngine.Utility;

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
        String UserName;
        String Email;
        int userID;
        List<string> myListItems;
        ListView myListView;
        List<ChatUserDetail> allUsers;
        List<ChatMessageDetail> messages;

        // Connect to the server
        HubConnection hubConnection;

        // Create a proxy to the 'ChatHub' SignalR Hub
        IHubProxy chatHubProxy;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Chat);
            myListView = FindViewById<ListView>(Resource.Id.myListView);

            myListItems = new List<string>();
            myListItems.Add("Sydney");
            myListItems.Add("Melbourne");

            ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, myListItems);
            myListView.Adapter = adapter;

            myListView.ItemClick += MyListView_ItemClick;

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
                await hubConnection.Start();
                chatHubProxy.On<int, string, List<ChatUserDetail>, List<ChatMessageDetail>>("onConnected", (userID, UserName, allUsers, messages) =>
                {

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

                if (hubConnection.State == ConnectionState.Connected)
                {
                    await chatHubProxy.Invoke("Connect", UserName, Email);
                }

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

           
           
        }



        // Add User
        public void AddUser(IHubProxy chatHub, string id, string name, string email)
        {
            myListItems.Add(name);

            // var userId = $('#hdId').val();
            // var userEmail = $('#hdEmailID').val();
            // var code = "";

            // if (userEmail == email && $('.loginUser').length == 0) {
            //     code = $('<div class="loginUser">' + name + "</div>");
            // }
            // else {
            //     code = $('<a id="' + id + '" class="user" >' + name + '<a>');
            //     $(code).click(function () {
            //         var id = $(this).attr('id');
            //         if (userEmail != email) {
            //             OpenPrivateChatWindow(chatHub, id, name, userEmail, email);
            //         }
            //     });
            // }

            // $("#divusers").append(code);
        }

        // Add Message
        public void AddMessage(string userName, string message)
        {
            // $('#divChatWindow').append('<div class="message"><span class="userName">' + userName + '</span>: ' + message + '</div>');

            // var height = $('#divChatWindow')[0].scrollHeight;
            // $('#divChatWindow').scrollTop(height);
        }


        // void registerClientMethods(var chatHub) {
        //     // Calls when user successfully logged in
        //     chatHub.client.onConnected = function (id, userName, allUsers, messages) {
        //         setScreen(true);

        //         $('#hdId').val(id);
        //         $('#hdUserName').val(userName);
        //         $('#spanUser').html(userName);

        //         // Add All Users
        //         for (i = 0; i < allUsers.length; i++) {
        //             AddUser(chatHub, allUsers[i].ConnectionId, allUsers[i].UserName, allUsers[i].EmailID);
        //         }

        //         // Add Existing Messages
        //         for (i = 0; i < messages.length; i++) {
        //             AddMessage(messages[i].UserName, messages[i].Message);
        //         }

        //         $('.login').css('display', 'none');
        //     }

        public void MyListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {

            for (int i = 0; i < allUsers.Count; i++)
            {

                if (myListItems[e.Position] == allUsers[i].UserName)
                {

                }

            }
            SetContentView(Resource.Layout.Chat);


        }
        //protected override void OnListItemClick(ListView l, View v, int position, long id)
        //{
        //    base.OnListItemClick(l, v, position, id);

        //    Player player = ((ButtonAdapter)this.ListAdapter)[position];
        //    string text = string.Format("{0} Item Click!", player.Name);
        //    Toast.MakeText(this, text, ToastLength.Short).Show();
        //}
        //protected override void OnCreate(Bundle savedInstanceState)
        //{
        //    base.OnCreate(savedInstanceState);
        //    SetContentView(Resource.Layout.Users);
        //    var contactsAdapter = new ContactsAdapter(this);
        //    var contactsListView = FindViewById<ListView>(Resource.Id.ContactsListView);
        //    contactsListView.Adapter = contactsAdapter;


        //    Bundle bundler = Intent.GetBundleExtra("bundle");
        //    userName = bundler.GetString("UserName");
        //    email = bundler.GetString("Email");

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
            myListItems.Add(test1);
            myListItems.Add(test2);
            myListItems.Add(test3);




            //int intValue = 512;
            //byte[] intBytes = BitConverter.GetBytes(intValue);
            //if (BitConverter.IsLittleEndian)
            //    Array.Reverse(intBytes);
            //byte[] result = intBytes;


   
        }





        //var messageListAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem1, new List<string>());
        //    var messageList = FindViewById<ListView>(Resource.Id.Messages);
        //    messageList.Adapter = messageListAdapter;

        //    var hubConnection = new HubConnection("http://signalrchat.azurewebsites.net/");
        //    var chatHubProxy = hubConnection.CreateHubProxy("ChatHub");





        //    var connection = new Connection("http://signalrchat.azurewebsites.net/");
        //    connection.Received += data =>
        //        RunOnUiThread(() => messageListAdapter.Add(data));

        //    var sendMessage = FindViewById<Button>(Resource.Id.SendMessage);
        //    var message = FindViewById<TextView>(Resource.Id.Message);

        //    sendMessage.Click += delegate
        //    {
        //        if (!string.IsNullOrWhiteSpace(message.Text) && connection.State == ConnectionState.Connected)
        //        {
        //            connection.Send("Android: " + message.Text);

        //            RunOnUiThread(() => message.Text = "");
        //        }
        //    };

        //    connection.Start().ContinueWith(task => connection.Send("Android: connected"));
        //}


        //// Add User
        //function AddUser(chatHub, id, name, email)
        //{
        //    var userId = $('#hdId').val();
        //    var userEmail = $('#hdEmailID').val();
        //    var code = "";

        //    if (userEmail == email && $('.loginUser').length == 0) {
        //        code = $('<div class="loginUser">' + name + "</div>");
        //    }
        //    else {
        //        code = $('<a id="' + id + '" class="user" >' + name + '<a>');
        //        $(code).click(function() {
        //            var id = $(this).attr('id');
        //            if (userEmail != email)
        //            {
        //                OpenPrivateChatWindow(chatHub, id, name, userEmail, email);
        //            }
        //        });
        //    }

        //    $("#divusers").append(code);
        //}

    }
}
