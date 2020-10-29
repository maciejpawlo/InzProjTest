using MvvmCross.Platforms.Android.Core;
using InzProjTest.Core;
using InzProjTest.Core.Interfaces;
using InzProjTest.Droid.Services;
using MvvmCross;

namespace InzProjTest.Droid
{
    public class Setup : MvxAndroidSetup<App>
    {
        protected override void InitializeFirstChance()
        {
            Mvx.IoCProvider.RegisterSingleton<ILocalFileHelper>(new FileHelper());
            base.InitializeFirstChance();
        }
    }
}
