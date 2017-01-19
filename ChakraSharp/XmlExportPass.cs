namespace ChakraSharp
{
    using CppSharp.AST;
    using CppSharp.Passes;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    public class XmlExportPass : TranslationUnitPass
    {
        private readonly XDocument m_document;
        private readonly XElement m_root;

        public XDocument Document
        {
            get { return m_document; }
        }

        public XmlExportPass()
        {
            m_document = new XDocument();
            m_document.Declaration = new XDeclaration("1.0", "UTF-8", "yes");
            m_root = new XElement("ChakraDefinitions");
            m_document.Add(m_root);
        }

        public override bool VisitFunctionDecl(Function decl)
        {
            if (!VisitDeclaration(decl))
                return false;

            if (ASTUtils.CheckIgnoreFunction(decl, Options))
                return false;

            if (!decl.TranslationUnit.FileName.StartsWith("Chakra"))
                return false;

            var exportElement = new XElement("Export");
            exportElement.SetAttributeValue("name", decl.Name);
            exportElement.SetAttributeValue("target", "common");
            exportElement.SetAttributeValue("source", decl.TranslationUnit.FileName);

            var documentationElement = new XElement("Description");
            var descriptionXmlBuilder = new StringBuilder();
            Regex rx = new Regex(@"///\s*?(?<text>.*)", RegexOptions.ExplicitCapture);
            foreach (Match match in rx.Matches(decl.Comment.Text))
            {
                var text = match.Groups["text"].Value.Trim();
                descriptionXmlBuilder.AppendLine(text);
            }
            documentationElement.Add(new XCData(descriptionXmlBuilder.ToString()));
            exportElement.Add(documentationElement);

            var parametersElement = new XElement("Parameters");
            var isOut = false;
            foreach(var param in decl.Parameters)
            {
                var parameterElement = new XElement("Parameter");

                var typeDef = param.Type as TypedefType;
                var pointer = param.Type as PointerType;

                if (typeDef != null)
                    parameterElement.SetAttributeValue("type", typeDef.Declaration.QualifiedName);
                else if (pointer != null)
                {
                    var pointerTypeDef = pointer.Pointee as TypedefType;
                    if (pointerTypeDef != null)
                    {
                        parameterElement.SetAttributeValue("type", pointerTypeDef.Declaration.ToString());
                        isOut = true;
                    }
                    else
                        parameterElement.SetAttributeValue("type", pointer.Pointee.ToString().Replace("global::System.", "") + "*");
                }
                else
                    parameterElement.SetAttributeValue("type", param.QualifiedType.ToString().Replace("global::System.", ""));

                
                parameterElement.SetAttributeValue("name", param.Name);

                if (param.IsOut || isOut)
                    parameterElement.SetAttributeValue("direction", "out");

                parametersElement.Add(parameterElement);
            }
            exportElement.Add(parametersElement);

            m_root.Add(exportElement);
            return false;
        }
    }
}
