using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.IO;

namespace aclib
{

    public enum ConsoleMode
    {
        ENABLE_ECHO_INPUT = 0x0004,
        ENABLE_EXTENDED_FLAGS = 0x0080,
        ENABLE_INSERT_MODE = 0x0020,
        ENABLE_LINE_INPUT = 0x0002,
        ENABLE_MOUSE_INPUT = 0x0010,
        ENABLE_PROCESSED_INPUT = 0x0001,
        ENABLE_QUICK_EDIT_MODE = 0x0040,
        ENABLE_WINDOW_INPUT = 0x0008,
    }

    public enum ConsoleHandle
    {
        INPUT = -10,
        OUTPUT = -11,
        ERR = -12,
    }

    public class AdvConsole
    {
        static SafeFileHandle consoleOutput;
        static SafeFileHandle consoleInput;
        static int consoleWidth;
        static int consoleHeight;

        public static int ScreenWidth { get { return consoleWidth; } }
        public static int ScreenHeight { get { return consoleHeight; } }

        static Extern.CharInfo[] backBuffer;

        //static Point mousePosition = new Point(0, 0);
        static MouseState mouseState;
        public static MouseState MouseState { get { return mouseState; } }

        static List<KeyInfo> keys;

        private static string title;
        public static string Title { get { return title; } set { title = value; Extern.SetConsoleTitle(value); } }

        public static void Init() { Init("Advanced Console", 100, 40); }

        /// <summary>
        /// Initializes the Console window to allow the use of AdvConsole
        /// </summary>
        /// <param name="width">Console Screen Width (in characters)</param>
        /// <param name="height">Console Screen Height (in characters)</param>
        public static void Init(int width, int height) { Init("Advanced Console", width, height); }
        /// <summary>
        /// Initializes the Console window to allow the use of AdvConsole
        /// </summary>
        /// <param name="title">Console Screen Title</param>
        /// <param name="width">Console Screen Width (in characters)</param>
        /// <param name="height">Console Screen Height (in characters)</param>
        public static void Init(string title, int width, int height)
        {
            consoleOutput = new SafeFileHandle(Extern.GetStdHandle((int)ConsoleHandle.OUTPUT), true);
            consoleInput = new SafeFileHandle(Extern.GetStdHandle((int)ConsoleHandle.INPUT), true);

            Console.CursorVisible = false;

            //This is equivalent to Console.OutputEncoding = Encoding.Unicode
            //This ensures a failsafe way of setting the output to Unicode
            Console.SetOut(new StreamWriter(new FileStream(consoleOutput, FileAccess.Write), Encoding.Unicode));
            //Console.BufferWidth = width;
            //Console.BufferHeight = height;
            Console.SetBufferSize(width, height);
            Console.SetWindowSize(width, height);

            consoleWidth = width;
            consoleHeight = height;

            backBuffer = new Extern.CharInfo[width * height];

            Extern.SetConsoleTitle(title);

            //Block Control calls so they can be handled by the AdvConsole implementation
            Extern.SetConsoleCtrlHandler(new Extern.ConsoleCtrlDelegate(delegate(Extern.CtrlTypes t) { return true; }), true);

            keys = new List<KeyInfo>();

            mouseState = new MouseState();
        }

        public static void SetConsoleMode(ConsoleHandle bufferHandle, ConsoleMode mode)
        {
            if(bufferHandle == ConsoleHandle.OUTPUT)
                Extern.SetConsoleMode(consoleOutput, (byte)mode);
            if (bufferHandle == ConsoleHandle.INPUT)
                Extern.SetConsoleMode(consoleInput, (byte)mode);
        }

        public static void Write(string text, Point position, AdvConsoleColor foreColor, AdvConsoleColor backColor = AdvConsoleColor.None) { Write(text, position.X, position.Y, foreColor, backColor); }
        public static void Write(string text, int x, int y, AdvConsoleColor foreColor, AdvConsoleColor backColor = AdvConsoleColor.None)
        {
            Blit(ConstructStringBuffer(text), x, y, foreColor, backColor);
        }

        public static void WriteList(List<string> text, Point position, bool horizontal, AdvConsoleColor foreColor, AdvConsoleColor backColor) { WriteList(text.ToArray(), position.X, position.Y, horizontal, foreColor, backColor); }
        public static void WriteList(string[] text, Point position, bool horizontal, AdvConsoleColor foreColor, AdvConsoleColor backColor) { WriteList(text, position.X, position.Y, horizontal, foreColor, backColor); }
        public static void WriteList(string[] text, int x, int y, bool horizontal, AdvConsoleColor foreColor, AdvConsoleColor backColor)
        {
            int lineX = x;
            int lineY = y;

            //int lastLineLength = 0;

            for (int s = 0; s < (text.Length); s++)
            {
                char[,] line = ConstructStringBuffer(text[s]);

                Blit(line, lineX, lineY, foreColor, backColor);

                if (horizontal)
                    lineX += text[s].Length;
                else
                    lineY += 1 + line.GetUpperBound(0);
            }
        }

        //Converts a string to a multi-dimensional character array (splits by line breaks)
        private static char[,] ConstructStringBuffer(string i)
        {
            char[,] o;

            if (i == null)
                return new char[,] { };

            int width, height;

            width = 0;
            string[] lines = i.Split('\n');
            height = lines.Length;

            foreach (string s in lines)
            {
                if ((s.Length) > width) width = s.Length;
            }

            o = new char[height, width];

            for(int l = 0; l < lines.Length; l++)
            {
                char[] chars = lines[l].ToCharArray();
                for (int x = 0; x < chars.Length; x++)
                    o[l, x] = chars[x];
            }

            return o;
        }

        public static void ReadInput()
        {
            keys.Clear();

            uint eventsCount;
            Extern.GetNumberOfConsoleInputEvents(consoleInput, out eventsCount);

            if (eventsCount > 0)
            {
                Extern.INPUT_RECORD[] recs = new Extern.INPUT_RECORD[eventsCount];
                uint reads;
                Extern.ReadConsoleInput(consoleInput, recs, (uint)5, out reads);

                //return recs;

                MouseState.LeftButton.state = ButtonState.Released;
                MouseState.RightButton.state = ButtonState.Released;

                foreach (Extern.INPUT_RECORD r in recs)
                {
                    switch(r.EventType)
                    {
                        case 0x0001: //Keyboard input
                            keys.Add(
                                new KeyInfo(
                                    new CharacterInfo()
                                    {
                                        KeyDown = r.KeyEvent.bKeyDown,
                                        KeyCode = r.KeyEvent.wVirtualKeyCode,
                                        AsciiChar = (char)r.KeyEvent.uChar.AsciiChar,
                                        UnicodeChar = (char)r.KeyEvent.uChar.UnicodeChar,
                                        ControlKeyState = r.KeyEvent.dwControlKeyState
                                    }));
                            break;
                            
                        case 0x0002: //Mouse input
                            UpdateMouseState(r);
                        break;
                    }
                }
            }
            //else
                //return null;
        }

        /// <summary>
        /// Gets an array of KeyInfo for keys pressed since the last ReadInput call which exclude special keys
        /// </summary>
        /// <returns>Input Keys</returns>
        public static KeyInfo[] GetTextInputKeys()
        {
            List<KeyInfo> characters = new List<KeyInfo>();

            foreach (KeyInfo c in keys)
            {
                if ((byte)c.Character != '\0' && c.IsPressed)
                {
                    characters.Add(c);
                }
            }

            return characters.ToArray();
        }

        /// <summary>
        /// Gets an array of KeyInfo for all keys pressed since the last ReadInput call
        /// </summary>
        /// <returns>Input Keys</returns>
        public static KeyInfo[] GetInputKeys()
        {
            return keys.ToArray();
        }

        public static bool IsKeyDown(Keys key)
        {
            return keys.Exists(i => i.KeyCode == key && i.IsPressed == true);
        }

        private static void UpdateMouseState(Extern.INPUT_RECORD record)
        {
            if (record.EventType != 0x0002)
                return;

            Extern.MOUSE_EVENT_RECORD mouseEvent = record.MouseEvent;

            switch (mouseEvent.dwEventFlags)
            {
                default:
                    if (mouseEvent.dwButtonState == 0x0001)
                    {
                        if(mouseEvent.dwEventFlags == 0x0002)
                            MouseState.lb.doubleClick = true;

                        MouseState.lb.state = ButtonState.Pressed;
                    }
                    else if (mouseEvent.dwButtonState == 0x0002)
                    {
                        if (mouseEvent.dwEventFlags == 0x0002)
                            MouseState.rb.doubleClick = true;

                        MouseState.rb.state = ButtonState.Pressed;
                    }
                break;

                case 0x0001:
                    MouseState.Position = new Point(mouseEvent.dwMousePosition.X, mouseEvent.dwMousePosition.Y);
                break;
            }
        }

        #region Fill
        public static void Fill(Rectangle area, char character, AdvConsoleColor foreColor, AdvConsoleColor backColor) { Fill(area.X, area.Y, area.Width, area.Height, character, foreColor, backColor); }
        public static void Fill(int x, int y, int width, int height, char character, AdvConsoleColor foreColor = AdvConsoleColor.None, AdvConsoleColor backColor = AdvConsoleColor.None)
        {
            for (int ly = 0; ly < height; ly++)
            {
                for (int lx = 0; lx < width; lx++)
                {
                    int bufferIndex = ((x + lx) + ((y * consoleWidth) + (ly * consoleWidth)));

                    if (bufferIndex < backBuffer.Length)
                    {
                        SetCharacter(bufferIndex, character, foreColor, backColor);
                    }
                }
            }
        }
        #endregion

        #region Outline
        public static void Outline(Rectangle area, char character, AdvConsoleColor foreColor, AdvConsoleColor backColor = AdvConsoleColor.None) { Outline(area.X, area.Y, area.Width, area.Height, character, foreColor, backColor); }
        public static void Outline(int x, int y, int width, int height, char character, AdvConsoleColor foreColor, AdvConsoleColor backColor = AdvConsoleColor.None) { Outline(x, y, width, height, character, character, character, character, character, character, foreColor, backColor); }

        public static void Outline(Rectangle area, char topCharacter, char sideCharacter, AdvConsoleColor foreColor, AdvConsoleColor backColor = AdvConsoleColor.None) { Outline(area.X, area.Y, area.Width, area.Height, topCharacter, sideCharacter, topCharacter, sideCharacter, topCharacter, sideCharacter, foreColor, backColor); }
        public static void Outline(int x, int y, int width, int height, char topCharacter, char sideCharacter, AdvConsoleColor foreColor, AdvConsoleColor backColor = AdvConsoleColor.None) { Outline(x, y, width, height, topCharacter, sideCharacter, topCharacter, sideCharacter, topCharacter, sideCharacter, foreColor, backColor); }

        public static void Outline(Rectangle area, char topCharacter, char sideCharacter, char topLeftCharacter, char topRightCharacter, char bottomLeftCharacter, char bottomRightCharacter, AdvConsoleColor foreColor, AdvConsoleColor backColor = AdvConsoleColor.None) { Outline(area.X, area.Y, area.Width, area.Height, topCharacter, sideCharacter, topLeftCharacter, topRightCharacter, bottomLeftCharacter, bottomRightCharacter, foreColor, backColor); }
        public static void Outline(int x, int y, int width, int height, char topCharacter, char sideCharacter, char topLeftCharacter, char topRightCharacter, char bottomLeftCharacter, char bottomRightCharacter, AdvConsoleColor foreColor, AdvConsoleColor backColor = AdvConsoleColor.None)
        {
            for (int ly = 0; ly < height; ly++)
            {
                for (int lx = 0; lx < width; lx++)
                {
                    if ((lx > 0 && lx < (width - 1)) && (ly > 0 && ly < (height - 1)))
                        continue;

                    int bufferIndex = ((x + lx) + ((y * consoleWidth) + (ly * consoleWidth)));

                    if (bufferIndex < backBuffer.Length)
                    {
                        if (ly <= 0 || ly >= (height - 1))
                        {
                            char character;
                            if (lx >= (width - 1) && ly <= 0)
                                character = topRightCharacter;
                            else if (lx >= (width - 1) && ly >= (height - 1))
                                character = bottomRightCharacter;
                            else if (lx <= 0 && ly <= 0)
                                character = topLeftCharacter;
                            else if (lx <= 0 && ly >= (height - 1))
                                character = bottomLeftCharacter;
                            else
                                character = topCharacter;

                            SetCharacter(bufferIndex, character, foreColor, backColor);
                        }
                        else if  (lx <= 0 || lx >= (width - 1))
                        {
                            SetCharacter(bufferIndex, sideCharacter, foreColor, backColor);
                        }
                    }
                }
            }
        }
        #endregion

        #region Blit
        public static void Blit(char[,] inBuffer, Point position, AdvConsoleColor foreColor, AdvConsoleColor backColor) { Blit(inBuffer, position.X, position.Y, foreColor, backColor); }
        public static void Blit(char[,] inBuffer, int x, int y, AdvConsoleColor foreColor = AdvConsoleColor.None, AdvConsoleColor backColor = AdvConsoleColor.None)
        {
            int width = inBuffer.GetLength(1);
            int height = inBuffer.GetLength(0);

            for (int ly = 0; ly < height; ly++)
            {
                for (int lx = 0; lx < width; lx++)
                {
                    if ((x + lx) > consoleWidth - 1 || (y + ly) > consoleHeight - 1)
                        continue;

                    int bufferIndex = ((x + lx) + ((y * consoleWidth) + (ly * consoleWidth)));

                    if (bufferIndex >= 0 && bufferIndex < backBuffer.Length)
                    {
                        SetCharacter(bufferIndex, inBuffer[ly, lx], foreColor, backColor);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Sets the info for a character at the specified index in the buffer
        /// </summary>
        /// <param name="index">Index of the buffer</param>
        /// <param name="character">Character to write</param>
        /// <param name="foreColor">Foreground Color</param>
        /// <param name="backColor">Background Color</param>
        public static void SetCharacter(int index, char character, AdvConsoleColor foreColor, AdvConsoleColor backColor)
        {
            backBuffer[index].Char.UnicodeChar = character;

            byte foreCol = (byte)foreColor;
            byte backCol = (byte)backColor;

            if (backColor == AdvConsoleColor.None)
            {
                byte fc = (byte)(backBuffer[index].Attributes % 16);
                backCol = (byte)((backBuffer[index].Attributes - fc) / 16);
            }

            if (foreColor == AdvConsoleColor.None)
            {
                foreCol = (byte)(backBuffer[index].Attributes % 16);
            }

            backBuffer[index].Attributes = (byte)((byte)(foreCol) + (byte)((int)backCol * 16));
        }

        /// <summary>
        /// Removes text from the entire screen and sets the background color
        /// </summary>
        /// <param name="backColor">Background Color</param>
        public static void ClearScreen(AdvConsoleColor backColor)
        {
            for (int i = 0; i < backBuffer.Count(); i++)
            {
                backBuffer[i].Char.UnicodeChar = ('\0');
                backBuffer[i].Attributes = (byte)((int)backColor * 16);
            }
        }

        /// <summary>
        /// Writes the output buffer to the console window.
        /// </summary>
        public static void SwapBuffers()
        {
            Extern.SmallRect r = new Extern.SmallRect() { Top = (short)0, Left = (short)0, Bottom = (short)consoleHeight, Right = (short)consoleWidth };
            Extern.WriteConsoleOutputW(
                consoleOutput,
                backBuffer,
                new Extern.Coord() { X = (short)consoleWidth, Y = (short)consoleHeight },
                new Extern.Coord() { X = (short)0, Y = (short)0 }, ref r);
        }
    }
}
