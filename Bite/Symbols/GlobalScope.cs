namespace Bite.Symbols
{

public class GlobalScope : BaseScope
{
    public override string Name => "global";

    #region Public

    public GlobalScope( Scope scope ) : base( scope )
    {
    }

    #endregion
}

}
