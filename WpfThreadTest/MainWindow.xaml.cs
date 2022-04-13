using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bite.Compiler;
using Bite.Modules.Callables;
using Bite.Runtime;

namespace WpfThreadTest
{

    public class GameObject
    {
        private readonly UIElement m_Element;

        public int X { get; set; }
        public int Y { get; set; }
        public int dX { get; set; }
        public int dY { get; set; }


        public GameObject( UIElement element )
        {
            m_Element = element;
            dX = 1;
            dY = 1;
        }

        public void Move()
        {
            Canvas.SetLeft( m_Element, X );
            Canvas.SetTop( m_Element, Y );
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BiteVm vm = new BiteVm();

        public MainWindow()
        {
            InitializeComponent();
            Execute();
        }

        private void Execute()
        {
            var circle = new Ellipse()
            {
                Fill = Brushes.Black,
                Width = 100,
                Height = 100
            };

            var gameObject = new GameObject( circle );

            Canvas.Children.Add( circle );

            vm.InitVm();
            vm.RegisterSystemModuleCallables();
            vm.SynchronizationContext = SynchronizationContext.Current;

            vm.RegisterExternalGlobalObjects( new Dictionary<string, object>()
            {
                { "gameObject", gameObject },
            } );
            //particle.X += particle.dX;
            //if (particle.X >= 600 || particle.X <= 0)
            //{
            //    particle.dX = -particle.dX;
            //}
            //particle.Y += particle.dY;
            //if (particle.Y >= 400 || particle.Y <= 0)
            //{
            //    particle.dY = -particle.dY;
            //}

            //sync {
            //    textbox.Text = particle.X + ""px"";
            //}

            BiteCompiler compiler = new BiteCompiler();

            var mod = @"module Main;  
                while ( true ) { 
                    if (gameObject.X < 0 || gameObject.X > 300 ) {
                        gameObject.dX = -gameObject.dX;
                    }
                    gameObject.X += gameObject.dX;
                    sync {
                        gameObject.Move();
                    }
                }";

            var program = compiler.Compile( new[] { mod } );

            Task.Run( () =>
            {
                vm.Interpret( program );
            } ).ContinueWith( t =>
            {
                if (t.IsFaulted)
                {
                    Debug.WriteLine( t.Exception.InnerException.Message );
                }
            } ); ;
        }
    }
}
