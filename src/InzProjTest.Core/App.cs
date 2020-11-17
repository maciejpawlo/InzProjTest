using Acr.UserDialogs;
using InzProjTest.Core.Interfaces;
using InzProjTest.Core.Services;
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
            MvvmCross.Mvx.IoCProvider.RegisterType<ISignalAnalyzer, SignalAnalyzer>();
            RegisterAppStart<MainViewModel>();
        }
    }
}
