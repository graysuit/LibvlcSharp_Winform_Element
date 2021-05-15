using System;
using System.Windows.Forms;
using LibVLCSharp.Shared;
using LibVLCSharp.WinForms;
using System.Timers;
using System.Runtime.InteropServices;

namespace MediaElement
{
    public partial class Controller : UserControl
    {
        [DllImport("user32")]
        private static extern long ShowCursor(long bShow);

        Form fullScreenForm = new Form();
        bool IsFullScreen = false;
        System.Timers.Timer Timer1 = new System.Timers.Timer(1000);
        System.Timers.Timer Timer2 = new System.Timers.Timer(50);
        TimeSpan activityThreshold = TimeSpan.FromSeconds(3);
        bool cursorHidden = false;
        bool shouldHide;

        public Controller()
        {
            if (!DesignMode)
            {
                Core.Initialize();
                InitializeComponent();
            }
        }
        private void OnPlaying(object sender, EventArgs e)
        {
            if (VideoView.MediaPlayer.Media != null) VideoView.MediaPlayer.Media.Dispose(); // If video found then dispose it
            PlayBtn.Image = Properties.Resources.play;
            Timer1.Start();
            VideoView.MediaPlayer.EnableMouseInput = false;
            VideoView.MediaPlayer.EnableKeyInput = false;
            VideoView.MediaPlayer.AspectRatio = VideoView.Width + ":" + VideoView.Height;
            VideoSeekBar.Maximum = Convert.ToInt32(VideoView.MediaPlayer.Media.Duration);
            TimeSpan t = TimeSpan.FromMilliseconds(VideoView.MediaPlayer.Media.Duration);
            TotalTimeLabel.Text = string.Format("{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
        }

        private void OnEndReached(object sender, EventArgs e)
        {
            Timer1.Stop();
            Timer2.Stop();
            VideoView.MediaPlayer.AspectRatio = VideoView.Width + ":" + VideoView.Height;
            if (IsFullScreen)
            {
                ShowCursor(1);
                PanelBackControls.Visible = true;
                tableLayoutPanel1.RowCount = 2;
                fullScreenForm.Controls.Remove(tableLayoutPanel1);
                Controls.Add(tableLayoutPanel1);
                fullScreenForm.Hide();
                IsFullScreen = false;
                FulScrBtn.Image = Properties.Resources.full;
            }
        }

        public void setEvents()
        {
            VideoView.SizeChanged += new EventHandler(VideoView_SizeChanged);
            VideoView.Click += new EventHandler(VideoView_Click);
            VideoView.DoubleClick += new EventHandler(VideoView_DoubleClick);
            PlayBtn.Click += new EventHandler(Play_Button_Click);
            VideoSeekBar.MouseDown += new MouseEventHandler(VideoSeekBar_MouseDown);
            VideoSeekBar.MouseMove += new MouseEventHandler(VideoSeekBar_MouseMove);
            StopBtn.Click += new EventHandler(On_Stop_Button_Click);
            VolBar.Scroll += new EventHandler(On_VolumeBar_Scroll);
            VolBtn.Click += new EventHandler(On_Volume_Button_Click);
            FulScrBtn.Click += new EventHandler(On_FullScreen_Button_Click);
            VideoView.MediaPlayer.EndReached += OnEndReached;
            VideoView.MediaPlayer.Playing += OnPlaying;
            Timer1.Elapsed += new ElapsedEventHandler(CrntTimeTimer_Tick);
            Timer2.Elapsed += new ElapsedEventHandler(On_FullScreen_Timer_Ticked);
        }

        private void CrntTimeTimer_Tick(object sender, EventArgs e)
        {
            if (VideoView.MediaPlayer.Media != null & !((int)VideoView.MediaPlayer.Time > VideoSeekBar.Maximum) & !((int)VideoView.MediaPlayer.Time <= 0))
            {
                VideoSeekBar.Value = Convert.ToInt32(VideoView.MediaPlayer.Time);
            }
            var t = TimeSpan.FromMilliseconds(VideoView.MediaPlayer.Time);
            CrntTimeLabel.Text = string.Format("{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
        }



        private void On_FullScreen_Timer_Ticked(object sender, EventArgs e)
        {
            // This timer is for hiding controls and cursor 
            // After every 3 seconds when no mouse movement
            if (IsFullScreen & VideoView.MediaPlayer.Media != null)
            {
                shouldHide = User32Interop.GetLastInput() > activityThreshold;
                if (cursorHidden != shouldHide)
                {
                    if (shouldHide)
                    {
                        ShowCursor(0);
                        PanelBackControls.Visible = false;
                        tableLayoutPanel1.RowCount = 1;
                    }
                    else
                    {
                        ShowCursor(1);
                        PanelBackControls.Visible = true;
                        tableLayoutPanel1.RowCount = 2;
                    }
                    cursorHidden = shouldHide;
                }
            }
            else Timer2.Stop();
        }


        private void Play()
        {
            if (VideoView.MediaPlayer.Media != null)
            {
                if (VideoView.MediaPlayer.IsPlaying)
                {
                    VideoView.MediaPlayer.Pause();
                    PlayBtn.Image = Properties.Resources.pause;
                    Timer1.Stop();
                }
                else
                {
                    VideoView.MediaPlayer.Play();
                    PlayBtn.Image = Properties.Resources.play;
                    Timer1.Start();
                }

                VideoSeekBar.Maximum = Convert.ToInt32(VideoView.MediaPlayer.Media.Duration);
                VideoView.MediaPlayer.AspectRatio = VideoView.Width + ":" + VideoView.Height;

                // Gets total video duration and insert into label2 like 00:54
                TimeSpan t = TimeSpan.FromMilliseconds(VideoView.MediaPlayer.Media.Duration);
                TotalTimeLabel.Text = string.Format("{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
            }
        }

        private void On_VolumeBar_Scroll(object sender, EventArgs e)
        {
            VideoView.MediaPlayer.Volume = VolBar.Value;
            Label3.Text = VolBar.Value + "%";
            Properties.Settings.Default.volume = VolBar.Value;
            Properties.Settings.Default.Save();
        }

        private void Fullscreen()
        {
            fullScreenForm.ShowInTaskbar = false;
            fullScreenForm.ShowIcon = false;
            if (VideoView.MediaPlayer.Media != null)
            {
                InitFullScreenForm();
                if (IsFullScreen)
                {
                    fullScreenForm.Controls.Remove(tableLayoutPanel1);
                    this.Controls.Add(tableLayoutPanel1);
                    fullScreenForm.Hide();

                    IsFullScreen = false;
                    FulScrBtn.Image = Properties.Resources.full;
                    Timer2.Stop();
                }
                else
                {
                    // fullScreenForm = new Form();
                    InitFullScreenForm();
                    fullScreenForm.Controls.Add(tableLayoutPanel1);
                    fullScreenForm.Show();
                    IsFullScreen = true;
                    FulScrBtn.Image = Properties.Resources.exit_full;
                    Timer2.Start();
                }
            }
        }
        private void InitFullScreenForm()
        {
            fullScreenForm.FormBorderStyle = FormBorderStyle.None;
            fullScreenForm.WindowState = FormWindowState.Maximized;
            tableLayoutPanel1.Dock = DockStyle.Fill;
        }

        private void On_Stop_Button_Click(object sender, EventArgs e)
        {
            if (VideoView.MediaPlayer.Media != null)
            {
                Timer1.Stop();
                VideoView.MediaPlayer.Stop();
                VideoSeekBar.Value = 1;
                CrntTimeLabel.Text = "00:00";
                TotalTimeLabel.Text = "00:00";
                PlayBtn.Image = Properties.Resources.pause;
            }
        }

        private void ChangeProgress(ProgressBar bar, MouseEventArgs e)
        {
            // It will change progressbar value
            if (e.Button == MouseButtons.Left)
            {
                var mousepos = Math.Min(Math.Max(e.X, 0), bar.ClientSize.Width);
                var value = System.Convert.ToInt32(bar.Minimum + (bar.Maximum - bar.Minimum) * mousepos / (double)bar.ClientSize.Width);
                if (value > bar.Value & value < bar.Maximum)
                {
                    bar.Value = value + 1;
                    bar.Value = value;
                }
                else bar.Value = value;
                if (VideoView.MediaPlayer.Media != null) VideoView.MediaPlayer.Time = VideoSeekBar.Value;
            }
        }

        private void VideoView_DoubleClick(object sender, EventArgs e)
        {
            Fullscreen();
            Play();
        }

        private void VideoView_SizeChanged(object sender, EventArgs e)
        {
            if (VideoView.MediaPlayer.Media != null) VideoView.MediaPlayer.AspectRatio = VideoView.Width + ":" + VideoView.Height;
        }

        private void On_Volume_Button_Click(object sender, EventArgs e)
        {
            // Speaker/Volume Button click
            if (VideoView.MediaPlayer.Mute)
            {
                VideoView.MediaPlayer.Mute = false;
                VolBtn.Image = Properties.Resources.volume;
            }
            else
            {
                VideoView.MediaPlayer.Mute = true;
                VolBtn.Image = Properties.Resources.mute;
            }
            Properties.Settings.Default.volume = VolBar.Value;
            Properties.Settings.Default.Save();
        }

        private void Play_Button_Click(object sender, EventArgs e) => Play();
        private void On_FullScreen_Button_Click(object sender, EventArgs e) => Fullscreen();
        private void VideoView_Click(object sender, EventArgs e) => Play();
        private void Controller_Load(object sender, EventArgs e) => VolBar.Value = Properties.Settings.Default.volume <= 0 ? 1 : Properties.Settings.Default.volume;
        private void VideoSeekBar_MouseDown(object sender, MouseEventArgs e) => ChangeProgress(VideoSeekBar, e);
        private void VideoSeekBar_MouseMove(object sender, MouseEventArgs e) => ChangeProgress(VideoSeekBar, e);
    }

}
static class User32Interop
{
    // It returns time of last mouse/keyboard move
    public static TimeSpan GetLastInput()
    {
        var plii = new LASTINPUTINFO();
        plii.cbSize = Convert.ToUInt32(Marshal.SizeOf(plii));
        if (GetLastInputInfo(ref plii))
            return TimeSpan.FromMilliseconds(Environment.TickCount - plii.dwTime);
        else
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);
    struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }
}