# Bite Programmiersprache
Bite ist eine dynamische Programmiersprache. 

Sie verwendet Module, Klassen und Funktionen, um den Code zu trennen.

Meine Idee war es, eine dynamisch interpretierte Sprache in C# zu haben, um Modding-Funktionalität und einfaches Patching für die Unity Game Engine zu unterstützen.

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

# Module
Bite-Module kapseln Code, Klassen und Funktionen ähnlich wie Namespaces in C#. Sie können ein Modul in ein anderes importieren, um auf seine Deklarationen zuzugreifen.

Module sind die grundlegende Basis eines Programms in Bite. Jedes Programm besteht aus mindestens einem Modul. Das so genannte Hauptmodul. Module werden wie folgt definiert:
```
module ModuleName;
```


Module können Klassen, Funktionen und Code enthalten.

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


# Example Code

Die folgenden Beispiele finden Sie hier:

[Examples](https://github.com/Maximilian-Winter/Bite-Programming-Language/blob/master/TestApp/TestProgram/MainModule.bite)


Der folgende Code berechnet die ersten 36 Fibonacci-Zahlen 1000 Mal und druckt die 36-te Fibonacci-Zahl:

![BiteFibo](https://user-images.githubusercontent.com/24946356/161903954-3d85e17d-34ef-4e60-aac6-f8a0fad8380d.PNG)





Der folgende Code berechnet und druckt die 2-, 4-, 8-, 16-, 32- und 64-te Primzahl:

![BitePrime](https://user-images.githubusercontent.com/24946356/161903979-3d4cbdc5-8d22-4bcd-9719-a75428227d6a.PNG)





Der folgende Code erstellt ein dynamisches Array und füllt es mit Zeichenketten. Dann werden die Array-Elemente ausgedruckt:

![biteDynamic](https://user-images.githubusercontent.com/24946356/161903997-fe14fa25-9b80-4962-aeec-a977052b834a.PNG)

