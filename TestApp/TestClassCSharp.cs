using System;

namespace TestApp
{

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

}
