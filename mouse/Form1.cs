using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Diagnostics;

namespace mouse
{
    public partial class Form1 : Form
    {
        MouseHook mh;
        LowLevelKeyboardListener listener;
        private bool clicked = true;

        private bool KeyboardState = true;
        private bool MouseState = true;

        private bool KeyboardSoundState = false;

        System.Media.SoundPlayer MouseSound = new System.Media.SoundPlayer(Properties.Resources.click2);
        System.Media.SoundPlayer KeyboardSound1 = new System.Media.SoundPlayer(Properties.Resources.KeyClick);
        System.Media.SoundPlayer KeyboardSound2 = new System.Media.SoundPlayer(Properties.Resources.KeyClick);

        public Form1()
        {
            
            InitializeComponent();
            mh = new MouseHook();
            mh.SetHook();
            mh.MouseMoveEvent += mh_MouseMoveEvent;
            mh.MouseClickEvent += mh_MouseClickEvent;
            mh.MouseDownEvent += mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;

            listener = new LowLevelKeyboardListener();
            listener.OnKeyPressed += KeyboardPressed;
            listener.HookKeyboard();
        }
        private void mh_MouseDownEvent(object sender, MouseEventArgs e)
        {
        }

        private void mh_MouseUpEvent(object sender, MouseEventArgs e)
        {

        }

        private void mh_MouseMoveEvent(object sender, MouseEventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private void mh_MouseClickEvent(object sender, MouseEventArgs e)
        {
            if (MouseState)
            {
                if (clicked)
                {
                    clicked = false;
                }
                else
                {
                    clicked = true;
                    //MessageBox.Show(e.X + "-" + e.Y);
                    if (e.Button != MouseButtons.None)
                    {

                        MouseSound.Play();

                    }

                }
            }
        }

        private void KeyboardPressed(object sender, KeyPressedArgs e)
        {
            if (KeyboardState)
            {
                if (KeyboardSoundState)
                {
                    KeyboardSoundState = false;
                    KeyboardSound1.Play();
                }
                else
                {
                    KeyboardSoundState = true;
                    KeyboardSound2.Play();
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            mh.UnHook();
            listener.UnHookKeyboard();

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (MouseState)
            {
                MouseState = false;
                button1.BackgroundImage = Properties.Resources.off;
            }
            else
            {
                MouseState = true;
                button1.BackgroundImage = Properties.Resources.on;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (KeyboardState)
            {
                KeyboardState = false;
                button2.BackgroundImage = Properties.Resources.off;
            }
            else
            {
                KeyboardState = true;
                button2.BackgroundImage = Properties.Resources.on;
            }
        }
    }
}
#region kod ze strony: https://github.com/kellman616/TouchScreenControlEverythingDemo/blob/master/Demo1_Detect_Mouse_Position_And_Click_Event/Demo_mousehook_csdn/Form1.cs

public class Win32Api
{
    [StructLayout(LayoutKind.Sequential)]
    public class POINT
    {
        public int x;
        public int y;
    }
    [StructLayout(LayoutKind.Sequential)]
    public class MouseHookStruct
    {
        public POINT pt;
        public int hwnd;
        public int wHitTestCode;
        public int dwExtraInfo;
    }
    public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern bool UnhookWindowsHookEx(int idHook);
    [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
}


public class MouseHook
{
    private Point point;
    private Point Point
    {
        get { return point; }
        set
        {
            if (point != value)
            {
                point = value;
                if (MouseMoveEvent != null)
                {
                    var e = new MouseEventArgs(MouseButtons.None, 0, point.X, point.Y, 0);
                    MouseMoveEvent(this, e);
                }
            }
        }
    }
    private int hHook;
    private const int WM_MOUSEMOVE = 0x200;
    private const int WM_LBUTTONDOWN = 0x201;
    private const int WM_RBUTTONDOWN = 0x204;
    private const int WM_MBUTTONDOWN = 0x207;
    private const int WM_LBUTTONUP = 0x202;
    private const int WM_RBUTTONUP = 0x205;
    private const int WM_MBUTTONUP = 0x208;
    private const int WM_LBUTTONDBLCLK = 0x203;
    private const int WM_RBUTTONDBLCLK = 0x206;
    private const int WM_MBUTTONDBLCLK = 0x209;
    public const int WH_MOUSE_LL = 14;
    public Win32Api.HookProc hProc;
    public MouseHook()
    {
        this.Point = new Point();
    }
    public int SetHook()
    {
        hProc = new Win32Api.HookProc(MouseHookProc);
        hHook = Win32Api.SetWindowsHookEx(WH_MOUSE_LL, hProc, IntPtr.Zero, 0);
        return hHook;
    }
    public void UnHook()
    {
        Win32Api.UnhookWindowsHookEx(hHook);
    }
    private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
    {
        Win32Api.MouseHookStruct MyMouseHookStruct = (Win32Api.MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(Win32Api.MouseHookStruct));
        if (nCode < 0)
        {
            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
        else
        {
            if (MouseClickEvent != null)
            {
                MouseButtons button = MouseButtons.None;
                int clickCount = 0;
                switch ((Int32)wParam)
                {
                    case WM_LBUTTONDOWN:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        MouseDownEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_RBUTTONDOWN:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        MouseDownEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_MBUTTONDOWN:
                        button = MouseButtons.Middle;
                        clickCount = 1;
                        MouseDownEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_LBUTTONUP:
                        button = MouseButtons.Left;
                        clickCount = 1;
                        MouseUpEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_RBUTTONUP:
                        button = MouseButtons.Right;
                        clickCount = 1;
                        MouseUpEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                    case WM_MBUTTONUP:
                        button = MouseButtons.Middle;
                        clickCount = 1;
                        MouseUpEvent(this, new MouseEventArgs(button, clickCount, point.X, point.Y, 0));
                        break;
                }

                var e = new MouseEventArgs(button, clickCount, point.X, point.Y, 0);
                MouseClickEvent(this, e);
            }
            this.Point = new Point(MyMouseHookStruct.pt.x, MyMouseHookStruct.pt.y);
            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }

    public delegate void MouseMoveHandler(object sender, MouseEventArgs e);
    public event MouseMoveHandler MouseMoveEvent;

    public delegate void MouseClickHandler(object sender, MouseEventArgs e);
    public event MouseClickHandler MouseClickEvent;

    public delegate void MouseDownHandler(object sender, MouseEventArgs e);
    public event MouseDownHandler MouseDownEvent;

    public delegate void MouseUpHandler(object sender, MouseEventArgs e);
    public event MouseUpHandler MouseUpEvent;


}

#endregion


#region kod ze strony: http://www.dylansweb.com/2014/10/low-level-global-keyboard-hook-sink-in-c-net/
public class LowLevelKeyboardListener
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    public event EventHandler<KeyPressedArgs> OnKeyPressed;

    private LowLevelKeyboardProc _proc;
    private IntPtr _hookID = IntPtr.Zero;

    public LowLevelKeyboardListener()
    {
        _proc = HookCallback;
    }

    public void HookKeyboard()
    {
        _hookID = SetHook(_proc);
    }

    public void UnHookKeyboard()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            if (OnKeyPressed != null) { OnKeyPressed(this, new KeyPressedArgs(KeyInterop.KeyFromVirtualKey(vkCode))); }
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }
}

public class KeyPressedArgs : EventArgs
{
    public Key KeyPressed { get; private set; }

    public KeyPressedArgs(Key key)
    {
        KeyPressed = key;
    }
}
#endregion
