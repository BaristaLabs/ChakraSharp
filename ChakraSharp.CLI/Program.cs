namespace ChakraSharp.CLI
{
    using CppSharp;
    using System;
    using System.Text;
    using System.Xml;

    class Program
    {
        static void Main(string[] args)
        {
            var chakraInfo = new ChakraInfo
            {
                ChakraPath = @"C:\Projects\ChakraCore"
            };

            var chakraSharp = new ChakraSharp(chakraInfo);
            ConsoleDriver.Run(chakraSharp);

            var settings = new XmlWriterSettings()
            {
                Indent = true
            };

            using (var writer = XmlWriter.Create("ChakraExternDefinitions.xml", settings))
            {
                chakraSharp.XmlExport.WriteTo(writer);
            }
            
            Console.WriteLine("Generated 'ChakraExternDefinitions.xml'");
            Console.WriteLine("Press enter to continue...");
            Console.ReadLine();
        }
    }
}
