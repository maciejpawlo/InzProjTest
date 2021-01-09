using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using InzProjTest.Core.Interfaces;
using Encoding = Android.Media.Encoding;
using Environment = Android.OS.Environment;

namespace InzProjTest.Droid.Services
{
    public class FileHelper : ILocalFileHelper
    {
        public string GetPath(string filename)
        {
            var directory = Application.Context.GetExternalFilesDir(null).AbsolutePath;
            var file = Path.Combine(directory, filename);
            return file;
        }

        public string GetPatientPath(string filename, string firstName, string lastName)
        {
            var directory = Application.Context.GetExternalFilesDir(null).AbsolutePath;
            directory = directory + $"/{firstName}{lastName}";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            var file = Path.Combine(directory, filename);
            return file;
        } 
    }
}
