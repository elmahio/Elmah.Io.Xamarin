using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Elmah.Io.Xamarin;
using System;

namespace ElmahIo.Samples.XamarinAndroid
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        TextView textMessage;

        // Examples of how to log breadcrumbs as part of actions like back button pressed or setting the app on pause

        public override void OnBackPressed()
        {
            ElmahIoXamarin.AddBreadcrumb("OnBackPressed", DateTime.UtcNow, action: "Navigation");
            base.OnBackPressed();
        }

        protected override void OnPause()
        {
            ElmahIoXamarin.AddBreadcrumb("OnPause", DateTime.UtcNow);
            base.OnPause();
        }

        // End of examples

        protected override void OnCreate(Bundle savedInstanceState)
        {
            ElmahIoXamarin.Init(new ElmahIoXamarinOptions
            {
                ApiKey = "API_KEY",
                LogId = new Guid("LOG_ID"),
            });

            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            textMessage = FindViewById<TextView>(Resource.Id.message);
            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
        public bool OnNavigationItemSelected(IMenuItem item)
        {
            // More examples of logging breadcrumbs to elmah.io when clicking in the app

            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    ElmahIoXamarin.AddBreadcrumb("Navigate to Home", DateTime.UtcNow, action: "Navigation");
                    textMessage.SetText(Resource.String.title_home);
                    return true;
                case Resource.Id.navigation_dashboard:
                    ElmahIoXamarin.AddBreadcrumb("Navigate to Dashboard", DateTime.UtcNow, action: "Navigation");
                    throw new ApplicationException("We who are about to die salute you!");
                case Resource.Id.navigation_notifications:
                    ElmahIoXamarin.AddBreadcrumb("Navigate to Notifications", DateTime.UtcNow, action: "Navigation");
                    textMessage.SetText(Resource.String.title_notifications);
                    return true;
            }
            return false;
        }
    }
}

