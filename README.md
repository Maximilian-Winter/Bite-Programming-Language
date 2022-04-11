# Bite Programming Language
[German Readme|Deutsches Readme](https://github.com/Maximilian-Winter/Bite-Programming-Language/blob/master/README_DE.md)

[Most of the following information can also be found here in the wiki!](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki)

Bite is a dynamically typed programming language

Which means that the type of the content of a variable can change during runtime of the program.

Bite uses Modules, Classes and Functions to separate the code.

Modules are the basic foundation of a program in Bite. Each program consists of at least one module. 

The code on the module level can contain functions, classes, objects and other variables. It can also create objects from classes, call functions and access objects. 
This is the place where the actual Bite program is written.
You can import one module into another to access its declarations.

Classes in Bite are an object-oriented way to separate Code into blueprints for objects and data structures. Classes can contain objects, other variables and functions. Classes can inherit members of other classes through inheritance. Functions in classes and functions in general can also create objects from classes, call functions and access objects. 

Bite compiles to a bytecode that is run on a virtual machine.

The reference virtual machine, BiteVM, is a stack-based virtual machine written in C#.

# Features

* Module system
* Dynamically typed
* Functions are first class citizens and allow Higher Order Functions 
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
* Virtual Inheritance
* Better Error Messages for Users


# Syntax
[Get information about the Bite Language Syntax here on the Wiki!](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Language-Syntax)


# Example Code

[Code for the following example and more examples can be found here!](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Example-Code)

The following code will calculate the first 50 fibonacci numbers and print them on the console:

![BiteFibo](https://user-images.githubusercontent.com/24946356/162203003-13b87476-4d38-4187-9b76-fbdcc2ae5c6a.PNG)


# Getting Started

To try out Bite, you will need to download the [BiteVM CLI](http://link.to.bitevm). It's a command line program that will compile and interpret Bite programs. 

The CLI has two modes, [REPL mode](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Bite-CLI#repl-mode) and [Compile and Interpret mode](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Bite-CLI#compile-and-interpret-mode)

To start the REPL mode, just start the [BiteVM CLI](http://link.to.bitevm).

To compile the bite modules in `.\TestProgram` and start the execution, just start the [BiteVM CLI](http://link.to.bitevm) like this:

```
  bitevm.exe -p .\TestProgram
```


You can use your favorite editor to create Bite programs, but we have a [Visual Studio Code Extension](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code/releases/tag/alpha). that gives .bite programs syntax highlighting.


## If you want to use BiteVM in your Unity or C# application, do the following:


Install the Nuget package for BiteVM

```ps
> Install-package BiteVM
```

Create an instance of the `BiteCompiler` class and call the `Compile()` method. The only argument is an `IEnumerable<string>` that takes a collection of strings that contain the Bite code of each module. For this sample the modules are being loaded from disk, but they can come from memory as they are compiled during runtime.

The function will return a `BiteProgram` instance. You can call the `Run()` method on this object to execute the compiled Bite modules.

```c#
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "*.bite",
            SearchOption.AllDirectories );

        BiteCompilercompiler = new BiteCompiler();

        BiteProgram program = compiler.Compile( files.Select( File.ReadAllText ) );

        program.Run();
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

The following code  shows how to create an C# Object by calling his constructor and the use after it:

```
var testClassInterface = new CSharpInterface();
testClassInterface.Type = "TestApp.TestClassCSharp, TestApp";

testClassInterface.ConstructorArguments[0] = 42;
testClassInterface.ConstructorArgumentsTypes[0] = "System.Int32";


var TestCSharp = CSharpInterfaceCall(testClassInterface);

TestCSharp.PrintVar();
PrintLine(TestCSharp.testfield.i);
TestCSharp.testfield.i = 58;
PrintLine(TestCSharp.testfield.i);
```

The corresponding C# Classes:
```C#
public class Foo
{
    public int i = 5;
}

public class TestClassCSharp
{
    private readonly int i = 5;

    public Foo testfield { get; set; } = new Foo();

    #region Public

    public TestClassCSharp( int n )
    {
        i = n;
    }

    public void PrintVar()
    {
        Console.WriteLine( i );
    }

    #endregion
}
```
