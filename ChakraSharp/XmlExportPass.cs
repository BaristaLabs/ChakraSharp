namespace ChakraSharp
{
    using CppSharp.AST;
    using CppSharp.Passes;
    using System.Linq;
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

            var descriptionElement = new XElement("Description");
            descriptionElement.Add(new XText("\r\n"));
            descriptionElement.Add(new XCData("\r\n" + decl.Comment.Text + "\r\n"));
            descriptionElement.Add(new XText("\r\n    "));
            exportElement.Add(descriptionElement);

            var parametersElement = new XElement("Parameters");
            foreach(var param in decl.Parameters)
            {
                var isOut = false;
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
                        isOut = true;
                        parameterElement.SetAttributeValue("type", MapType(param, pointerTypeDef.Declaration.ToString(), ref isOut));
                    }
                    else
                    {
                        //Fallback to the original qualified type.
                        parameterElement.SetAttributeValue("type", MapType(param, param.QualifiedType.ToString(), ref isOut));
                    }
                }
                else
                    parameterElement.SetAttributeValue("type", MapType(param, param.QualifiedType.ToString(), ref isOut));

                
                parameterElement.SetAttributeValue("name", param.Name);

                if (param.IsOut || isOut)
                    parameterElement.SetAttributeValue("direction", "Out");

                parametersElement.Add(parameterElement);
            }
            exportElement.Add(parametersElement);

            if (m_root.Elements().Where(e => e.Attribute("source").Value == exportElement.Attribute("source").Value).Count() == 0)
                m_root.Add(new XComment("\r\n  ***************************************\r\n  **\r\n  ** " + exportElement.Attribute("source").Value + "\r\n  **\r\n  ***************************************\r\n  "));

            m_root.Add(exportElement);
            return false;
        }

        private string MapType(Parameter param, string type, ref bool isOut)
        {
            type = type.Replace("global::System.", "");

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
            }

            if (type == "uint16_t" && param.DebugText == "uint16_t* buffer")
            {
                isOut = false;
                type = "uint16_t*";
            }

            if (type == "uint16_t" && param.DebugText == "const uint16_t *content")
            {
                isOut = false;
                type = "string";
            }

            return type;
        }
    }
}
