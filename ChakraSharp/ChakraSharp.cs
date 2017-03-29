namespace ChakraSharp
{
    using CppSharp;
    using CppSharp.AST;
    using CppSharp.Generators;
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class ChakraSharp : ILibrary
    {
        private readonly ChakraInfo m_chakraInfo;
        private readonly XmlDefinitionTranslationPass m_xmlExportPass;

        public ChakraSharp(ChakraInfo chakraInfo)
        {
            if (chakraInfo == null)
                throw new ArgumentNullException(nameof(chakraInfo));

            m_chakraInfo = chakraInfo;
            m_xmlExportPass = new XmlDefinitionTranslationPass();
        }

        /// <summary>
        /// Gets the XDocument of the xml representation.
        /// </summary>
        public XDocument XmlExport
        {
            get
            {
                return m_xmlExportPass.Document;
            }
        }

        /// <summary>
        /// Sets the driver options. First method called.
        /// </summary>
        /// <param name="driver"></param>
        public void Setup(Driver driver)
        {
            var options = driver.Options;
            options.GeneratorKind = GeneratorKind.CSharp;
            options.GenerateFinalizers = true;

            var chakraSharpModule = options.AddModule("ChakraSharp");
            chakraSharpModule.OutputNamespace = "ChakraSharp";
            chakraSharpModule.LibraryName = "ChakraCore";

            chakraSharpModule.Defines.Add("_AMD64_");
            chakraSharpModule.Defines.Add("BIT64");

            chakraSharpModule.Undefines.Add("_WIN32");

            chakraSharpModule.Headers.Add(Path.Combine(m_chakraInfo.ChakraPath, @"lib\Jsrt\ChakraCore.h"));
        }

        /// <summary>
        /// Setup passes. Second method called.
        /// </summary>
        /// <param name="driver"></param>
        public void SetupPasses(Driver driver)
        {
            driver.AddTranslationUnitPass(new FixOutParamsPass());
            driver.AddTranslationUnitPass(m_xmlExportPass);
            driver.AddTranslationUnitPass(new FixObjectNamesPass());
            driver.AddTranslationUnitPass(new FixCommentsPass());
        }

        /// <summary>
        /// Do transformations that should happen before any passes are processed.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="ctx"></param>
        public void Preprocess(Driver driver, ASTContext ctx)
        {
        }

        /// <summary>
        /// Do transformations that should happen after all passes are processed.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="ctx"></param>
        public void Postprocess(Driver driver, ASTContext ctx)
        {
        }
    }
}
