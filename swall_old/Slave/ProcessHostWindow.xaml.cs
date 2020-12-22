using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

namespace Prototype1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ProcessHostWindow : Window
    {
        private Process _process;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]
        static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32")]
        private static extern IntPtr SetParent(IntPtr hWnd, IntPtr hWndParent);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const int SWP_NOZORDER = 0x0004;
        private const int SWP_NOACTIVATE = 0x0010;
        private const int SWP_ACTIVATE = 0x1101;
        private const int GWL_STYLE = -16;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_MAXIMIZE = 0x01000000;
        private const int WS_THICKFRAME = 0x00040000;
        const string patran = "patran";
        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 SWP_SHOWWINDOW = 0x0040;
        public static uint MF_BYPOSITION = 0x400;
        public static uint MF_REMOVE = 0x1000;
        private string currentProcessName;
        public ProcessHostWindow(string currentProcessName)
        {
            InitializeComponent();
            this.currentProcessName = currentProcessName;
            Loaded += (s, e) => LaunchChildProcess();
        }

        private void LaunchChildProcess()
        {
            try
            {
                System.Timers.Timer inputTimer = new System.Timers.Timer();
                inputTimer.AutoReset = false;
                inputTimer.Interval = 5000;
                inputTimer.Elapsed += InputTimer_Elapsed;
                inputTimer.Enabled = true;
            }
            catch (Exception)
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Close();
                    }));
                }
                else
                {
                    Close();
                }
            }
        }

        private void InputTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try {

                (sender as System.Timers.Timer).Stop();
                (sender as System.Timers.Timer).Dispose();
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Console.Out.WriteLine("Check0");
                        Process[] p = Process.GetProcessesByName(currentProcessName);
                        if (p.Length != 0)
                        {
                            _process = p[0];
                            
                        }
                        else
                        {
                            var processes = Process.GetProcesses().Where(x => x.MainWindowTitle.Equals(currentProcessName)).ToList();
                            if (processes.Count != 0)
                            {
                                Console.Out.WriteLine("Check1");
                                foreach (var process in processes)
                                {
                                    if (!process.HasExited)
                                    {
                                        Console.Out.WriteLine("Check2");
                                        var id = process.Id;
                                        var Wintitle = process.MainWindowTitle;
                                        Console.WriteLine("title: {0}, id: {1}", Wintitle, id);
                                        _process = process;
                                        
                                    }
                                }
                            }
                            else
                            {
                                Close();
                                return;
                            }
                        }
                        if (_process != null && !_process.HasExited)
                        {
                            _process.EnableRaisingEvents = true;
                            _process.Exited += _process_Exited;
                            var helper = new WindowInteropHelper(this);

                            SetParent(_process.MainWindowHandle, helper.Handle);

                            // remove control box
                            int style = GetWindowLong(_process.MainWindowHandle, GWL_STYLE);
                            style = style & ~WS_CAPTION;
                            SetWindowLong(_process.MainWindowHandle, GWL_STYLE, style);
                            // resize embedded application & refresh
                            ResizeEmbeddedApp();
                        }
                    }));
                }
                else
                {
                    Console.Out.WriteLine("Check0");
                    Process[] p = Process.GetProcessesByName(currentProcessName);
                    if (p.Length != 0)
                    {
                        _process = p[0];
                        _process.EnableRaisingEvents = true;
                    }
                    else
                    {
                        var processes = Process.GetProcesses().Where(x => x.MainWindowTitle.Equals(currentProcessName)).ToList();
                        if (processes.Count != 0)
                        {
                            Console.Out.WriteLine("Check1");
                            foreach (var process in processes)
                            {
                                if (!process.HasExited)
                                {
                                    Console.Out.WriteLine("Check2");
                                    var id = process.Id;
                                    var Wintitle = process.MainWindowTitle;
                                    Console.WriteLine("title: {0}, id: {1}", Wintitle, id);
                                    _process = process;
                                    _process.EnableRaisingEvents = true;
                                }
                            }
                        }
                    }
                    if (_process != null && !_process.HasExited)
                    {
                        _process.Exited += _process_Exited;
                        var helper = new WindowInteropHelper(this);

                        SetParent(_process.MainWindowHandle, helper.Handle);

                        // remove control box
                        int style = GetWindowLong(_process.MainWindowHandle, GWL_STYLE);
                        style = style & ~WS_CAPTION;
                        SetWindowLong(_process.MainWindowHandle, GWL_STYLE, style);
                        // resize embedded application & refresh
                        ResizeEmbeddedApp();
                    }
                }
            }
            catch (Exception)//TODO ADD EXCEPTION HANDLING
            {
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
                {
                    //remove the current coaxer
                    System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        Close();
                    }));
                }
                else
                {
                    Close();
                }
            }
        }

        private void _process_Exited(object sender, EventArgs e)
        {
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())  //Must execute this method on the UI thread
            {
                //remove the current coaxer
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    (sender as Process).Exited -= _process_Exited;
                    _process = null;
                    Close();
                }));
            }
            else
            {
                (sender as Process).Exited -= _process_Exited;
                _process = null;
                Close();
            }
        }

        private void ResizeEmbeddedApp()
        {
            if (_process == null)
                return;
            SetWindowPos(_process.MainWindowHandle, HWND_TOP, 0, 0, (int)this.ActualWidth, (int)this.ActualHeight, SWP_NOACTIVATE);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    _process.Exited -= _process_Exited;
                    _process.Kill();
                    _process.Close();
                    _process = null;
                }
            }
            catch (Exception) { }//TODO ADD EXCEPTION HANDLING
        }
    }
}
