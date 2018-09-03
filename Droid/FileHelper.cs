using System;
using System.IO;
using ASolute_Mobile.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileHelper))]
namespace ASolute_Mobile.Droid
{
    public class FileHelper : IFileHelper
    {
		public string GetLocalFilePath(string filename)
		{
			string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			return Path.Combine(path, filename);
		}
    }
}
