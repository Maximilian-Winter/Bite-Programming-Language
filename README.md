# Srsl - A Bytecode VM and Code Generator

Srsl stands for "Simple Runtime Scripting Language".

The idea was to have a dynamically interpreted language in C# to support modding functionality and easy patching for the Unity Game Engine.

Right now only the basic functionality of the language is implemented!

# Example Code

The following code will calculate the first 37 fibonacci numbers 1000 times:

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

The easiest way to get up and running is to create an instance of the `Compiler` class and call the `Compile()` method.  The first argument is the name of the main module or entrypoint as declared by the `module` statement. The next argument is an `IEnumerable<string>` that takes a collection of strings that contain the Srsl code of each module. For this sample the modules are being loaded from disk, but they can come from memory as they are compiled during runtime.

```c#
   var files = Directory.EnumerateFiles(o.Path, "*.srsl", SearchOption.AllDirectories);

    // Sets ThrowOnRecognitionException to catch parsing errors
    var compiler = new Compiler(true);

    var program = compiler.Compile(o.MainModule, files.Select(File.ReadAllText));

    // Executes the program
    program.Run();
```

# CLI

The `Srsl.Cli` project outputs an executable `srslvm.exe` that will compile and run a set of files in the specified location.

```
USAGE:

  srslvm.exe <OPTIONS>

OPTIONS:

  -m  (--main)  : The entry point of the program
  -p  (--path)  : The path containing the modules to be loaded
  -i  (--input) : A list of modules to be loaded
```

The following command will compile the srsl modules in `.\TestProgram` and start execution from the `MainModule` module.

```
  srslvm -m MainModule -p .\TestProgram
```

# Modules

**TODO:**

Srsl modules encapsulate code similar to classes in C#. You can import one module into another to access its declarations.



# Importing System Types

**TODO:**

You can import system types into a module. For example, to write to the console you can use the `CSharpInterface` object like so:

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
