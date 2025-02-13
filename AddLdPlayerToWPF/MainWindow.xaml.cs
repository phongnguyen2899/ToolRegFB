using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using AddLdPlayerToWPF.Ultil;
using Auto_LDPlayer;
using Auto_LDPlayer.Enums;
using Emgu.CV.Flann;

namespace AddLdPlayerToWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    /// TRẦN DUY LUYỆN
    /// FB: https://www.facebook.com/luyen.tranduy.56/
    public partial class MainWindow : Window
    {
        [DllImport("user32")] private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32")] private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32")] private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int Width, int Height, bool Repaint);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int GWL_STYLE = -16;
        private const int WS_THICKFRAME = 0x00040000;
        private const int WS_CAPTION = 0x00C00000;
        private const int WS_CHILD = 0x40000000;
        public MainWindow()
        {
            InitializeComponent();
            initLd();
        }

        public void initLd()
        {
            LDPlayer.PathLD = @"D:\work\devops\HTTTN\tool\LDPlayer\ldconsole.exe";
            KAutoHelper.ADBHelper.SetADBFolderPath(@"D:\work\devops\HTTTN\tool\LDPlayer");
            //LDPlayer.Open(LDType.Name, "LDPlayer");
            //await Task.Delay(1000);
            //LDPlayer.Open(LDType.Name, "LDPlayer-1");
        }

        private void BtnTest_Click1(object sender, RoutedEventArgs e)
        {
            try
            {
                LDPlayer.Open(LDType.Name, "LDPlayer_Core");
                LDPlayer.Copy("LDPlayer_1", "LDPlayer_Core");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void BtnTest_Click(object sender, RoutedEventArgs e)
        {
            int thread = Convert.ToInt32(txtThread.Text);
            Thread t = new Thread(() =>
            {
                int delay = 100;
                List<string> name = CommonFuntion.GetName(thread);

                using (List<string>.Enumerator enumerator = name.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        string ldName = enumerator.Current;
                        Thread t1 = new Thread(() =>
                        {
                            for (; ; )
                            {
                                this.RunNow(ldName);
                            }
                        });
                        t1.Start();
                        Thread.Sleep(10000);
                    }
                }
            });
            t.Start();
            //RunNow();
        }
        public void RunNow(string ldName)
        {
            // đóng phiên cũ
            LDPlayer.Close(LDType.Name, ldName);
            Thread.Sleep(1000);
            base.Dispatcher.Invoke(new Action(delegate ()
            {
                //IEnumerator enumerator = PanelTest.Children.GetEnumerator();
                var elements = PanelTest.Children;

                for (int i = elements.Count - 1; i >= 0; i--)
                {
                    var element = PanelTest.Children[i];
                    WindowsFormsHost item = element as WindowsFormsHost;

                    if (item != null && item.Name == ldName.Replace("-", ""))
                    {
                        PanelTest.Dispatcher.Invoke(new Action(delegate ()
                        {
                            PanelTest.Children.Remove(item);
                        }));
                    }
                }

                //while (enumerator.MoveNext())
                //{
                //    WindowsFormsHost item = (WindowsFormsHost)enumerator.Current;
                //    bool flag2 = item.Name == ldName.Replace("-", "");
                //    if (flag2)
                //    {
                //        PanelTest.Dispatcher.Invoke(new Action(delegate ()
                //        {
                //            PanelTest.Children.Remove(item);
                //        }));
                //    }
                //}
            }));

            Thread.Sleep(5000);
            OpenLD(ldName);
            Thread.Sleep(5000);
            var data = LDPlayer.GetDevices2Running();
            foreach (var item in data)
            {
                Console.WriteLine(item.Index);
                Console.WriteLine(item.Name);
            }
        }

        private void OpenLD(string ldName)
        {
            PanelTest.Dispatcher.Invoke(new Action(delegate ()
            {
                int targetWidth = 270;
                int targetHeight = 480;
                var ldList = LDPlayer.GetDevices2();
                if (!ldList.Select(x => x.Name).Contains(ldName))
                {
                    LDPlayer.Copy(ldName, "LDPlayer_Core");
                    Thread.Sleep(10000);
                }

                LDPlayer.Open(LDType.Name, ldName);

                Stopwatch sw = new Stopwatch();
                sw.Start();
                int timeout = 10 * 1000;
                //Thread.Sleep(5000);
                while (WinGetHandle(ldName) == IntPtr.Zero)
                {
                    Thread.Sleep(10);
                    if (sw.ElapsedMilliseconds > timeout)
                    {
                        sw.Stop();
                        return;
                    }
                }
                IntPtr ldplayerHandle = WinGetHandle(ldName);
                WindowsFormsHost windowsFormsHost = new WindowsFormsHost();
                System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
                panel.Name = ldName.Replace("-", "");
                panel.BorderStyle = BorderStyle.FixedSingle;
                panel.BackColor = System.Drawing.Color.Red;
                panel.Size = new System.Drawing.Size(targetWidth, targetHeight);
                SetParent(ldplayerHandle, panel.Handle);
                windowsFormsHost.Child = panel;
                windowsFormsHost.Height = targetHeight + 100;
                windowsFormsHost.Width = targetWidth + 50;
                windowsFormsHost.Name = ldName.Replace("-","");
                MoveWindow(ldplayerHandle, -1, -36, targetWidth, targetHeight, true);
                PanelTest.Children.Add(windowsFormsHost);
            }));
        }

        public static IntPtr WinGetHandle(string wName)
        {
            IntPtr hWnd = IntPtr.Zero;
            var process = Process.GetProcesses();
            foreach (Process pList in process)
            {
                if (pList.MainWindowTitle.Length == 0) continue;


                if (pList.MainWindowTitle.Equals(wName))
                {
                    hWnd = pList.MainWindowHandle;
                }
            }
            return hWnd;
        }
        public int[] LDSize = new int[]
        {
            240,
            360,
            -1,
            -36,
            240,
            395
        };

    }
}
