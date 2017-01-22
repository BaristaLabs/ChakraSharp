namespace ChakraSharp
{
    using CppSharp.AST;
    using CppSharp.Passes;
    using System.Linq;

    public class FixOutParamsPass : TranslationUnitPass
    {
        private static readonly string[] s_outParamMacros = new string[] { "_Out_", "_Outptr_result_maybenull_", "_Out_opt_", "_Outptr_result_bytebuffer_(*bufferLength)" };

        public override bool VisitParameterDecl(Parameter parameter)
        {
            if (!VisitDeclaration(parameter))
                return false;

            var expansions = parameter.PreprocessedEntities.OfType<MacroExpansion>();

            if (expansions.Any(e => s_outParamMacros.Contains(e.Text)))
            {
                if (parameter.DebugText != "uint16_t* buffer" && parameter.DebugText != "char* buffer")
                    parameter.Usage = ParameterUsage.Out;

                var currentQualifiedType = parameter.QualifiedType;
                var pointerType = currentQualifiedType.Type as PointerType;

                Type targetType;

                //There's probably a more elegant way to do this...
                switch(currentQualifiedType.ToString())
                {
                    case "void**":
                    case "global::System.IntPtr":
                        targetType = new BuiltinType(PrimitiveType.IntPtr);
                        break;
                    case "uint*":
                        targetType = new BuiltinType(PrimitiveType.UInt);
                        break;
                    case "int*":
                        targetType = new BuiltinType(PrimitiveType.Int);
                        break;
                    case "double*":
                        targetType = new BuiltinType(PrimitiveType.Double);
                        break;
                    case "bool*":
                        targetType = new BuiltinType(PrimitiveType.Bool);
                        break;
                    case "byte*":
                        targetType = new BuiltinType(PrimitiveType.IntPtr);
                        break;
                    case "sbyte*":
                        targetType = new BuiltinType(PrimitiveType.Char);
                        break;
                    case "ushort*":
                        targetType = new BuiltinType(PrimitiveType.UShort);
                        break;
                    case "long*":
                        targetType = new BuiltinType(PrimitiveType.Long);
                        break;
                    case "ulong*":
                        targetType = new BuiltinType(PrimitiveType.ULong);
                        break;
                    case "global::ChakraSharp._JsDiagBreakOnExceptionAttributes*":
                        targetType = parameter.TranslationUnit.FindTypedef("JsDiagBreakOnExceptionAttributes").Type;
                        break;
                    case "global::ChakraSharp._JsTypedArrayType*":
                        targetType = parameter.TranslationUnit.FindTypedef("JsTypedArrayType").Type;
                        break;
                    case "global::ChakraSharp._JsValueType*":
                        targetType = parameter.TranslationUnit.FindTypedef("JsValueType").Type;
                        break;
                    case "global::ChakraSharp._JsPropertyIdType*":
                        targetType = parameter.TranslationUnit.FindTypedef("JsPropertyIdType").Type;
                        break;
                    default:
                        throw new System.InvalidOperationException("Unexpected type:" + currentQualifiedType.ToString());
                }
                parameter.QualifiedType = new QualifiedType(new PointerType(new QualifiedType(targetType, new TypeQualifiers())));
            }

            return true;
        }
    }
}
