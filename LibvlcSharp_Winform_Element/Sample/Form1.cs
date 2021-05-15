using LibVLCSharp.Shared;
using System;
using System.Windows.Forms;
namespace Sample
{
    public partial class Form1 : Form
    {
        private void Form1_Shown(object sender, EventArgs e)
        {
            Core.Initialize();
            var _libvlc = new LibVLC();
            var _url = new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4");
            var _media = new Media(_libvlc, _url);
            controller1.VideoView.MediaPlayer= new MediaPlayer(_media);
            controller1.setEvents();
            controller1.VideoView.MediaPlayer.Play();
        }
        public Form1() => InitializeComponent();
    }
}
