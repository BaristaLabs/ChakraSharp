namespace ChakraSharp
{
    using CppSharp;
    using CppSharp.AST;
    using CppSharp.Generators;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;

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
            foreach (var translationUnit in ctx.TranslationUnits.Where(tu => tu.IsValid == true && tu.FileName.StartsWith("Chakra")))
            {
                foreach (var declaration in translationUnit.Declarations)
                {
                    if (declaration.Comment != null)
                        declaration.Comment = CreateCommentFromXml(declaration.QualifiedLogicalName, declaration.Comment.Text);

                    var enumDecl = declaration as Enumeration;
                    if (enumDecl != null)
                    {
                        foreach(var item in enumDecl.Items)
                        {
                            if (item.Comment != null)
                                item.Comment = CreateCommentFromXml(item.QualifiedLogicalName, item.Comment.Text);
                        }
                    }
                }
            }
        }

        private RawComment CreateCommentFromXml(string parentQualifiedName, string descriptionXml)
        {
            var rc = new RawComment();

            if (string.IsNullOrWhiteSpace(descriptionXml))
                return rc;

            var descriptionXmlBuilder = new StringBuilder();
            descriptionXmlBuilder.AppendLine("<description>");
            Regex rx = new Regex(@"///\s*?(?<text>.*)", RegexOptions.ExplicitCapture);
            foreach (Match match in rx.Matches(descriptionXml))
            {
                var text = match.Groups["text"].Value.Trim();
                text = text.Replace("< ", "&lt; ");
                text = text.Replace(" >", " &gt;");
                descriptionXmlBuilder.AppendLine(text);
            }
            descriptionXmlBuilder.AppendLine("</description>");

            try
            {
                var descriptionXDoc = XDocument.Parse(descriptionXmlBuilder.ToString());
                if (descriptionXDoc.Root.Element("summary") != null)
                    rc.BriefText = descriptionXDoc.Root.Element("summary").Value.Trim().Replace("\n", " ").Replace("\r\n", " ");
            }
            catch
            {
                //Do nothing.
            }

            //if (descriptionXDoc.Root.Element("remarks") != null)
            //    rc.Text = descriptionXDoc.Root.Element("remarks").Value.Trim();

            //if (descriptionXDoc.Root.Element("returns") != null)
            //    def.ReturnParameter.Description = descriptionXDoc.Root.Element("returns").Value.Trim();
            //Dictionary<string, string> paramDescriptions = new Dictionary<string, string>();
            //foreach (var parm in descriptionXDoc.Root.Elements("param"))
            //{
            //    var paramName = parm.Attribute("name").Value.Trim();
            //    var paramDescription = parm.Value.Trim();
            //    paramDescriptions.Add(paramName, paramDescription);
            //}

            return rc;
        }

        public void Preprocess(Driver driver, ASTContext ctx)
        {
        }

        public void SetupPasses(Driver driver)
        {
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
            options.UnityBuild = true;
            options.GenerateFinalizers = true;
            options.MarshalCharAsManagedChar = true;

            options.MainModule.Defines.Add("_AMD64_");
            options.MainModule.Defines.Add("BIT64");

            options.MainModule.Undefines.Add("_WIN32");
            
            options.Headers.Add(Path.Combine(m_chakraInfo.ChakraPath, @"lib\Jsrt\ChakraCore.h"));
        }
    }
}
