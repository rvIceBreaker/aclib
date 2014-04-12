using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aclib
{
    // Summary:
    //     Specifies constants that define foreground and background colors for the
    //     console.
    [Serializable]
    public enum AdvConsoleColor
    {
        None = -1,
        Black = 0,
        DarkBlue = 1,
        DarkGreen = 2,
        DarkCyan = 3,
        DarkRed = 4,
        DarkMagenta = 5,
        DarkYellow = 6,
        Gray = 7,
        DarkGray = 8,
        Blue = 9,
        Green = 10,
        Cyan = 11,
        Red = 12,
        Magenta = 13,
        Yellow = 14,
        White = 15,
    }

    public class Point
    {
        public int X, Y;
        public Point(int x, int y) { X = x; Y = y; }

        public static Point operator +(Point A, Point B)
        {
            return new Point(A.X + B.X, A.Y + B.Y);
        }

        public static Point operator -(Point A, Point B)
        {
            return new Point(A.X - B.X, A.Y - B.Y);
        }
    }

    public class Rectangle
    {
        public int X, Y, Width, Height;
        public Rectangle(Point position, int width, int height) : this(position.X, position.Y, width, height) {  }
        public Rectangle(int x, int y, int width, int height) { X = x; Y = y; Width = width; Height = height; }

        public bool Contains(Point point)
        {
            return (point.X >= X && point.Y >= Y && point.X <= (X + Width - 1) && point.Y <= (Y + Height - 1));
        }

        public static Rectangle operator +(Rectangle A, Point B)
        {
            return new Rectangle(A.X + B.X, A.Y + B.Y, A.Width, A.Height);
        }

        public static Rectangle operator -(Rectangle A, Point B)
        {
            return new Rectangle(A.X - B.X, A.Y - B.Y, A.Width, A.Height);
        }
    }

    public class CharacterInfo
    {
        public bool KeyDown;
        public char UnicodeChar;
        public char AsciiChar;
        public Keys KeyCode;
        public ControlKeyStates ControlKeyState;
    }

    public class KeyState
    {
        private ControlKeyStates keyState;

        public bool Alt { get { return ((keyState & ControlKeyStates.LEFT_ALT_PRESSED) == ControlKeyStates.LEFT_ALT_PRESSED || (keyState & ControlKeyStates.RIGHT_ALT_PRESSED) == ControlKeyStates.RIGHT_ALT_PRESSED); } }
        public bool Ctrl { get { return ((keyState & ControlKeyStates.LEFT_CTRL_PRESSED) == ControlKeyStates.LEFT_CTRL_PRESSED || (keyState & ControlKeyStates.RIGHT_CTRL_PRESSED) == ControlKeyStates.RIGHT_CTRL_PRESSED); } }
        public bool Shift { get { return ((keyState & ControlKeyStates.SHIFT_PRESSED) == ControlKeyStates.SHIFT_PRESSED); } }

        public KeyState(ControlKeyStates state)
        {
            keyState = state;
        }
    }

    public class KeyInfo
    {
        private CharacterInfo info;

        public Keys KeyCode { get { return info.KeyCode; } }

        public char Character { get { return info.AsciiChar; } }
        public char UCharacter { get { return info.UnicodeChar; } }
        public bool IsPressed { get { return info.KeyDown; } }

        KeyState state;
        public KeyState KeyState { get { return state; } }

        public KeyInfo(CharacterInfo info)
        {
            this.info = info;
            state = new KeyState(info.ControlKeyState);
        }        
    }

    public enum MouseInput
    {
        LeftButton,
        RightButton,
        Horiz_WheelUp,
        Horiz_WheelDown,
        Vert_WheelUp,
        Vert_WheelDown,
    }

    public enum ButtonState
    {
        Pressed,
        Released
    }

    public enum WheelState
    {
        Increase,
        Decrease,
        None
    }

    public class Wheel
    {
        internal WheelState state;
        public WheelState State { get { return state; } }
    }

    public class Button
    {
        internal ButtonState state;
        internal bool doubleClick;

        public ButtonState State { get { return state; } }
        public bool WasDoubleClick { get { return doubleClick; } }

        public static bool operator ==(Button A, ButtonState state)
        {
            return A.State == state;
        }

        public static bool operator !=(Button A, ButtonState state)
        {
            return A.State != state;
        }
    }

    public class MouseState
    {
        Point position = new Point(0, 0);
        public Point Position { get { return position; } set { position = value; } }

        internal Button lb, rb;
        internal Wheel wh;
        public Button LeftButton { get { return lb; } }
        public Button RightButton { get { return rb; } }

        public Wheel Wheel { get { return wh; } }

        public MouseState()
        {
            lb = new Button();
            rb = new Button();
            wh = new Wheel();
        }
    }
}
