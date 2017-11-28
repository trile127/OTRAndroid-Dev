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

using System.IO;

using System.ServiceModel;

using System.Threading.Tasks;

using System.Runtime.Serialization.Json;


namespace OTRAndroidClient
{
    [Activity(Label = "LoginActivity")]
    public class LoginActivity : Activity
    {

        //public static readonly EndpointAddress EndPoint = new EndpointAddress("http://192.168.1.129:9608/MentorJService.svc");
        EditText txtEmail;
        EditText txtUserName;
        Button btnCreate;
        Button btnSign;
        Button btnforgotPW;
        public static String userSessionPref = "userPrefs";
        public static String User_Name = "userName";
        public static String User_Email = "userEmail";
        public static String User_Password = "userPassword";
        ISharedPreferences session;
        String SESSION_NAME, SESSION_EMAIL, SESSION_PASS;
        public static long SESSION_USERID;
        string msg;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource  
            SetContentView(Resource.Layout.Login);
            Initialize();


            //checkCredentials();
           // session = GetSharedPreferences(userSessionPref, FileCreationMode.Private);


        }

        private void Initialize()
        {
            // Get our button from the layout resource,  
            // and attach an event to it  
            btnSign = FindViewById<Button>(Resource.Id.btnLogin);
            btnCreate = FindViewById<Button>(Resource.Id.btnRegister);
            txtEmail = FindViewById<EditText>(Resource.Id.editEmail);
            txtUserName = FindViewById<EditText>(Resource.Id.editUserName);
            btnforgotPW = FindViewById<Button>(Resource.Id.btnForgotPw);
            btnSign.Click += Btnsign_Click;
            btnCreate.Click += Btncreate_Click;
            btnforgotPW.Click += BtnforgotPW_Click;

        }




        private async Task delayTask()
        {
            await Task.Delay(500);
        }

        private void Btncreate_Click(object sender, EventArgs e)
        {
            //Intent n = new Intent(this, typeof(RegisterActivity));
            //StartActivity(n);
            //Finish();
        }

        private void BtnforgotPW_Click(object sender, EventArgs e)
        {

            //Intent n = new Intent(this, typeof(ForgotPwActivity));
            //StartActivity(n);
            //Finish();
        }

        private async void Btnsign_Click(object sender, EventArgs e)
        {
            Bundle bundle = new Bundle();
            bundle.PutString("UserName", txtUserName.Text.Trim());
            bundle.PutString("Email", txtEmail.Text.Trim());
            Intent n = new Intent(this, typeof(ChatActivity));
            n.PutExtra("bundle", bundle);
            StartActivity(n);
            Finish();
        }

        //public async void checkCredentials()
        //{
        //    ISharedPreferences preferences = GetSharedPreferences(userSessionPref, FileCreationMode.Private);
        //    String email = preferences.GetString("email", "");
        //    String username = preferences.GetString("username", "");
        //    Toast.MakeText(this, "Username: " + username + "\nEmail: " + email, ToastLength.Short).Show();
        //    String pass = preferences.GetString("pass", "");
        //    long userid = preferences.GetLong("userid", -1);
        //    if (!username.Equals("") && !email.Equals("") && !pass.Equals("") && userid != -1)
        //    {
        //        //Check with webserver HERE
        //        _client.ValidateLogin_UserInfoAsync(email, pass);

        //        //Figure out a better way to wait and break out
        //        while (msg == null || msg != "Login Successful!")
        //        {
        //            await delayTask();
        //            if (msg != null && msg != "Login Successful!")
        //            {
        //                break;  //Error
        //            }
        //        }

        //        if (msg == "Login Successful!")
        //        {
        //            //Set user preferences
        //            msg = null;
        //            RunOnUiThread(() => Toast.MakeText(this, "Successful Login!!,", ToastLength.Short).Show());

        //            Bundle bundle = new Bundle();
        //            //Create a stream to serialize the object to.  
        //            MemoryStream ms = new MemoryStream();

        //            // Serializer the User object to the stream.  
        //            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(tblUserInfo));
        //            ser.WriteObject(ms, user);
        //            byte[] json = ms.ToArray();
        //            ms.Close();
        //            string userInfoString = Encoding.UTF8.GetString(json, 0, json.Length);

        //            bundle.PutString("UserInfo", userInfoString);
        //            Intent n = new Intent(this, typeof(MainInterfaceActivity));
        //            n.PutExtra("bundle", bundle);
        //            StartActivity(n);
        //            Finish();
        //        }
        //        else
        //        {
        //            msg = null;
        //            Toast.MakeText(this, "Login Failed", ToastLength.Long).Show();  //Add error message on UI code saying why it failed. IE: "Username or password incorrect"
        //        }
        //    }

        }

       
 }
