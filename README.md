#ChakraSharp
---

A quick & dirty pass at generating a [ChakraCore](https://github.com/Microsoft/ChakraCore) wrapper in C# using [CppSharp](https://github.com/mono/CppSharp)

1. Clone this repo
2. Clone Microsoft/ChakraCore - you won't need to build Chakra as the headers are used to generate the wrapper. In your implementing program you can use the version on NuGet.
3. Ensure that the path in ChakraShare.CLI/Program.cs points to the path where ChakraCore was cloned.
4. Build and run ChakraSharp.CLI -- ChakraCore.cs will be generated. Take this file and use it in the application that you'd like to use Chakra in.

For a sample of how to use the generated ChakraCore.cs file, see [ChakraSharp-Sample](https://github.com/BaristaLabs/ChakraSharp-Sample)

For a high-level object model, see [BaristaCore](https://github.com/BaristaLabs/BaristaCore)
