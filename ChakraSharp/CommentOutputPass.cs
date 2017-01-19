namespace ChakraSharp
{
    using CppSharp.Generators;
    using CppSharp.Generators.CLI;
    using CppSharp.Passes;

    public class CommentOutputPass : GeneratorOutputPass
    {
        public override void VisitGeneratorOutput(GeneratorOutput output)
        {
            foreach (var template in output.Templates)
            {
                var blocks = template.FindBlocks(CLIBlockKind.Class);
                foreach (var block in blocks)
                    VisitMethodBody(block);
            }
        }

        void VisitMethodBody(Block block)
        {
            
        }
    }
}
