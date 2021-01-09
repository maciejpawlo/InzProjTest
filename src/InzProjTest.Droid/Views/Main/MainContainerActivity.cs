using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Acr.UserDialogs;
using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using MvvmCross.Platforms.Android.Views.AppCompat;
using InzProjTest.Core.ViewModels.Main;
using InzProjTest.Droid.Views.Helpers;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Google.Android.Material.TextField;

namespace InzProjTest.Droid.Views.Main
{
    [Activity(
        Theme = "@style/AppTheme", ScreenOrientation = ScreenOrientation.Portrait,
        WindowSoftInputMode = SoftInput.AdjustResize | SoftInput.StateHidden)]
    public class MainContainerActivity : BaseActivity<MainContainerViewModel>, IDrawerActivity
    {
        protected override int ActivityLayoutId => Resource.Layout.activity_main_container;

        public DrawerLayout DrawerLayout { get; private set; }
        protected MvxActionBarDrawerToggle DrawerToggle { get; private set; }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            DrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            SetupDrawerLayout();
            DrawerToggle.SyncState();
            
            UserDialogs.Init(this); //acr dialgos init
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[] {Manifest.Permission.RecordAudio}, 1);
            }
            Xamarin.Essentials.Platform.Init(this, bundle);
            if (bundle == null)
                ViewModel.ShowMenuCommand.Execute();
        }

        private void SetupDrawerLayout()
        {
            SupportActionBar?.SetDisplayHomeAsUpEnabled(true);

            DrawerToggle = new MvxActionBarDrawerToggle(
                this,                           // host Activity
                DrawerLayout,                   // DrawerLayout object
                Toolbar,                        // nav drawer icon to replace 'Up' caret
                Resource.String.drawer_open,    // "open drawer" description
                Resource.String.drawer_close    // "close drawer" description
            );

            DrawerLayout.AddDrawerListener(DrawerToggle);
        }

        protected override void OnResume()
        {
            if (DrawerToggle != null)
            {
                DrawerToggle.DrawerOpened += HideSoftKeyboard;
            }

            base.OnResume();
        }

        protected override void OnPause()
        {
            if (DrawerToggle != null)
            {
                DrawerToggle.DrawerOpened -= HideSoftKeyboard;
            }

            base.OnPause();
        }

        public override void OnBackPressed()
        {
            if (DrawerLayout != null && DrawerLayout.IsDrawerOpen(GravityCompat.Start))
                DrawerLayout.CloseDrawers();
            else
                base.OnBackPressed();
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            DrawerToggle.OnConfigurationChanged(newConfig);
        }

        public void HideSoftKeyboard() => HideSoftKeyboard(this, EventArgs.Empty);

        private void HideSoftKeyboard(object sender, EventArgs args)
        {
            if (CurrentFocus != null)
            {
                var inputMethodManager = (InputMethodManager)GetSystemService(InputMethodService);
                inputMethodManager.HideSoftInputFromWindow(CurrentFocus.WindowToken, 0);
                CurrentFocus.ClearFocus();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
