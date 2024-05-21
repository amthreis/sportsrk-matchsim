using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futzim
{
    public partial class App : Node
    {
        public static void Log(object value)
        {
#if DEBUG
            GD.Print(value);
#else
            Console.WriteLine(value);
#endif
        }

        RandomNumberGenerator rng;

        public App() 
        {
            rng = new RandomNumberGenerator();
            rng.Randomize();

            instance = this;
        }

        public static RandomNumberGenerator RNG => instance.rng;

        public static App instance;
    }
}
