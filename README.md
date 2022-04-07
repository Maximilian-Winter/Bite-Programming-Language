# Bite Programming Language
[German Readme|Deutsches Readme](https://github.com/Maximilian-Winter/Bite-Programming-Language/blob/master/README_DE.md)



Bite is a dynamically typed programming language

Bite uses Modules, Classes and Functions to separate the code.

Modules are the basic foundation of a program in Bite. Each program consists of at least one module. 

The code on the module level can contain functions, classes, objects and other variables. It can also create objects from classes, call functions and access objects. 
This is the place where the actual Bite program is written.
You can import one module into another to access its declarations.

Code on the module level will be executed in dependency order. 

Classes in Bite are an object-oriented way to separate Code into blueprints for objects and data structures. Classes can contain objects, other variables and functions. Classes can inherit members of other classes through inheritance. Functions in classes and functions in general can also create objects from classes, call functions and access objects. 


Bite compiles to a bytecode that is run on a virtual machine.

The reference virtual machine, BiteVM, is a stack-based virtual machine written in C#.

# Features

* Module system
* Dynamically typed
* Importing  and using C# Types and Objects
* Supports .NET Framework 4.x and .NET Core 3.1 to .NET 6.0 (netstandard2.0)
* [VS Code Language Extension for Bite](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code) ([VISX Installer](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code/releases))

# Overall Status
Most of Language Features are implemented.
Still has to be integrated into unity!

ToDo:
* Finish implementation of private and public access modifiers
* Finish implementation of static and abstract modifiers
* More Testing
* Better Error Messages for Users
* Switch back to antlr based parser 

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

[Examples](https://github.com/Maximilian-Winter/Bite-Programming-Language/blob/master/TestApp/TestProgram/MainModule.bite)


The following code will calculate the first 36 fibonacci numbers 1000 times and print the 36-th fibonacci number:

![BiteFibo](https://user-images.githubusercontent.com/24946356/161903954-3d85e17d-34ef-4e60-aac6-f8a0fad8380d.PNG)






The following code will calculate and print the 2-, 4-, 8-, 16-, 32- and 64-th Prime Number:

![BitePrime](https://user-images.githubusercontent.com/24946356/161903979-3d4cbdc5-8d22-4bcd-9719-a75428227d6a.PNG)






The following code will create a dynamic array and fill it with strings. Then it will print out the array elements:

![biteDynamic](https://user-images.githubusercontent.com/24946356/161903997-fe14fa25-9b80-4962-aeec-a977052b834a.PNG)


The following code shows the C# Type Import System. It shows how to create an C# Object by calling his constructor and the use after it:

![BiteFFI](https://user-images.githubusercontent.com/24946356/161910038-cfa41e5d-ecb6-4e75-a912-2a7517dfe967.PNG)



The following code shows the corresponding C# Class used for type import above. 

![BiteFFICSharpClass](https://user-images.githubusercontent.com/24946356/161909903-f045b75f-734a-4de3-8203-d47644a8c8d4.PNG)



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
