using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using aclib;
using System.Diagnostics;
using TestApp.States;

namespace TestApp
{
    class Engine
    {
        public bool IsRunning = true;

        public static List<State> states;
        int activeStateIndex;

        public static float lastFrameDuration, elapsedTime;

        public Engine()
        {
            AdvConsole.Init("Advanced Console Libarary Test App", 100, 40);

            states = new List<State>();
            activeStateIndex = 0;

            states.Add(new State_Info());
            states.Add(new State_TextInput());
        }

        public void Run()
        {
            while (IsRunning)
            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                Update();
                Draw();

                watch.Stop();
                lastFrameDuration = (float)watch.Elapsed.TotalSeconds;
                elapsedTime += lastFrameDuration;

                AdvConsole.Title = "Advanced Console Libarary Test App - " + (1 / lastFrameDuration) + " fps";
            }
        }

        public void Draw()
        {
            states[activeStateIndex].Draw();
        }

        public void Update()
        {
            AdvConsole.ReadInput();

            states[activeStateIndex].Update();

            foreach (State s in states)
            {
                if (AdvConsole.IsKeyDown(s.activationKeyCode))
                {
                    activeStateIndex = states.IndexOf(s);
                }
            }
        }
    }
}
