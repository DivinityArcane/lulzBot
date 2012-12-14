using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace lulzBot
{
    class Program
    {
        static void Main(string[] args)
        {
            // Let's just do sort of a Hello, World! type thing here.
            // We're going to ask their name, then say hi.
            String Name = String.Empty;

            // Console.Write outputs the line with no line ending. 
            // Console.WriteLine outputs the line appended with a line ending.
            Console.Write("Hi there! What's your name?: ");
            Name = Console.ReadLine();

            // OK! Let's say hi! :)
            // Write and WriteLine kinda work like String.Format. We use {0},
            //  {1}, {2}, etc. Like Python.
            Console.WriteLine("Well, hello there, {0}!", Name);

            // That's all!
            Console.WriteLine("Press any key to close this window...");
            Console.ReadKey();


            // Press F5 to compile and run this.
        }
    }
}
