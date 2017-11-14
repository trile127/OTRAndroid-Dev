using Android.App;
using Android.Widget;
using Android.OS;
using Microsoft.AspNet.SignalR.Client;
using System;
using Android.Content.Res;
using Android.Views;

namespace SignalR_Client
{
    [Activity(Label = "SignalR_Client", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public string UserName { get; set; }
        public int BackgroungColor { get; set; }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            GetInfo getinfo = new GetInfo();
            getinfo.OnGetInfoComplete += Getinfo_OnGetInfoComplete;
            getinfo.Show(FragmentManager, "GetInfo");
        }

        private async void Getinfo_OnGetInfoComplete(object sender, GetInfo.OnGetInfoCompleteEventArgs e)
        {
            UserName = e.TxtName;
            BackgroungColor = e.BackgroundColor;

            var hubConnection = new HubConnection("http://otrxamarinchat.azurewebsites.net/Home/Chat");
            var chatHubProxy = hubConnection.CreateHubProxy("ChatHub");

            chatHubProxy.On<string, int, string>("UpdateChatMessage", (message, color, user) =>
            {
                //UpdateChatMessage has been called from Sever

                RunOnUiThread(() =>
                {
                    TextView txt = new TextView(this);
                    txt.Text = user + ": " + message;
                    txt.SetTextSize(Android.Util.ComplexUnitType.Sp, 20);
                    txt.SetPadding(10, 10, 10, 10);

                    switch (color)
                    {
                        case 1:
                            txt.SetTextColor(Android.Graphics.Color.Red);
                            break;
                        case 2:
                            txt.SetTextColor(Android.Graphics.Color.MediumSeaGreen);
                            break;
                        case 3:
                            txt.SetTextColor(Android.Graphics.Color.Blue);
                            break;
                        default:
                            txt.SetTextColor(Android.Graphics.Color.Black);
                            break;
                    }

                    txt.LayoutParameters = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        TopMargin = 10,
                        BottomMargin = 10,
                        LeftMargin = 10,
                        RightMargin = 10,
                        Gravity = GravityFlags.Right
                    };

                    FindViewById<LinearLayout>(Resource.Id.llChatMessage).AddView(txt);

                });
            });


            try
            {
                await hubConnection.Start();
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
            }

            FindViewById<Button>(Resource.Id.btnSend).Click += async (o, e2) =>
            {
                var message = FindViewById<EditText>(Resource.Id.txtChat).Text;
                await chatHubProxy.Invoke("SendMessageColor", new object[] { message, BackgroungColor, UserName });
            };
        }
    }
}

