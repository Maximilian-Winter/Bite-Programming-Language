using System.Windows;
using System.Windows.Controls;

namespace WpfThreadTest
{

public class GameObject
{
    private readonly UIElement m_Element;

    public float X { get; set; }

    public float Y { get; set; }

    public float dX { get; set; }

    public float dY { get; set; }

    #region Public

    public GameObject( UIElement element )
    {
        m_Element = element;
        dX = 0.01f;
        dY = 0.01f;
    }

    public void Move()
    {
        // This code needs to be executed on the UI thread, where the element was created
        Canvas.SetLeft( m_Element, X );
        Canvas.SetTop( m_Element, Y );
    }

    #endregion
}

}
