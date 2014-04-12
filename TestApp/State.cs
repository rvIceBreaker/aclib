using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using aclib;

namespace TestApp
{
    class State
    {
        public Keys activationKeyCode;
        public string name;

        public State() { }

        public virtual void Update() { }
        public virtual void Draw() { }
    }
}
