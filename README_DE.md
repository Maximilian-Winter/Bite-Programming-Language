# Bite Programmiersprache
Bite ist eine dynamisch typisierte Programmiersprache

Bite verwendet Module, Klassen und Funktionen, um den Code zu trennen.

Module sind die grundlegende Basis eines Programms in Bite. Jedes Programm besteht aus mindestens einem Modul. 

Der Code auf der Modulebene kann Funktionen, Klassen, Objekte und andere Variablen enthalten. Er kann Objekte aus Klassen erzeugen, Funktionen aufrufen und auf Objekte zugreifen. 
Dies ist der Ort, an dem das eigentliche Bite-Programm geschrieben wird.
Sie können ein Modul in ein anderes importieren, um auf seine Deklarationen zuzugreifen.

Klassen in Bite sind ein objektorientierter Weg, um Code in Entwürfe für Objekte und Datenstrukturen zu unterteilen. Klassen können Objekte, andere Variablen und Funktionen enthalten. Klassen können durch Vererbung Mitglieder von anderen Klassen übernehmen. Funktionen in Klassen und Funktionen im Allgemeinen können auch Objekte aus Klassen erstellen, Funktionen aufrufen und auf Objekte zugreifen. 

Bite kompiliert zu einem Bytecode, der auf einer virtuellen Maschine ausgeführt wird.

Die Referenz Implementierung der virtuellen Maschine, BiteVM, ist eine stapelbasierte virtuelle Maschine, die in C# geschrieben wurde.

# Features

* Modulsystem
* Dynamisch typisiert
* Importieren und Verwenden von C#-Typen und -Objekten
* Unterstützt .NET Framework 4.x und .NET Core 3.1 bis .NET 6.0 (netstandard2.0)
* [VS Code Language Extension for Bite](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code) ([VISX Installer](https://github.com/Maximilian-Winter/Bite-Language-Extension-for-VS-Code/releases))

# Gesamt Status
Die meisten Features sind implementiert.
Muss noch in Unity integriert werden.

ToDo:
* Fertigstellung der Implementierung von privaten und öffentlichen Zugriffsmodifikatoren
* Fertigstellung der Implementierung von statischen und abstrakten Modifikatoren
* Mehr Tests
* Bessere Fehlermeldungen für Benutzer
# Syntax

## Module
Module werden wie folgt definiert:
```
module ModuleName;
```

Sie können andere Module mit dem Schlüsselwort "import" importieren, etwa so:
```
import ModuleName;
```


Sie können importierte Funktionen und Variablen verwenden, etwa so:
```
ModuleName.FunctioName();
ModuleName.VariableName;
```


Durch die Verwendung des Schlüsselworts "using" können Sie die Modulnamen weglassen, etwa so:
```
import ModuleName;
using ModuleName;

FunctioName();       // ModuleName Function
VariableName;        // ModuleName Variable
```

Der Code des Hauptmoduls wird ausgeführt, nachdem der Code des importierten Moduls ausgeführt wurde.

## Variablen
Variablen in Bite sind dazu da, Daten zu speichern.
Unterstützte Datentypen:
zahlen
string
object 
boolsch
array
 
Variablen werden wie folgt definiert:

```
var a = 42; // numeric data
a = "Hello World!"; // now 'a' is a variable that holds string data
a = new TestClass(); // now 'a' is a variable that holds object data from type TestClass

var b = new TestClass() // created a new variable of type TestClass
```

## Funktionen

Funktionen in Bite können Objekte aus Klassen erzeugen, Funktionen aufrufen, auf Objekte zugreifen und Werte zurückgeben. 

Sie sind wie folgt definiert:
```
function FunctionName()
{

}
```

Sie können Parameter und Rückgabewerte hinzufügen, wie zum Beispiel:
```
function FunctionName(parameterOne, parameterTwo)
{
  return parameterOne * parameterTwo;
}
```


## Klassen

Klassen in Bite sind eine objektorientierte Methode, um Code in Entwürfe für Objekte und Datenstrukturen zu unterteilen. Klassen können Objekte, andere Variablen und Funktionen enthalten. Klassen können durch Vererbung Mitglieder von anderen Klassen übernehmen. Funktionen in Klassen und Funktionen im Allgemeinen können auch Objekte aus Klassen erzeugen, Funktionen aufrufen und auf Objekte zugreifen. 

Klassen werden wie folgt definiert:
```
class ClassName
{

}
```

Sie können die Mitglieder anderer Klassen erben, etwa so:
```
class ClassName : OtherClassOne, OtherClassTwo
{

}
```

Sie können Mitglieder wie Variablen und Funktionen zu einer Klasse hinzufügen, etwa so:
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



# Beispiel Code

Die folgenden Beispiele finden Sie hier:

[Examples](https://github.com/Maximilian-Winter/Bite-Programming-Language/wiki/Example-Code)


Der folgende Code berechnet die ersten 50 Fibonacci-Zahlen und gibt sie auf der Konsole aus:

![BiteFibo](https://user-images.githubusercontent.com/24946356/162203003-13b87476-4d38-4187-9b76-fbdcc2ae5c6a.PNG)





Der folgende Code berechnet und druckt die 2-, 4-, 8-, 16-, 32- und 64-te Primzahl:

![BitePrime](https://user-images.githubusercontent.com/24946356/161903979-3d4cbdc5-8d22-4bcd-9719-a75428227d6a.PNG)





Der folgende Code erstellt ein dynamisches Array und füllt es mit Zeichenketten. Dann werden die Array-Elemente ausgedruckt:

![biteDynamic](https://user-images.githubusercontent.com/24946356/161903997-fe14fa25-9b80-4962-aeec-a977052b834a.PNG)


Der folgende Code zeigt das C# Type Import System. Er zeigt, wie man ein C#-Objekt erstellt, indem man seinen Konstruktor aufruft und das Benutzen danach:

![BiteFFI](https://user-images.githubusercontent.com/24946356/161910038-cfa41e5d-ecb6-4e75-a912-2a7517dfe967.PNG)



Der folgende Code zeigt die entsprechende C#-Klasse, die für den obigen Typ Import verwendet wird. 

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



# Benutzung
Am einfachsten ist es, die REPL (Read Evalue Print Loop) in der Bite CLI zu verwenden. Starten Sie einfach die bitevm.exe ohne commandline optionen. Ein Hauptmodul ist bereits für Sie erstellt! Sie können also direk mit dem eigentlichen Code beginnen. Sie können die REPL-Sitzung durch Eingabe von `exit` beenden.

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

Der einfachste Weg, eine Skriptdatei auszuführen, ist die Verwendung der Bite CLI.

Der folgende Befehl kompiliert die Bite-Module in `.\TestProgram` und startet die Ausführung aus dem Modul `MainModule`.

```
  bitevm -m MainModule -p .\TestProgram
```

Ein anderer Weg, um loszulegen, besteht darin in C# mit der Bite Dll, eine Instanz der Klasse `BITECompiler` zu erzeugen und die Methode `Compile()` aufzurufen.  Das erste Argument ist der Name des Hauptmoduls oder des Einstiegspunktes, wie er in der `module`-Anweisung deklariert ist. Das nächste Argument ist ein `IEnumerable<string>`, das eine Sammlung von Strings aufnimmt, die den Bite-Code jedes Moduls enthalten. In diesem Beispiel werden die Module von der Festplatte geladen, aber sie können auch aus dem Speicher kommen, wenn sie während der Laufzeit kompiliert werden.

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

Das Projekt `Bite.Cli` gibt eine ausführbare Datei `bitevm.exe` aus, die eine Reihe von Dateien am angegebenen Ort kompiliert und ausführt oder eine interaktive REPL-Sitzung startet.

```
USAGE:

  bitevm.exe <OPTIONS>

OPTIONS:

  -m  (--main)  : The entry point of the program
  -p  (--path)  : The path containing the modules to be loaded
  -i  (--input) : A list of modules to be loaded
  -r  (--repl)  : Start bitevm in interactive mode (REPL)

```

Der folgende Befehl kompiliert die Bite-Module in `.\TestProgram` und startet die Ausführung mit dem Modul `MainModule`.
```
  bitevm -m MainModule -p .\TestProgram
```

# Importieren und Verwenden von C#-Typen und -Objekten.

Sie können C#-Typen in ein Modul importieren. Um zum Beispiel in die Konsole zu schreiben, können Sie das `CSharpInterface` Objekt wie folgt verwenden:

```
Modul CSharpSystem;

importieren System;
using System;

var CSharpInterfaceObject = new CSharpInterface();

CSharpInterfaceObject.Type = "System.Console";

var Console = CSharpInterfaceCall(CSharpInterfaceObject);
```
Jetzt können Sie die Variable Console wie die statische Klasse Console in C# verwenden.
