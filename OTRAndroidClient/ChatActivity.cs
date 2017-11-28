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

namespace OTRAndroidClient
{
    [Activity(Label = "ChatActivity")]
    public class ChatActivity : Activity
    {
        String userName;
        String email;
        List<string> myListItems;
        ListView myListView;

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

            //List<Player> players = Player.GetPlayers();
            //this.ListAdapter = new ButtonAdapter(this, players);
        }

        void MyListView_ItemClick (object sender, AdapterView.ItemClickEventArgs e)
        {
            //if (myListItems [e.Position])
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
