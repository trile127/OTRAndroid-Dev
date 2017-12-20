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
using System.Timers;

namespace OTRAndroidClient
{
    public class DiffieHellman
    {

        const string Algorithm = "ECDH"; //What do you think about the other algorithms?
        const int KeyBitSize = 256;
        const int NonceBitSize = 128;
        const int MacBitSize = 128;
        const int DefaultPrimeProbability = 30;

        public static int milliseconds = 1, secs = 0, mins = 0;
        public static string TestMethod()
        {
            string time_elapsed = "";

            Timer timer = new Timer();
            timer.Interval = 1; // 1 milliseconds  
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

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

            //END SETUP ALICE

            /////AT THIS POINT, Alice's Public Key, Alice's Parameter P and Alice's Parameter G are sent unsecure to BOB

            //BEGIN SETUP BOB
            IAsymmetricCipherKeyPairGenerator bobKeyGen = GeneratorUtilities.GetKeyPairGenerator(Algorithm);
            DHParameters bobParameters = new DHParameters(aliceParameters.P, aliceParameters.G);

            KeyGenerationParameters bobKGP = new DHKeyGenerationParameters(new SecureRandom(), bobParameters);
            bobKeyGen.Init(bobKGP);

            AsymmetricCipherKeyPair bobKeyPair = bobKeyGen.GenerateKeyPair();
            IBasicAgreement bobKeyAgree = AgreementUtilities.GetBasicAgreement(Algorithm);
            bobKeyAgree.Init(bobKeyPair.Private);
            //END SETUP BOB

            BigInteger aliceAgree = aliceKeyAgree.CalculateAgreement(bobKeyPair.Public);
            BigInteger bobAgree = bobKeyAgree.CalculateAgreement(aliceKeyPair.Public);

            timer.Stop();
            timer.Dispose();
            time_elapsed += ("\nTotal DHKE Setup Time: " + String.Format("{0}:{1:00}:{2:000}", mins, secs, milliseconds));
            resetTime();


            time_elapsed += ("\nBefore RE-key DHKE" + getDeviceMemoryUsage());
            timer = new Timer();
            timer.Interval = 1; // 1 milliseconds  
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            bobKeyPair = bobKeyGen.GenerateKeyPair();
            aliceKeyPair = aliceKeyGen.GenerateKeyPair();
            aliceKeyAgree.Init(aliceKeyPair.Private);
            bobKeyAgree.Init(bobKeyPair.Private);
            aliceAgree = aliceKeyAgree.CalculateAgreement(bobKeyPair.Public);
            bobAgree = bobKeyAgree.CalculateAgreement(aliceKeyPair.Public);
            timer.Stop();
            timer.Dispose();
            time_elapsed += ("\nAfter RE-key DHKE" + getDeviceMemoryUsage());
            time_elapsed += ("\nDHKE Re-key Time: " + String.Format("{0}:{1:00}:{2:000}", mins, secs, milliseconds));
            resetTime();

            return time_elapsed;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            milliseconds++;
            if (milliseconds >= 1000)
            {
                secs++;
                milliseconds = 0;
            }
            if (secs == 59)
            {
                mins++;
                secs = 0;
            }
        }

        private static void resetTime()
        {
            milliseconds = 1;
            secs = 0;
            mins = 0;
        }
            public static void TestRSA()
        {
            //lets take a new CSP with a new 2048 bit rsa key pair
            var csp = new RSACryptoServiceProvider(2048);

            //how to get the private key
            var privKey = csp.ExportParameters(true);

            //and the public key ...
            var pubKey = csp.ExportParameters(false);

            


        }

        public static string getDeviceMemoryUsage()
        {
            //DeviceInformation devInfo = new DeviceInformation();
            ////* Gets the main memory (RAM) information.
            //var activityManager = (ActivityManager)Forms.Context.GetSystemService(Context.ActivityService);

            //ActivityManager.MemoryInfo memInfo = new ActivityManager.MemoryInfo();
            //activityManager.GetMemoryInfo(memInfo);
            string message = "";
            long freeSize;
            long totalSize;
            long usedSize;
            try
            {
                Java.Lang.Runtime info = Java.Lang.Runtime.GetRuntime();
                freeSize = info.FreeMemory();
                totalSize = info.TotalMemory();
                usedSize = totalSize - freeSize;
                //MessageText.Append("\nGetDeviceInfo - Avail {0} - {1} MB", (int)freeSize, (int)freeSize / 1024 / 1024);
                message += ("\nGetDeviceInfo - Used: " + (int)((usedSize & 0xFFFFFFFF)) + " B");
                message += ("\nGetDeviceInfo - Available: " + (int)((freeSize & 0xFFFFFFFF)) + " B");
                //MessageText.Append("\nGetDeviceInfo - Total {0} - {1} MB", (int)totalSize, (int)totalSize / 1024 / 1024);
            }
            catch (Java.Lang.Exception e)
            {
                e.PrintStackTrace();
            }
            return message;
            //MessageText.Append("\nGetDeviceInfo - Avail {0} - {1} MB", memInfo.AvailMem, memInfo.AvailMem / 1024 / 1024);
            //MessageText.Append("\nGetDeviceInfo - Low {0}", memInfo.LowMemory);
            //MessageText.Append("\nGetDeviceInfo - Total {0} - {1} MB", memInfo.TotalMem, memInfo.TotalMem / 1024 / 1024);
            //// System.Diagnostics.Debug.WriteLine ("GetDeviceInfo - Avail {0} - {1} MB", memInfo.AvailMem, memInfo.AvailMem / 1024 / 1024);
            //// System.Diagnostics.Debug.WriteLine ("GetDeviceInfo - Low {0}", memInfo.LowMemory);
            //// System.Diagnostics.Debug.WriteLine ("GetDeviceInfo - Total {0} - {1} MB", memInfo.TotalMem, memInfo.TotalMem / 1024 / 1024);

        }


    }

}
      