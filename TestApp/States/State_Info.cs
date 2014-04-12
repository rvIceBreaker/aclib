using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using aclib;

namespace TestApp.States
{
    class State_Info : State
    {
        string text = "ACLib Test Application\n\n" +
            "This is the Advanced Console Library Test Application!\n" +
            "Here you'll find various demos of the functionality of aclib.\n" +
            "Press the key to see a certain demo\n\n\n";

        public State_Info()
        {
            name = "This Screen";
            activationKeyCode = Keys.F1;
        }

        public override void Update()
        {
            text = "ACLib Test Application\n\n" +
            "This is the Advanced Console Library Test Application!\n" +
            "Here you'll find various demos of the functionality of aclib.\n" +
            "Press the key to see a certain demo\n\n\n";

            foreach (State s in Engine.states)
            {
                text += s.activationKeyCode.ToString() + " - " + s.name + "\n";
            }

            base.Update();
        }

        public override void Draw()
        {
            AdvConsole.ClearScreen(AdvConsoleColor.Black);

            AdvConsole.Write(text,
                new Point(10, 6),
                AdvConsoleColor.Gray);

            AdvConsole.SwapBuffers();

            base.Draw();
        }
    }
}
