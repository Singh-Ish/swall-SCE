using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox : Window
    {
        private System.Windows.Threading.DispatcherTimer timer;
        private int timeLeft = 60000;
        public bool isTimed
        {
            set
            {
                if (value)
                {
                    timer = new System.Windows.Threading.DispatcherTimer();
                    timer.Interval = new TimeSpan(0, 0, 0, 1);
                    timer.Tick += timer_Tick;
                    timer.Start();
                }
                else
                {
                    timeTextBox.Visibility = Visibility.Collapsed;
                }
            }
        }
        public MessageBox()
        {
            InitializeComponent();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            Console.Out.WriteLine("Tick");
            try
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        if (timeLeft > 0)
                        {
                            // Display the new time left
                            // by updating the Time Left label.
                            timeLeft = timeLeft - 1000;
                            timeTextBox.Text = "Accepting in...\r\n" + TimeSpan.FromMilliseconds(timeLeft).ToString(@"hh\:mm\:ss");
                        }
                        else
                        {
                            timer.Stop();
                            OkButton_Click(null, null);
                        }
                    }));
                }
                else
                {
                    if (timeLeft > 0)
                    {
                        // Display the new time left
                        // by updating the Time Left label.
                        timeLeft = timeLeft - 1000;
                        Console.Out.WriteLine("timeLeft" + timeLeft);
                        timeTextBox.Text = "Accepting in...\r\n" + TimeSpan.FromMilliseconds(timeLeft).ToString(@"hh\:mm\:ss");
                    }
                    else
                    {
                        timer.Stop();
                        OkButton_Click(null, null);
                    }
                }
            }
            catch (Exception) { }//TODO ADD EXCEPTION HANDLING
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
            {
                //remove the current coaxer
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if(timer != null)
                    {
                        timer.Stop();
                    }
                    UserChoice args = new UserChoice();
                    args.Approved = true;
                    OnButtonClicked(args);
                    this.Close();
                }));
            }
            else
            {
                if (timer != null)
                {
                    timer.Stop();
                }
                UserChoice args = new UserChoice();
                args.Approved = true;
                OnButtonClicked(args);
                this.Close();
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
            {
                //remove the current coaxer
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    if (timer != null)
                    {
                        timer.Stop();
                    }
                    UserChoice args = new UserChoice();
                    args.Approved = false;
                    OnButtonClicked(args);
                    this.Close();
                }));
            }
            else
            {
                if (timer != null)
                {
                    timer.Stop();
                }
                UserChoice args = new UserChoice();
                args.Approved = false;
                OnButtonClicked(args);
                this.Close();
            }
        }
        protected virtual void OnButtonClicked(UserChoice uc)
        {
            ButtonClickedEvent?.Invoke(this, uc);
        }

        public event EventHandler<UserChoice> ButtonClickedEvent;

    }
    public class UserChoice : EventArgs
    {
        public bool Approved { get; set; }
    }
}
