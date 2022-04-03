using System;

namespace TestApp
{

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

}
