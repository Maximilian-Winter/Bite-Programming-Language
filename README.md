# Bite Programming Language
Bite is a dynamic programming language. 

It uses Modules, Classes and Functions to separate the code.

My idea was it to have a dynamically interpreted language in C# to support modding functionality and easy patching for the Unity Game Engine.

# Features

* Module system
* Dynamically typed
* Importing  and using C# Types and Objects
* Supports .NET Framework 4.x and .NET Core 3.1 to .NET 6.0 (netstandard2.0)
* [VS Code Language Extension for Bite](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code) ([VISX Installer](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code/releases))

# Overall Status

I'm still in the progress of developing the language itself. After that I will integrate it in Unity!

ToDo:
* Finish implementation of private and public access modifiers
* Finish implementation of static and abstract modifiers
* Implementing Constructors for classes
* More Testing

# Modules
Bite modules encapsulate code, classes and functions similar to namespaces in C#. You can import one module into another to access its declarations.

Modules are the basic foundation of a program in Bite. Each program consist of at least one module. The so called main module. Modules are defined like this:
```
module ModuleName;
```


Modules can contain classes, functions and code.

You can import other modules through the "import" keyword, like this:
```
import ModuleName;
```


You can use imported functions and variables, like this:
```
ModuleName.FunctioName();
ModuleName.VariableName;
```


Through the use of the "using" keyword, you can omit the module names, like this:
```
import ModuleName;
using ModuleName;

FunctioName();       // ModuleName Function
VariableName;        // ModuleName Variable
```

The main module code is executed after the imported module code is excecuted.


# Example Code

You can find the following Examples here:

[Examples]https://github.com/Maximilian-Winter/Bite-Programming-Language/blob/master/TestApp/TestProgram/MainModule.bite


The following code will calculate the first 36 fibonacci numbers 1000 times:

![CodeBitePic](https://user-images.githubusercontent.com/24946356/161370277-ec838b53-0865-4536-ae74-c4b25d4ac850.PNG)


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


# Importing and using C# Types and Objects.

You can import c# types into a module. For example, to write to the console you can use the `CSharpInterface` object like so:

```
module CSharpSystem;

import System;
using System;

var CSharpInterfaceObject = new CSharpInterface();

CSharpInterfaceObject.Type = "System.Console";

var Console = CSharpInterfaceCall(CSharpInterfaceObject);
```
Now you can use the variable Console like the static Class Console in C#.



For .NET Core to .NET 6.0, you need to specify an Assembly Qualified Name if the type it is not in mscorlib. You don't need the full name, but you need to specify the assembly.

```
CSharpInterfaceObject.Type = "System.Console, System.Console";
```
