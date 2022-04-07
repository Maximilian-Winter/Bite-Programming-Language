# Bite Programming Language
[German Readme|Deutsches Readme](https://github.com/Maximilian-Winter/Bite-Programming-Language/blob/master/README_DE.md)

[Most of the following information can also be found here in the wiki!](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki)

Bite is a dynamically typed programming language

Bite uses Modules, Classes and Functions to separate the code.

Modules are the basic foundation of a program in Bite. Each program consists of at least one module. 

The code on the module level can contain functions, classes, objects and other variables. It can also create objects from classes, call functions and access objects. 
This is the place where the actual Bite program is written.
You can import one module into another to access its declarations.

Classes in Bite are an object-oriented way to separate Code into blueprints for objects and data structures. Classes can contain objects, other variables and functions. Classes can inherit members of other classes through inheritance. Functions in classes and functions in general can also create objects from classes, call functions and access objects. 

Bite compiles to a bytecode that is run on a virtual machine.

The reference virtual machine, BiteVM, is a stack-based virtual machine written in C#.



# Syntax
## Modules
Modules are the basic foundation of a program in Bite. Each program consists of at least one module. 

The code on the module level can contain functions, classes, objects and other variables. It can also create objects from classes, call functions and access objects. 
Modules are defined like this:
```
module ModuleName;
```

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

## Variables
Variables in Bite are there to hold data.
Supported data types:
numeric
string
object 
boolean
array
 
Variables are defined like this:

```
var a = 42; // numeric data
a = "Hello World!"; // now 'a' is a variable that holds string data
a = new TestClass(); // now 'a' is a variable that holds object data from type TestClass

var b = new TestClass() // created a new variable of type TestClass
```

## Functions

Functions in Bite can create objects from classes, call functions, access objects and return values. 

They are defined, like that:
```
function FunctionName()
{

}
```

You can add parameters and return values, like that:
```
function FunctionName(parameterOne, parameterTwo)
{
  return parameterOne * parameterTwo;
}
```

## Classes

Classes in Bite are an object-oriented way to separate Code into blueprints for objects and data structures. Classes can contain objects, other variables and functions. Classes can inherit members of other classes through inheritance. Functions in classes and functions in general can also create objects from classes, call functions and access objects. 

Classes are defined like this:
```
class ClassName
{

}
```

You can inherit the members of other classes, like this:
```
class ClassName : OtherClassOne, OtherClassTwo
{

}
```

You can add members like variables and functions to a class, like this:
```
class ClassName
{
  var MemberOne = 5;
  function MethodOne(t)
  {
     return MemberOne * t;
  }
}
```


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
* Better Error Messages for Users




# Example Code

You can find the following Examples as Code here:

[Examples](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Example-Code)


The following code will calculate the first 50 fibonacci numbers and print them on the console:

![BiteFibo](https://user-images.githubusercontent.com/24946356/162203003-13b87476-4d38-4187-9b76-fbdcc2ae5c6a.PNG)






The following code will calculate and print the 2-, 4-, 8-, 16-, 32- and 64-th Prime Number:

![BitePrime](https://user-images.githubusercontent.com/24946356/161903979-3d4cbdc5-8d22-4bcd-9719-a75428227d6a.PNG)






The following code will create a dynamic array and fill it with strings. Then it will print out the array elements:

![biteDynamic](https://user-images.githubusercontent.com/24946356/161903997-fe14fa25-9b80-4962-aeec-a977052b834a.PNG)





The following code show the use of classes, constructors and inheritance:

![BiteClasses](https://user-images.githubusercontent.com/24946356/162202377-444a4156-d257-48bc-b3ba-7dfdd215ff7e.PNG)



The following code will create a function, that returns a function and call that function afterwards:

![BiteFuncAsReturn](https://user-images.githubusercontent.com/24946356/162301361-dc4e0d2b-8d1a-450a-ac9f-ddf94d666230.PNG)




The following code will pass a function, to a function and call that passed function:

![BiteFuncAsArg](https://user-images.githubusercontent.com/24946356/162301881-122c8cc9-dc18-4fcf-9102-65412efc6820.PNG)



The following code shows the C# Type Import System. It shows how to create an C# Object by calling his constructor and the use after it:

![BiteFFI](https://user-images.githubusercontent.com/24946356/161910038-cfa41e5d-ecb6-4e75-a912-2a7517dfe967.PNG)



The following code shows the corresponding C# Class used for type import above. 

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



# Usage
The easiest way to get up and running is to use the REPL(Read Evalue Print Loop) in the Bite CLI, just start the bitevm.exe without commandline options. A main module is already created for you! So you can start with the actual code. You can exit REPL session by entering `exit`

```
     Bite Programming Langauge v0.1 (c) 2022

     Bite REPL(Read Evaluate Print Loop)
     
     type 'declare' to declare functions, structs and classes
     type 'reset' to reset the module
     type 'help' for help.
     type 'exit' or ^Z to quit. type 'help' for help.
     > var a = 5;
     > var b = 42;
     > PrintLine(5 + 42);
     47
     >

```
The easiest way to run a script file, is the use of the Bite CLI.

The following command will compile the bite modules in `.\TestProgram` and start execution.

```
  bitevm -p .\TestProgram
```

Another way to get up and running is to use the Bite dll in C# to create an instance of the `BITECompiler` class and call the `Compile()` method. The first argument is an `IEnumerable<string>` that takes a collection of strings that contain the Bite code of each module. For this sample the modules are being loaded from disk, but they can come from memory as they are compiled during runtime.

```c#
        IEnumerable < string > files = Directory.EnumerateFiles(
            ".\\TestProgram",
            "*.bite",
            SearchOption.AllDirectories );

        BITECompiler compiler = new BITECompiler();

        BiteProgram program = compiler.Compile( "MainModule", files.Select(File.ReadAllText));

        program.Run();
```

# CLI

The `Bite.Cli` project outputs an executable `bitevm.exe` that will compile and run a set of files in the specified location or start an interactive REPL session, when given no commandline options.

```
USAGE:

  bitevm.exe <OPTIONS>

OPTIONS:
  -p  (--path)  : The path containing the modules to be loaded
  -i  (--input) : A list of modules to be loaded

```

The following command will compile the bite modules in `.\TestProgram` and start execution.

```
  bitevm -p .\TestProgram
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
