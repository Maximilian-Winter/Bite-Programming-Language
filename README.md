# Bite Programming Language
Bite
is a dynamic programming language.
The idea was it, to have a dynamically interpreted language in C#, to support modding functionallity and easy patching for the Unity Game Engine.

I'm still in the progress of developing the language itself. After that I will integrate it in Unity!

Example Code for calculating the first 37 fibonacci numbers a 1000x times:
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
Print(temp);
```
