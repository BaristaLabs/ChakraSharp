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

        public void Postprocess(Driver driver, ASTContext ctx)
        {
            foreach (var translationUnit in ctx.TranslationUnits.Where(tu => tu.IsValid == true && tu.FileName.StartsWith("Chakra")))
            {
                //Fix comments on generated cs code.
                foreach (var declaration in translationUnit.Declarations)
                {
                    if (declaration.Comment != null)
                    {
                        var xDoc = GetOriginalDocumentationDocument(declaration.Comment.Text);
                        var xRoot = xDoc.Root;

                        declaration.Comment.Kind = RawCommentKind.BCPLSlash;
                        var fullComment = declaration.Comment.FullComment;
                        fullComment.Blocks.Clear();

                        var summaryPara = new ParagraphComment();
                        var summaryElement = xRoot.Element("summary");
                        summaryPara.Content.Add(new TextComment { Text = summaryElement == null ? "" : summaryElement.Value.ReplaceLineBreaks("").Trim() });
                        fullComment.Blocks.Add(summaryPara);

                        
                        var remarksElement = xRoot.Element("remarks");
                        if (remarksElement != null)
                        {
                            foreach (var remarksLine in remarksElement.Value.Split('\n'))
                            {
                                var remarksPara = new ParagraphComment();
                                remarksPara.Content.Add(new TextComment { Text = remarksLine.ReplaceLineBreaks("").Trim() });
                                fullComment.Blocks.Add(remarksPara);
                            }
                        }

                        var paramElements = xRoot.Elements("param");
                        foreach(var paramElement in paramElements)
                        {
                            var paramComment = new ParamCommandComment();
                            paramComment.Arguments.Add(new BlockCommandComment.Argument { Text = paramElement.Attribute("name").Value });
                            paramComment.ParagraphComment = new ParagraphComment();
                            paramComment.ParagraphComment.Content.Add(new TextComment { Text = paramElement.Value.ReplaceLineBreaks("") });
                            fullComment.Blocks.Add(paramComment);
                        }
                    }
                }
            }
        }

        private readonly Regex m_rx = new Regex(@"///(?<text>.*)", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

        private XDocument GetOriginalDocumentationDocument(string documentationText)
        {
            var descriptionXmlBuilder = new StringBuilder();
            descriptionXmlBuilder.AppendLine("<description>");
            foreach (Match match in m_rx.Matches(documentationText))
            {
                var text = match.Groups["text"].Value;
                text = text.Replace("< 0", "&lt; 0");
                text = text.Replace("<param name=\"referencingModule</param>", "<param name=\"referencingModule\"></param>");
                descriptionXmlBuilder.Append(text);
            }
            descriptionXmlBuilder.AppendLine("</description>");
            var descriptionXDoc = XDocument.Parse(descriptionXmlBuilder.ToString());
            return descriptionXDoc;
        }

        private RawComment CreateCommentFromXml(string parentQualifiedName, string descriptionXml)
        {
            var rc = new RawComment();
            rc.Kind = RawCommentKind.BCPLSlash;
            rc.FullComment = new FullComment();
            rc.BriefText = "foo";
            rc.Text = "foo";

            if (string.IsNullOrWhiteSpace(descriptionXml))
                return rc;

            var blockComment = new VerbatimBlockComment();
            blockComment.Lines.Add(new VerbatimBlockLineComment { Text = "foo" });
            rc.FullComment.Blocks.Add(blockComment);

            

            //try
            //{
            //    var descriptionXDoc = XDocument.Parse(descriptionXmlBuilder.ToString());
            //    if (descriptionXDoc.Root.Element("summary") != null)
            //        rc.BriefText = descriptionXDoc.Root.Element("summary").Value.Trim().Replace("\n", " ").Replace("\r\n", " ");
            //}
            //catch
            //{
            //    //Do nothing.
            //}

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
            driver.AddTranslationUnitPass(m_xmlExportPass);
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
