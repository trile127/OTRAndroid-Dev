using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System;
using System.Threading.Tasks;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.RLWE;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Asymmetric.Encrypt.RLWE.Arithmetic;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Symmetric.Block;
using VTDev.Libraries.CEXEngine.Crypto.Cipher.Symmetric;
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
using System.Timers;
using System.Text;

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

            //Intent n = new Intent(this, typeof(LoginActivity));
            //StartActivity(n);
            //Finish();
            //_loginButton = FindViewById<Button>(Resource.Id.loginButton);
            //_loginButton.Click += LoginButtonOnClick;


            using (ICipherMode cipher = new CTR(BlockCiphers.RHX))
            {
                // initialize for encryption
                string keyStr = "abcdefghijklmnopqrstuvwxyz123456";
                byte[] keyBytes = Encoding.ASCII.GetBytes(keyStr);
                byte[] messageBytes = Encoding.ASCII.GetBytes("Message 1");
                byte[] enc = new byte[2000];
                cipher.Initialize(true, new KeyParams(keyBytes));
                // encrypt a block
                cipher.Transform(messageBytes, 0, enc, 0);

            }



            Intent n = new Intent(this, typeof(DemoActivity));
            StartActivity(n);
            Finish();
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

