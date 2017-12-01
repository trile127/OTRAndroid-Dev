using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.AspNet.SignalR.Client;
using Android;
using SignalRChat;
using System.Threading.Tasks;

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


            Bundle bundler = Intent.GetBundleExtra("bundle");
            UserName = bundler.GetString("UserName");
            Email = bundler.GetString("Email");

            // Connect to the server
            var hubConnection = new HubConnection("http://signalrchat.azurewebsites.net/");

            // Create a proxy to the 'ChatHub' SignalR Hub
            var chatHubProxy = hubConnection.CreateHubProxy("ChatHub");

            chatHubProxy.On<int, string, List<ChatUserDetail>, List<ChatMessageDetail>>("onConnected", (userID, UserName, allUsers, messages) =>
            {
                Email = bundler.GetString("Email");
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



            //Start the connection
            hubConnection.Start();

            // Invoke the 'UpdateNick' method on the server
            chatHubProxy.Invoke("Connect", UserName, Email);

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

            for (i = 0; i < allUsers.length; i++)
            {

                if (myListItems[e.Position] == allUsers[i].UserName)
                {

                }

            }
            if (myListItems[e.Position] == )
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
