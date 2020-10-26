using Acr.UserDialogs;
using MvvmCross.IoC;
using MvvmCross.ViewModels;
using InzProjTest.Core.ViewModels.Main;

namespace InzProjTest.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            CreatableTypes()
                .EndingWith("Service")
                .AsInterfaces()
                .RegisterAsLazySingleton();
            MvvmCross.Mvx.IoCProvider.RegisterSingleton<IUserDialogs>(()=>UserDialogs.Instance);
            RegisterAppStart<MainViewModel>();
        }
    }
}
