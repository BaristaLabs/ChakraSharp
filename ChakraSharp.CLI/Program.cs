namespace ChakraSharp.CLI
{
    using CppSharp;
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var chakraInfo = new ChakraInfo
            {
                ChakraPath = @"C:\Projects\ChakraCore"
            };

            ConsoleDriver.Run(new ChakraSharp(chakraInfo));

            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}
