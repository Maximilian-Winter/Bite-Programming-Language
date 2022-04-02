# Bite Programming Language
Bite is a dynamic programming language.

I'm still in the progress of developing the language itself. After that I will integrate it in Unity!

The idea was to have a dynamically interpreted language in C# to support modding functionality and easy patching for the Unity Game Engine.


# Example Code

The following code will calculate the first 36 fibonacci numbers 1000 times:

![CodeBitePic](https://user-images.githubusercontent.com/24946356/161369765-c39d3d79-d065-4de4-8043-b41667b6a971.PNG)


```
module MainModule;

import System;
using System;

function FindFibonacciNumber(n)
{
    var count= 2;
    var a = 1;
    var b = 1;
    var c = 1;
    if(n == 0)
    {
        return 0;
    }
    while(count<n)
    {
        c = a + b;
        a = b;
        b = c; 
        count++;
    }

    return c;
}

var temp = 0;
var count3 = 0;

while(count3 < 1000)
{
    var count2 = 0;
    while(count2 < 37)
    {
        temp = FindFibonacciNumber(count2);
        count2++;
    }
    count3++;
}
```

# Features

* Module system
* Dynamically typed
* Supports .NET Framework 4.x and .NET Core 3.1 to .NET 6.0 (netstandard2.0)

# Usage

The easiest way to get up and running is to create an instance of the `Compiler` class and call the `Compile()` method.  The first argument is the name of the main module or entrypoint as declared by the `module` statement. The next argument is an `IEnumerable<string>` that takes a collection of strings that contain the Bite code of each module. For this sample the modules are being loaded from disk, but they can come from memory as they are compiled during runtime.

```c#
   var files = Directory.EnumerateFiles(o.Path, "*.bite", SearchOption.AllDirectories);

    // Sets ThrowOnRecognitionException to catch parsing errors
    var compiler = new Compiler(true);

    var program = compiler.Compile(o.MainModule, files.Select(File.ReadAllText));

    // Executes the program
    program.Run();
```

# CLI

The `Bite.Cli` project outputs an executable `bitevm.exe` that will compile and run a set of files in the specified location.

```
USAGE:

  bitevm.exe <OPTIONS>

OPTIONS:

  -m  (--main)  : The entry point of the program
  -p  (--path)  : The path containing the modules to be loaded
  -i  (--input) : A list of modules to be loaded
```

The following command will compile the bite modules in `.\TestProgram` and start execution from the `MainModule` module.

```
  bitevm -m MainModule -p .\TestProgram
```

# Modules

Bite modules encapsulate code and classes similar to namespaces in C#. You can import one module into another to access its declarations.



# Importing C# System Types.

You can import c# system types into a module. For example, to write to the console you can use the `CSharpInterface` object like so:

```
module CSharpSystem;

import System;
using System;

var CSharpInterfaceObject = new CSharpInterface();

CSharpInterfaceObject.Type = "System.Console";

var Console = CSharpInterfaceCall(CSharpInterfaceObject);
```

For .NET Core to .NET 6.0, you need to specify an Assembly Qualified Name if the type it is not in mscorlib. You don't need the full name, but you need to specify the assembly.

```
CSharpInterfaceObject.Type = "System.Console, System.Console";
```
