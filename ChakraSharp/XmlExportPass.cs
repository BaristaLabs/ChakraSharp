namespace ChakraSharp
{
    using CppSharp.AST;
    using CppSharp.Passes;
    using System.Xml.Linq;

    public class XmlExportPass : TranslationUnitPass
    {
        private readonly XDocument m_document;
        private readonly XElement m_root;

        public XmlExportPass()
        {
            m_document = new XDocument();
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
            

            return false;
        }
    }
}
