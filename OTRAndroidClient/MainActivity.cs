using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System;

namespace OTRAndroidClient
{
    [Activity(Label = "OTRAndroidClient", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private Button _loginButton;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);


            _loginButton = FindViewById<Button>(Resource.Id.loginButton);
            _loginButton.Click += LoginButtonOnClick;

        }

        private void LoginButtonOnClick(object sender, EventArgs eventArgs)
        {
            //Intent n = new Intent(this, typeof(LoginActivity));
            //StartActivity(n);
            //Finish();
            Intent n = new Intent(this, typeof(LoginActivity));
            StartActivity(n);
            Finish();
        }
    }
}

