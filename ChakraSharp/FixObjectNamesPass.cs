namespace ChakraSharp
{
    using CppSharp.AST;
    using CppSharp.Passes;

    public class FixObjectNamesPass : TranslationUnitPass
    {
        public override bool VisitParameterDecl(Parameter parameter)
        {
            if (!VisitDeclaration(parameter))
                return false;

            //Rename parameters named "object" to "targetObject" as the CSharpGenerator has a hard time with these names in method bodies.
            if (parameter.Name == "object")
            {
                parameter.Name = "targetObject";
                return true;
            }
            return false;
        }
    }
}
