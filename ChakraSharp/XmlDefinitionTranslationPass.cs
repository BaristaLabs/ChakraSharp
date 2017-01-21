namespace ChakraSharp
{
    using CppSharp.AST;
    using CppSharp.Passes;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;

    /// <summary>
    /// Pass that takes a snapshot of the current state of the AST and generates an Xml Definition
    /// </summary>
    public class XmlDefinitionTranslationPass : TranslationUnitPass
    {
        private readonly Regex m_rx = new Regex(@"///(?<text>.*)", RegexOptions.ExplicitCapture | RegexOptions.Compiled);
        private readonly XDocument m_document;
        private readonly XElement m_root;

        public XDocument Document
        {
            get { return m_document; }
        }

        public XmlDefinitionTranslationPass()
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

            //Skip TTD functions for now.
            if (decl.Name.StartsWith("JsTTD"))
                return false;

            var exportElement = new XElement("Export");
            exportElement.SetAttributeValue("name", decl.Name);
            exportElement.SetAttributeValue("target", "Common");
            exportElement.SetAttributeValue("source", decl.TranslationUnit.FileName);

            //Manual attribute defs
            if (decl.Name == "JsCreateStringUtf16")
                exportElement.SetAttributeValue("dllImportEx", ", CharSet = CharSet.Unicode");
            else if (decl.Name == "JsCopyString")
                exportElement.SetAttributeValue("dllImportEx", ", CharSet = CharSet.Ansi");

            //Normalize Comments.
            var commentBuilder = new StringBuilder();
            foreach (Match match in m_rx.Matches(decl.Comment.Text))
            {
                var text = match.Groups["text"].Value;
                text = text.Replace("< 0", "&lt; 0");
                commentBuilder.Append("///" + text);
            }

            var descriptionElement = new XElement("Description");
            descriptionElement.Add(new XText("\r\n      "));
            descriptionElement.Add(new XCData("\r\n" + commentBuilder.ToString() + "\r\n"));
            descriptionElement.Add(new XText("\r\n    "));
            exportElement.Add(descriptionElement);

            //Output and map parameters
            var parametersElement = new XElement("Parameters");
            foreach(var param in decl.Parameters)
            {
                var isOut = false;
                var parameterElement = new XElement("Parameter");

                var typeDef = param.Type as TypedefType;
                var pointer = param.Type as PointerType;

                if (typeDef != null)
                    parameterElement.SetAttributeValue("type", MapParameterType(decl, param, typeDef.Declaration.QualifiedName, ref isOut));
                else if (pointer != null)
                {
                    var pointerTypeDef = pointer.Pointee as TypedefType;
                    if (pointerTypeDef != null)
                    {
                        isOut = true;
                        parameterElement.SetAttributeValue("type", MapParameterType(decl, param, pointerTypeDef.Declaration.ToString(), ref isOut));
                    }
                    else
                    {
                        //Fallback to the original qualified type.
                        parameterElement.SetAttributeValue("type", MapParameterType(decl, param, param.QualifiedType.ToString(), ref isOut));
                    }
                }
                else
                    parameterElement.SetAttributeValue("type", MapParameterType(decl, param, param.QualifiedType.ToString(), ref isOut));


                if (param.Name == "ref" || param.Name == "object")
                    parameterElement.SetAttributeValue("name", "@" + param.Name);
                else
                    parameterElement.SetAttributeValue("name", param.Name);

                if (param.IsOut || isOut)
                    parameterElement.SetAttributeValue("direction", "Out");

                parametersElement.Add(parameterElement);
            }

            if (parametersElement.HasElements)
                exportElement.Add(parametersElement);

            //Add breaks between Exports from different sources.
            if (m_root.Elements().Where(e => e.Attribute("source").Value == exportElement.Attribute("source").Value).Count() == 0)
                m_root.Add(new XComment("\r\n  ***************************************\r\n  **\r\n  ** " + exportElement.Attribute("source").Value + "\r\n  **\r\n  ***************************************\r\n  "));

            m_root.Add(exportElement);
            return false;
        }

        private string MapParameterType(Function decl, Parameter param, string type, ref bool isOut)
        {
            type = type.Replace("global::System.", "");

            //Some manual parameter type mappings.
            switch(type)
            {
                case "void**":
                    type = "IntPtr";
                    isOut = true;
                    break;
                case "BYTE":
                    type = "byte[]";
                    isOut = false;
                    break;
                case "ChakraBytePtr":
                    type = "IntPtr";
                    break;
                case "bool*":
                case "int*":
                case "uint*":
                case "double*":
                    type = type.Replace("*", "");
                    isOut = true;
                    break;
                case "JsThreadServiceCallback":
                case "JsMemoryAllocationCallback":
                case "JsBeforeCollectCallback":
                case "JsObjectBeforeCollectCallback":
                    type = type.Replace("Js", "JavaScript");
                    break;
            }

            if (type == "uint16_t" && param.DebugText == "uint16_t* buffer")
            {
                isOut = false;
                type = "uint16_t*";
            }
            else if (type == "uint16_t" && param.DebugText == "const uint16_t *content")
            {
                isOut = false;
                type = "string";
            }
            else if (type == "JsValueRef" && param.DebugText == "JsValueRef *arguments")
            {
                isOut = false;
                type = "JsValueRef[]";
            }
            else if (type == "JsRuntimeHandle" && decl.Name == "JsDisposeRuntime")
            {
                type = "IntPtr";
            }

            return type;
        }
    }
}
