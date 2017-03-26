# ChakraSharp
---

A quick & dirty pass at generating a [ChakraCore](https://github.com/Microsoft/ChakraCore) P/Invoke layer in C# using [CppSharp](https://github.com/mono/CppSharp)

### Advantages
---
 - Generates P/Invoke layer directly from ChakraCore Jsrt headers; any changes and additions made to ChakraCore can be integrated quickly and easily without manual porting
 - Function and parameter descriptions contained in the Jsrt headers are retained and included in the generated cs file.
 - Customization of the generated code can be performed by forking this project and adding/changing the passes.
 
### Instructions
---
1. Clone this repo
2. Clone Microsoft/ChakraCore - you won't need to build Chakra as the headers are used to generate the P/Invoke. In your implementing program you can use the version on NuGet.
3. Ensure that the path in ChakraShare.CLI/Program.cs points to the path where ChakraCore was cloned.
4. Build and run ChakraSharp.CLI -- ChakraCore.cs will be generated. Take this file and use it in the application that you'd like to use Chakra in.

For a sample of how to use the generated ChakraCore.cs file, see [ChakraSharp-Sample](https://github.com/BaristaLabs/ChakraSharp-Sample)

For a high-level object model, see [BaristaCore](https://github.com/BaristaLabs/BaristaCore)
