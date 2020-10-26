using Foundation;
using MvvmCross.Platforms.Ios.Core;
using InzProjTest.Core;

namespace InzProjTest.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : MvxApplicationDelegate<Setup, App>
    {
    }
}
