# Bite Programming Language

<img src="https://user-images.githubusercontent.com/24946356/163622783-148528b0-607d-4824-b561-054708507078.png" alt="Bite Logo" width="400"/>



# Introduction

[Most of the following information and more can also be found here in the wiki!](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki)

[Join the Bite Language Discord Server!](https://discord.gg/wM44r5Ustt)

**Bite** is a dynamically typed, object oriented programming language

Dynamically typed, because the type of the content of a variable can change during runtime of the program.

Object-oriented, because you can define classes to encapsulate your code and data.

Bite uses modules, classes and functions to separate code.

Modules are the basic foundation of a program in Bite. Each program consists of at least one module. 

The code on the module level can contain functions, classes, objects and other variables. It can also create objects from classes, call functions and access objects. 

This is the place where the actual Bite program is written. You can import one module into another to access its declarations.

Classes in Bite are an object-oriented way to separate Code into blueprints for objects and data structures. Classes can contain objects, other variables and functions. Classes can inherit members of other classes through inheritance. Functions in classes and functions in general can also create objects from classes, call functions and access objects. 

Bite compiles to a bytecode that is run on a virtual machine.

The reference virtual machine, BiteVM, is a stack-based virtual machine written in C#.

# Nuget

```ps
> Install-package BiteVM
```


# Disclaimer

Bite is still in early stages and some things may change as the language develops.

# Features

* Module system
* Dynamically typed
* Module-level code, variables and functions
* Classes, with inheritance
* Functions are first class citizens and allow Higher Order Functions 
* Importing and using .NET Types and Objects
* Supports .NET Framework 4.x and .NET Core 3.1 to .NET 6.0 (netstandard2.0)
* [VS Code Language Extension for Bite](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code) ([VISX Installer](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code/releases))

# Overall Status

Most of the basic Language Features are implemented.

Still has to be integrated into unity!
An early test of the integration can be found [here](https://github.com/Maximilian-Winter/BiteUnity).

ToDo:
* Finish implementation of private and public access modifiers
* Finish implementation of static and abstract modifiers
* More Testing
* Virtual Inheritance
* Better Error Messages for Users

# Syntax

For an introduction about how to write Code in the Bite Language, go [here](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki#writing-code-in-bite).

And get more information about the Bite Language Syntax [here on the Wiki!](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Language-Syntax).


# Example Code

[Code for the following example and more examples can be found here!](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Example-Code)

The following code will calculate the first 50 fibonacci numbers and print them on the console:

![BiteFibo](https://user-images.githubusercontent.com/24946356/162203003-13b87476-4d38-4187-9b76-fbdcc2ae5c6a.PNG)


# Getting Started

To try out Bite, you will need to download the [BiteVM CLI](https://github.com/Maximilian-Winter/Bite-Programming-Language/releases/download/alpha-release/Bite-Language-Alpha-v0.1.2.zip). It's a command line program that will compile and interpret Bite programs. 

The CLI has two modes, [REPL mode](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Bite-CLI#repl-mode) and [Compile and Interpret mode](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Bite-CLI#compile-and-interpret-mode)

To start the REPL mode, just start `bitevm` without parameters.

To compile the bite modules in `.\TestProgram` and start the execution:

```
  bitevm.exe -p .\TestProgram
```

You can use your favorite editor to create Bite programs, but we have a [Visual Studio Code Extension](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code/releases/tag/alpha). that gives .bite programs syntax highlighting.


# Integrating BiteVM in your Unity or C# application

Install the nuget package:
```ps
> Install-package BiteVM
```
Or download the release package.

Create an instance of the `BiteCompiler` class and call the `Compile()` method. The only argument is an `IEnumerable<string>` that takes a collection of strings that contain the Bite code of each module. For this sample the modules are being loaded from disk, but they can come from memory as they are compiled during runtime.

The function will return a `BiteProgram` instance. You can call the `Run()` method on this object to execute the compiled Bite modules.

```c#
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "*.bite",
            SearchOption.AllDirectories );

        BiteCompiler compiler = new BiteCompiler();

        BiteProgram program = compiler.Compile( files.Select( File.ReadAllText ) );

        program.Run();
```

# Importing and using .NET Types and Objects.

You can register .NET types in the BiteProgram Type Registry and import these types into a module. For example, to get and use the static class `System.Console` from C#, you first has to register it in a BiteProgram through the TypeRegisty RegisterType Function.

```c#
        program.TypeRegistry.RegisterType (typeof(System.Console),"Console");
```
The first parameter is the C# Type, the second is an alias used in Bite to identifiy the class.

After this, you can use the registered class, in Bite code, through the `NetLanguageInterface` like so:

```
// Get the static class System.Console
var Console = NetLanguageInterface("Console");

// Use the static class and call the method WriteLine.
Console.WriteLine("Hello World!");
```


The following code shows how to create an C# Object in Bite by calling his constructor and the use after it:

```
// Create an instance of TestClassCSharp, the first argument of NetLanguageInterface function is the class name,
// the second argument is a boolean that signals to the NetLanguageInterface to create an object.
// The third argument is a argument for the constructor and the fourth is its C# type.
// Constructor Arguments are passed to the NetLanguageInterface like this: constructorArgument, constructorArgumentCSharpType

var TestCSharp = NetLanguageInterface("TestClassCSharp", true, 42, "int");

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
    public static int t = 45;
    public double i = 5;

    public Foo testfield { get; set; } = new Foo();

    #region Public
    
    public TestClassCSharp()
    {
        i = 0;
    }

    public TestClassCSharp( int n )
    {
        i = n;
    }
    
    public TestClassCSharp( int n, double x, int y, float z )
    {
        i = n * x * y * z;
    }

    public void PrintVar()
    {
        Console.WriteLine( i );
    }

    #endregion
}
```


Here are more examples on how to use the `NetLanguageInterface`

![BitNetInterface](https://user-images.githubusercontent.com/24946356/163724215-78bde228-9ce7-4c92-b7c5-52163d6fcd45.PNG)
