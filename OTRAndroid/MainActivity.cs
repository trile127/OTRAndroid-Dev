using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

using System;
using System.Windows.Input;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Agreement.Kdf;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.X509;
using System.Diagnostics;
using Org.BouncyCastle.OpenSsl;

using Elliptic;
using Rebex.Security.Cryptography;



namespace OTRAndroid
{
    [Activity(Label = "Elliptic Key Cryptography", MainLauncher = true, Icon = "@mipmap/icon", ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : Activity
    {

        byte[] aliceRandomBytes;
        byte[] alicePrivateBytes;
        byte[] alicePublicBytes;

        byte[] bobRandomBytes;
        byte[] bobPrivateBytes;
        byte[] bobPublicBytes;

        byte[] aliceBobSharedBytes;
        byte[] bobAliceSharedBytes;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            const string Algorithm = "ECDH"; //What do you think about the other algorithms?
            const int KeyBitSize = 2056;
            const int NonceBitSize = 128;
            const int MacBitSize = 128;
            const int DefaultPrimeProbability = 30;

            //BEGIN SETUP ALICE
            IAsymmetricCipherKeyPairGenerator aliceKeyGen = GeneratorUtilities.GetKeyPairGenerator(Algorithm);
            DHParametersGenerator aliceGenerator = new DHParametersGenerator();
            aliceGenerator.Init(KeyBitSize, DefaultPrimeProbability, new SecureRandom());
            DHParameters aliceParameters = aliceGenerator.GenerateParameters();

            KeyGenerationParameters aliceKGP = new DHKeyGenerationParameters(new SecureRandom(), aliceParameters);
            aliceKeyGen.Init(aliceKGP);

            AsymmetricCipherKeyPair aliceKeyPair = aliceKeyGen.GenerateKeyPair();
            IBasicAgreement aliceKeyAgree = AgreementUtilities.GetBasicAgreement(Algorithm);
            aliceKeyAgree.Init(aliceKeyPair.Private);

            Button a1b = FindViewById<Button>(Resource.Id.a1b);
            TextView AliceRandomText = FindViewById<TextView>(Resource.Id.a1t);

            Button a2b = FindViewById<Button>(Resource.Id.a2b);
            TextView AlicePrivateKeyText = FindViewById<TextView>(Resource.Id.a2t);

            Button a3b = FindViewById<Button>(Resource.Id.a3b);
            TextView AlicePublicKeyText = FindViewById<TextView>(Resource.Id.a3t);

            Button b1b = FindViewById<Button>(Resource.Id.b1b);
            TextView BobRandomText = FindViewById<TextView>(Resource.Id.b1t);

            Button b2b = FindViewById<Button>(Resource.Id.b2b);
            TextView BobPrivateKeyText = FindViewById<TextView>(Resource.Id.b2t);

            Button b3b = FindViewById<Button>(Resource.Id.b3b);
            TextView BobPublicKeyText = FindViewById<TextView>(Resource.Id.b3t);

            Button a4b = FindViewById<Button>(Resource.Id.a4b);
            TextView AliceBobSharedKeyText = FindViewById<TextView>(Resource.Id.a4t);

            Button b4b = FindViewById<Button>(Resource.Id.b4b);
            TextView BobAliceSharedKeyText = FindViewById<TextView>(Resource.Id.b4t);


            //// what Alice does
            //byte[] aliceRandomBytes = new byte[32];
            //RNGCryptoServiceProvider.Create().GetBytes(aliceRandomBytes);

            //byte[] alicePrivate = Curve25519.(aliceRandomBytes);
            //byte[] alicePublic = Curve25519.GetPublicKey(alicePrivate);

            //// what Bob does
            //byte[] bobRandomBytes = new byte[32];
            //RNGCryptoServiceProvider.Create().GetBytes(bobRandomBytes);

            //byte[] bobPrivate = Curve25519.ClampPrivateKey(bobRandomBytes);
            //byte[] bobPublic = Curve25519.GetPublicKey(bobPrivate);

            //// what Alice does with Bob's public key
            //byte[] aliceShared = Curve25519.GetSharedSecret(alicePrivate, bobPublic);

            //// what Bob does with Alice' public key
            //byte[] bobShared = Curve25519.GetSharedSecret(bobPrivate, alicePublic);

            //// aliceShared == bobShared

            //a1b.Click += delegate {
            //    alicePrivateBytes = null;
            //    AlicePrivateKeyText.Text = "";

            //    alicePublicBytes = null;
            //    AlicePublicKeyText.Text = "";

            //    aliceBobSharedBytes = null;
            //    bobAliceSharedBytes = null;

            //    AliceBobSharedKeyText.Text = "";
            //    BobAliceSharedKeyText.Text = "";

            //    aliceRandomBytes = new byte[32];
            //    RNGCryptoServiceProvider.Create().GetBytes(aliceRandomBytes);
            //    AliceRandomText.Text = BitConverter.ToString(aliceRandomBytes).Replace("-", "");
            //};

            //a2b.Click += delegate {
            //    if (aliceRandomBytes != null)
            //    {
            //        alicePrivateBytes = Curve25519.Create(aliceRandomBytes);
            //        AlicePrivateKeyText.Text = BitConverter.ToString(alicePrivateBytes).Replace("-", "");
            //    }
            //};

            //a3b.Click += delegate {
            //    if (alicePrivateBytes != null)
            //    {
            //        alicePublicBytes = Curve25519.GetPublicKey(alicePrivateBytes);
            //        AlicePublicKeyText.Text = BitConverter.ToString(alicePublicBytes).Replace("-", "");
            //    }
            //};

            //b1b.Click += delegate {
            //    bobPrivateBytes = null;
            //    BobPrivateKeyText.Text = ""; // Reset

            //    bobPublicBytes = null;
            //    BobPublicKeyText.Text = ""; // Reset

            //    aliceBobSharedBytes = null;
            //    bobAliceSharedBytes = null;

            //    AliceBobSharedKeyText.Text = "";
            //    BobAliceSharedKeyText.Text = "";

            //    bobRandomBytes = new byte[32];
            //    RNGCryptoServiceProvider.Create().GetBytes(bobRandomBytes);
            //    BobRandomText.Text = BitConverter.ToString(bobRandomBytes).Replace("-", "");
            //};


            //b2b.Click += delegate {
            //    if (bobRandomBytes != null)
            //    {
            //        bobPrivateBytes = Curve25519.ClampPrivateKey(bobRandomBytes);
            //        BobPrivateKeyText.Text = BitConverter.ToString(bobPrivateBytes).Replace("-", "");
            //    }
            //};

            //b3b.Click += delegate {
            //    if (bobPrivateBytes != null)
            //    {
            //        bobPublicBytes = Curve25519.GetPublicKey(bobPrivateBytes);
            //        BobPublicKeyText.Text = BitConverter.ToString(bobPublicBytes).Replace("-", "");
            //    }
            //};

            //a4b.Click += delegate {
            //    if ((alicePrivateBytes != null) && (bobPublicBytes != null))
            //    {
            //        aliceBobSharedBytes = Curve25519.GetSharedSecret(alicePrivateBytes, bobPublicBytes);
            //        AliceBobSharedKeyText.Text = BitConverter.ToString(aliceBobSharedBytes).Replace("-", "");
            //    }

            //};


            //b4b.Click += delegate {
            //    if ((bobPrivateBytes != null) && (alicePublicBytes != null))
            //    {
            //        bobAliceSharedBytes = Curve25519.GetSharedSecret(bobPrivateBytes, alicePublicBytes);
            //        BobAliceSharedKeyText.Text = BitConverter.ToString(bobAliceSharedBytes).Replace("-", "");
            //    }
            //};


        }
    }
}
