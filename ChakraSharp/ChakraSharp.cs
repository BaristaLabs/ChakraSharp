namespace ChakraSharp
{
    using System;
    using CppSharp;
    using CppSharp.AST;
    using CppSharp.Generators;
    using System.IO;

    public class ChakraSharp : ILibrary
    {
        private readonly ChakraInfo m_chakraInfo;

        public ChakraSharp(ChakraInfo chakraInfo)
        {
            if (chakraInfo == null)
                throw new ArgumentNullException(nameof(chakraInfo));

            m_chakraInfo = chakraInfo;
        }

        public void Postprocess(Driver driver, ASTContext ctx)
        {
            //throw new NotImplementedException();
        }

        public void Preprocess(Driver driver, ASTContext ctx)
        {
            //throw new NotImplementedException();
        }

        public void SetupPasses(Driver driver)
        {
            //throw new NotImplementedException();
        }

        private string GetPath(string relativePath)
        {
            return Path.Combine(m_chakraInfo.ChakraPath, relativePath);
        }

        public void Setup(Driver driver)
        {
            var options = driver.Options;
            options.GeneratorKind = GeneratorKind.CSharp;
            options.OutputNamespace = "ChakraSharp";
            options.LibraryName = "ChakraCore";

            options.MainModule.Undefines.Add("_WIN32");
            options.Headers.Add(Path.Combine(m_chakraInfo.ChakraPath, @"lib\Jsrt\ChakraCore.h"));
        }
    }
}
