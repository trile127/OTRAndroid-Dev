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
    [Activity(Label = "PrivateMessaging")]
    public class PrivateMessagingActivity : Activity
    {

        String UserName;
        String Email;
        int userID;
        String myconnectionID;
        String toconnectionID;
        HubConnection hub;
        IHubProxy proxy;

        List<ChatUserDetail> myUsers;
        List<ChatMessageDetail> myMessages;
        List<String> messageList;
        List<PrivateChatMessage> myPrivateMessages;

        EditText input;
        ListView messages;
        TextView privChatUser;
        InputMethodManager inputManager;
        ArrayAdapter privateadapter;
        List<string> myListItems;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.PrivateMessage);

            //Private Chat

            input = FindViewById<EditText>(Resource.Id.Input);
            messages = FindViewById<ListView>(Resource.Id.PrivateMessages);
            privChatUser = FindViewById<TextView>(Resource.Id.PrivateChatUser);
            myListItems = new List<string>();
            privateadapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItem2, myListItems);

            Bundle bundler = Intent.GetBundleExtra("bundle");
            UserName = bundler.GetString("UserName");
            Email = bundler.GetString("Email");
            toconnectionID = bundler.GetString("ConnectionID");
            hub = ChatActivity.hubConnection;
            proxy = ChatActivity.chatHubProxy;
            // Create your application here
            Connect();
            inputManager = (InputMethodManager)GetSystemService(InputMethodService);
            privChatUser.Text = UserName;
            messages.Adapter = privateadapter;
            getPrivateMessages(toconnectionID, Email);

            input.EditorAction +=
              delegate
              {
                  inputManager.HideSoftInputFromWindow(input.WindowToken, HideSoftInputFlags.None);

                  if (string.IsNullOrEmpty(input.Text))
                      return;


                  proxy.Invoke<List<PrivateChatMessage>>("SendPrivateMessage", toconnectionID, input.Text, "Click");
                  //client.Send(input.Text);

                  input.Text = "";
              };

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

               

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

        }

        public async Task getPrivateMessages(string toID, string toEmail)
        {
            myPrivateMessages = await proxy.Invoke<List<PrivateChatMessage>>("GetPrivateMessage", toID, toEmail, 10);

            for (int i = 0; i < myPrivateMessages.Count; i++)
            {
                privateadapter.Add(myPrivateMessages[i].message);
            }
            myListItems.Add("TEst GetPrivateMEssage Update");
            RunOnUiThread(() => privateadapter.NotifyDataSetChanged());

        }
    }
}