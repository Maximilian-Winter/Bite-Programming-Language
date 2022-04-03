using System;

namespace TestApp
{
public class Foo
{
    public int i = 5;
}

public class TestClassCSharp
{
    private int i = 5;

    private Foo field = new Foo();
    
    
    public TestClassCSharp(int n)
    {
        i = n;
    }

    public Foo testfield
    {
        get => field;
        set => field = value;
    }

    public void PrintVar()
    {
      Console.WriteLine(i);  
    }
}

}
