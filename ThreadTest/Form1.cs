using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using Bite.Compiler;
using Bite.Modules.Callables;
using Bite.Runtime;

namespace ThreadTest
{

    public partial class Form1 : Form
    {
        private BiteVm vm = new BiteVm();
        public Particle particle;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load( object sender, EventArgs e )
        {
            Task.Run( () => MainThread() );

            vm.InitVm();
            vm.RegisterSystemModuleCallables();
            vm.Dispatcher = Dispatcher.CurrentDispatcher;

            vm.RegisterExternalGlobalObjects( new Dictionary < string, object >()
            {
                { "particle", particle },
                { "textbox", textBox1 }
            } );


            BiteCompiler compiler = new BiteCompiler();

            var mod = @"module Main;  
                while ( true ) { 
                    particle.X += particle.dX;
                    if ( particle.X >= 600 || particle.X <= 0 ) {
                        particle.dX = -particle.dX;
                    }
                    particle.Y += particle.dY;
                    if ( particle.Y >= 400 || particle.Y <= 0 ) {
                        particle.dY = -particle.dY;
                    }
                    textbox.Text = particle.X + ""px"";
                }";

            var program = compiler.Compile( new[] { mod } );

            Task.Run( () =>
            {
                vm.Interpret( program );
            } );
        }

        private void MainThread()
        {
            particle = new Particle()
            {
                dX = 1,
                dY = 1
            };

            while (true)
            {
                pictureBox1.Invalidate();
            }
        }

        private void pictureBox1_Paint( object sender, PaintEventArgs e )
        {
            var g = e.Graphics;
            g.Clear( Color.White );
            g.DrawEllipse( Pens.Black, particle.X, particle.Y, 10, 10 );
        }
    }
}
