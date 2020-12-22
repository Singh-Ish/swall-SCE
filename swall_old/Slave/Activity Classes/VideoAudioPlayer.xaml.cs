using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace Prototype1.Activity_Classes
{
    /// <summary>
    /// Interaction logic for VideoAudioPLayer.xaml
    /// </summary>
    public partial class VideoAudioPlayer : UserControl
    {
        
        private bool userIsDraggingSlider = false;

        private DispatcherTimer timer;

        /// <summary>
        /// Used to bind the visibility of interactive objects
        /// </summary>
        public bool InteractiveElementsVisibility
        {
            get
            {
                return (bool)GetValue(InteractiveElementsVisibilityProperty);
            }
            set
            {
                SetValue(InteractiveElementsVisibilityProperty, value);
            }
        }

        /// <summary>
        /// Dependency Property to make InteractiveElementsVisibility bindable in Xaml
        /// </summary>
        public static readonly DependencyProperty InteractiveElementsVisibilityProperty =
                DependencyProperty.Register("InteractiveElementsVisibility",
                                            typeof(bool),
                                            typeof(VideoAudioPlayer));

        /// <summary>
        /// Flag indicating whether or not the video is paused
        /// </summary>
        public bool IsPaused
        {
            get
            {
                return (bool)GetValue(IsPausedProperty);
            }
            set
            {
                SetValue(IsPausedProperty, value);
            }
        }

        public static readonly DependencyProperty IsPausedProperty =
                DependencyProperty.Register("IsPaused",
                                            typeof(bool),
                                            typeof(VideoAudioPlayer));

        public VideoAudioPlayer()
        {
            InitializeComponent();

            mePlayer.MediaEnded += MePlayer_MediaEnded;
            mePlayer.MediaOpened += MePlayer_MediaOpened;
        }

        private void MePlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if ((mePlayer.NaturalDuration.HasTimeSpan))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
            }
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        private void MePlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            timer = null;
            mePlayer.Volume = 0.5;
            mePlayer.Close();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Value = mePlayer.Position.TotalMilliseconds;
            }
        }

        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            updatePosition();
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            updatePosition();
        }

        /// <summary>
        /// Update the player with the value gathered from the slider
        /// </summary>
        private void updatePosition()
        {
            TimeSpan newPosition = TimeSpan.FromMilliseconds(sliProgress.Value);
            lblProgressStatus.Text = newPosition.ToString(@"hh\:mm\:ss");
            mePlayer.Position = newPosition;
        }

        private void mePlayer_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (IsPaused)
            {
                mePlayer.Play();
                IsPaused = false;
            }
            else
            {
                mePlayer.Pause();
                IsPaused = true;
            }
        }
    }
}
