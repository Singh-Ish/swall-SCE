using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for SwipeToExit.xaml
    /// </summary>
    public partial class SwipeToExit : Window
    {
        private Point m_start;

        double bottom = 500;
        private bool captured = false;
        public Timer timer;
        private double timerInterval = 70;
        private static double rescaleFactor = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 1080;
        public SwipeToExit()
        {
            InitializeComponent();
            bottom = bottom * rescaleFactor;
            timer = new Timer();

            timer.Interval = timerInterval;

            timer.Elapsed += Timer_Elapsed;

            timer.AutoReset = true;

            timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)

        {
            try
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess()) //Must execute this method on the UI thread

                {

                    //remove the current coaxer

                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>

                    {
                        try
                        {
                            Thickness thick = new Thickness(0, 0, 0, bottom);
                     
                     
                     
                            border.Margin = thick;
                     
                            if (bottom == 850 * rescaleFactor)
                     
                            {

                                border.Visibility = Visibility.Visible;

                                textBox.FontSize = 26 * rescaleFactor;

                            }

                            if (bottom >= 1050 * rescaleFactor)

                            {

                                border.Visibility = Visibility.Hidden;

                                textBox.FontSize = 25 * rescaleFactor;

                                bottom = 500 * rescaleFactor;

                            }



                            bottom += 10;
                        }
                        catch (Exception ce) { }//TODO ADD EXCEPTION HANDLING
                    }));
                }
            }
            catch (Exception ce) { }//TODO ADD EXCEPTION HANDLING

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)

        {
            //ActiveAppSpace.showWindow = true;
            m_start = e.GetPosition(mainWindow);

            if (!captured)

            {

                captured = true;

            }
            e.Handled = true;
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)

        {
            

            if (captured)

            {
                Vector offset = Point.Subtract(e.GetPosition(mainWindow), m_start);

                if (offset.Y >= 100 || offset.Y <= -100)

                {
                    captured = false;

                    timer.Enabled = false;
                    MessageBox mb = new MessageBox();
                    mb.Topmost = true;
                    mb.ButtonClickedEvent += Mb_ButtonClickedEvent;
                    mb.textBox.Text = "Would you like to close the current Activity?";
                    mb.isTimed = true;
                    mb.Show();

                }

            }

        }
        private void Mb_ButtonClickedEvent(object sender, UserChoice e)
        {
            if (e.Approved)
            {
                this.Close();
            }
            else
            {
                //ActiveAppSpace.showWindow = true;
                timer.Enabled = true;
            }
        }

        private void grid_MouseUp(object sender, MouseButtonEventArgs e)

        {

            if (captured)

            {

                captured = false;

            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            grid.Width = grid.Width * rescaleFactor;
            border.Height = border.Height * rescaleFactor;
            border.Width = border.Width * rescaleFactor;
            textBox.FontSize = textBox.FontSize * rescaleFactor;
            Swipe.Width = Swipe.Width * rescaleFactor;
        }

        private void grid_FocusableChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Console.Out.WriteLine("Focus Changed" + sender.GetType());
        }
    }
}

