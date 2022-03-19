# SrslBytecodeVmAndCodeGenerator
Srsl
stands for "Simple Runtime Scripting Language".
The idea was it, to have a dynamically interpreted language in C#, to support modding functionallity and easy patching for the Unity Game Engine.
Right now only the basic functionality of the language is implemented!

Example Code for calculating the first 37 fibonacci numbers a 1000x times:

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
Console.WriteLine(temp);
