using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using InzProjTest.Core.ViewModels.Main;
using InzProjTest.Core.ViewModels.Results;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Platforms.Android.Views.Fragments;
using OxyPlot.Xamarin.Android;

namespace InzProjTest.Droid.Views.Results
{
    [MvxFragmentPresentation(typeof(MainContainerViewModel), Resource.Id.content_frame, true)]
    public class ResultsFragment : BaseFragment<ResultsViewModel>
    {
        protected override int FragmentLayoutId => Resource.Layout.fragment_results;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = base.OnCreateView(inflater, container, savedInstanceState);
            var plot = view.FindViewById<PlotView>(Resource.Id.plot);
            plot.Model = ViewModel.PlotModel;
            return view;
        }
    }
}
