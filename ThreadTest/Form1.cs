using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
        private System.Windows.Forms.Timer Timer;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load( object sender, EventArgs e )
        {
            //Timer = new System.Windows.Forms.Timer();
            //Timer.Interval = 166;
            //Timer.Tick += ( o, args ) =>
            //{
            //    pictureBox1.Invalidate();
            //};
            //Timer.Start();

            Task.Run( () => MainThread() );

            vm.InitVm();
            vm.RegisterSystemModuleCallables();
            vm.SynchronizationContext = SynchronizationContext.Current;


            vm.RegisterExternalGlobalObjects( new Dictionary < string, object >()
            {
                { "particle", particle },
                { "textbox", textBox1 }
            } );


            vm.Stop();

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

                    sync {
                        textbox.Text = particle.X + ""px"";
                    }
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

                this.Invalidate();

                Thread.Sleep( 6 );
            }
        }

        private void pictureBox1_Paint( object sender, PaintEventArgs e )
        {
        }

        private void Form1_Paint( object sender, PaintEventArgs e )
        {
            var g = e.Graphics;
            g.Clear( Color.White );
            g.FillEllipse( Brushes.Black, particle.X, particle.Y, 10, 10 );
        }
    }
}
