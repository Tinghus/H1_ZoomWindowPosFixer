using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;


namespace H1_ZoomWindowPosFixer
{
    internal class Program
    {
        private static List<Tuple<IntPtr, RECT>> savedWindowPositions = new List<Tuple<IntPtr, RECT>>();
        private static List<string> whiteList = new List<string>();
        private static bool useWhitelist = false;

        static void Main(string[] args)
        {
            CreateWhitelist();

            while (true)
            {
                if (savedWindowPositions.Count == 0)
                {
                    Console.WriteLine("You have not saved any positions yet");
                }

                Console.WriteLine("Press \"S\" to save and \"L\" to load. \"T\" to get all window titles \"Q\" to quit");

                ConsoleKeyInfo consoleKey;

                Console.CursorVisible = false;
                consoleKey = Console.ReadKey(true);


                switch (consoleKey.Key)
                {
                    case ConsoleKey.S:
                        SaveWindowPosition();
                        Console.WriteLine("Positions saved");
                        break;

                    case ConsoleKey.L:
                        if (savedWindowPositions.Count > 0)
                        {
                            LoadWindowPos();
                            Console.WriteLine("Positions loaded");
                        }
                        else
                        {
                            Console.WriteLine("No windows saved. If you think that is an error check your whitelist settings");
                        }
                        break;

                    case ConsoleKey.T:
                        Console.WriteLine("Titles");
                        GetWindowTitles();
                        break;

                    case ConsoleKey.Q:
                        Environment.Exit(0);
                        break;
                }
            }

        }

        static void GetWindowTitles()
        {
            Process[] proccesList = Process.GetProcesses();


            foreach (Process process in proccesList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    IntPtr hwnd = process.MainWindowHandle;
                    if (hwnd != IntPtr.Zero)
                    {
                        Console.Write("Press any key to get back to the menu");
                        Console.ReadKey();
                    }
                }
            }
        }

        static void CreateWhitelist()
        {
            whiteList.Add("H1_ZoomWindowPosFixer".ToLower());

        }

        static bool WindowIsOneWhitelist(string title)
        {
            foreach (string whiteListedTitle in whiteList)
            {
                if (title.ToLower().Contains(whiteListedTitle))
                {
                    return true;
                }
            }

            return false;
        }

        static void SaveWindowPosition()
        {
            Process[] proccesList = Process.GetProcesses();


            foreach (Process process in proccesList)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    IntPtr hwnd = process.MainWindowHandle;
                    if (hwnd != IntPtr.Zero)
                    {
                        if (useWhitelist && !WindowIsOneWhitelist(process.MainWindowTitle))
                        {
                            continue;
                        }

                        RECT rect = new RECT();
                        GetWindowRect(hwnd, ref rect);
                        GetWindowRect(hwnd, ref rect);
                        savedWindowPositions.Add(new Tuple<IntPtr, RECT>(hwnd, rect));
                    }
                }
            }
        }

        static void LoadWindowPos()
        {

            foreach (var windowPosition in savedWindowPositions)
            {
                SetWindowPos(
                    windowPosition.Item1,
                    IntPtr.Zero,
                    windowPosition.Item2.Left,
                    windowPosition.Item2.Top,
                    windowPosition.Item2.Right - windowPosition.Item2.Left,
                    windowPosition.Item2.Bottom - windowPosition.Item2.Top,
                    SWP_NOZORDER | SWP_NOACTIVATE);

                UpdateWindow(windowPosition.Item1);
            }



        }


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        static extern bool UpdateWindow(IntPtr hWnd);

        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_NOACTIVATE = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
    }
}