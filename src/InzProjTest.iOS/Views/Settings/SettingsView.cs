using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cirrious.FluentLayouts.Touch;
using Foundation;
using MvvmCross.Plugin.Sidebar;
using InzProjTest.Core.ViewModels.Settings;
using UIKit;

namespace InzProjTest.iOS.Views.Settings
{
    [MvxSidebarPresentation(MvxPanelEnum.Center, MvxPanelHintType.ResetRoot, false)]
    public class SettingsView : BaseViewController<SettingsViewModel>
    {
        private UILabel _labelMessage;

        protected override void CreateView()
        {
            _labelMessage = new UILabel
            {
                Text = "Settings",
                TextAlignment = UITextAlignment.Center
            };
            Add(_labelMessage);
        }

        protected override void LayoutView()
        {
            View.AddConstraints(new FluentLayout[]
            {
                _labelMessage.WithSameCenterX(View),
                _labelMessage.WithSameCenterY(View)
            });
        }
    }
}
