using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Plugin.SimpleAudioPlayer;
using Xamarin.Forms;

namespace ASolute_Mobile.testing
{
    public partial class beepSound : ContentPage
    {
        private ISimpleAudioPlayer _simpleAudioPlayer;

        public beepSound()
        {
            InitializeComponent();

            _simpleAudioPlayer = CrossSimpleAudioPlayer.CreateSimpleAudioPlayer();
            //Stream beepStream = GetType().Assembly.GetManifestResourceStream("errorSound.mp3");
            var beepStream = GetStreamFromFile("errorSound.mp3");
            bool isSuccess = _simpleAudioPlayer.Load(beepStream);
        }

		private void Button_OnClicked(object sender, EventArgs e)
		{
			_simpleAudioPlayer.Play();
		}

		Stream GetStreamFromFile(string filename)
		{
            var fileName = "Haulage.Droid.test.mp3";

            var assembly = typeof(beepSound).GetTypeInfo().Assembly;

			//var assembly = IntrospectionExtensions.GetTypeInfo(typeof()).Assembly;

			var stream = assembly.GetManifestResourceStream(fileName);

			return stream;
		}
	}
}
