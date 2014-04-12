using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using aclib;

namespace TestApp.States
{
    class State_TextInput : State
    {
        string description = "This is the Text Input demo!\n\nThis demo uses GetTextInputKeys() to get inputs common for standard typing.\nProcessing input is done without locking the thread.\nThis can be useful for things like chat boxes, as it disregards non-text key presses.";
        string[] inputLine = new string[20];
        int currentLine = 0;

        int maxLineLength = 75;

        int[] colors = new int[22];

        Point textPosition = new Point(10, 10);

        float nextColorShiftTime;
        float colorShiftDelay = 0.05f;

        public State_TextInput()
        {
            name = "Text Input";
            activationKeyCode = aclib.Keys.F2;

            for (int i = 0; i < inputLine.Length; i++) { inputLine[i] = ""; }

            for (int i = 0; i < colors.Length; i++) { colors[i] = i % 8; }
        }

        public override void Update()
        {
            //Get an array of text input keys for this frame
            KeyInfo[] chars = AdvConsole.GetTextInputKeys();

            //Handle our characters...
            foreach (KeyInfo c in chars)
            {
                if (c.KeyCode == Keys.Return) //If the user enters a Return key (enter), we shift to the next line in the array
                {
                    if (currentLine < (inputLine.Length - 1))
                        currentLine++;

                    continue;
                }

                if (c.KeyCode == Keys.Back) //If the user enters a Backspace, we remove the last character on our current line
                {
                    if (inputLine[currentLine].Length > 0) //If the line is longer than one character, we remove a character
                        inputLine[currentLine] = inputLine[currentLine].Remove(inputLine[currentLine].Length - 1);
                    else if(currentLine > 0) //If the line isn't longer than one character, and we're not on the first line, we back up to the previous line
                        currentLine--;

                    continue;
                }

                if (c.KeyCode == Keys.Tab) //If the user enters a Tab, we insert 3 spaces at once
                {
                    if ((inputLine[currentLine].Length) + 3 > maxLineLength) //Check if we can afford to append 3 characters at once
                    {
                        int length = maxLineLength - inputLine[currentLine].Length; //Get the number of characters available until the line will be filled
                        for (int i = 0; i < length; i++) { inputLine[currentLine] += ' '; } //Append a space in loop
                    }
                    else
                        inputLine[currentLine] += "   "; //Append the space block

                    continue;
                }

                //If the user didn't enter a special key above, its safe to assume its a normal text character, so we append it to the current line
                if (inputLine[currentLine].Length < maxLineLength && !c.KeyState.Ctrl) //Make sure our line isn't full and the control key isn't down
                    inputLine[currentLine] += c.Character;
            }

            base.Update();
        }

        public override void Draw()
        {
            //Clear the screen with a dark blue background
            AdvConsole.ClearScreen(AdvConsoleColor.DarkBlue);

            for (int i = 0; i < colors.Length; i++)
            {
                //Draw a row of color (no text, colored background)
                AdvConsole.Fill(new Rectangle(textPosition - new Point(2, 1) + new Point(0, i), maxLineLength + 4, 1), ' ', AdvConsoleColor.Yellow, (AdvConsoleColor)colors[i]);

                if (Engine.elapsedTime > nextColorShiftTime) //Limit the time between swapping colors, so we don't give everyone an extreme case of epilepsy
                {
                    colors[i] = colors[i] + 1 % 16; //Shift our color
                }
            }

            if (Engine.elapsedTime > nextColorShiftTime) //Sort of sloppy, but it makes sure we swap all color rows before resetting the delay
            {
                nextColorShiftTime = Engine.elapsedTime + colorShiftDelay; //Reset the delay by our delay amount
            }

            //Re-Clear the text input area so its not a mess of colors
            AdvConsole.Fill(new Rectangle(textPosition, maxLineLength, inputLine.Length), ' ', AdvConsoleColor.Yellow, AdvConsoleColor.DarkBlue);

            //Display our description string at the top of the screen
            AdvConsole.Write(description, new Point(10, 2), AdvConsoleColor.White);

            //Draw an outline around the input lines
            AdvConsole.Outline(new Rectangle(textPosition - new Point(1, 1), maxLineLength + 2, inputLine.Length + 2), '░', AdvConsoleColor.Blue, AdvConsoleColor.DarkBlue);

            //Draw text input cursor
            //This line is optional. You're not required to draw anything, but I like to show where the current text cursor is.
            AdvConsole.Write("▬", textPosition + new Point(inputLine[currentLine].Length, currentLine), AdvConsoleColor.Yellow);

            //Draw all of our input lines
            for(int i = 0; i < inputLine.Length; i++)
            {
                string s = inputLine[i];

                if (i == currentLine)
                    AdvConsole.Fill(new Rectangle(textPosition + new Point(0, i), maxLineLength, 1), ' ', AdvConsoleColor.Yellow, AdvConsoleColor.Blue);

                AdvConsole.Write(s, textPosition + new Point(0, i), AdvConsoleColor.Yellow);
            }

            //Swap the buffers and display our changes to the screen
            AdvConsole.SwapBuffers();

            base.Draw();
        }
    }
}
