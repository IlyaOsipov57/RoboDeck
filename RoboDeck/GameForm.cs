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

namespace RoboDeck
{
    public partial class GameForm : Form
    {
        public GameForm()
        {
            InitializeComponent();
            TheForm = this;
            StartMainLoop();
        }

        private static Thread MainLoop;
        private static bool Running = true;
        private static bool started = false;
        private static GameForm TheForm;
        public bool WasClosed = false;
        

        public void StartMainLoop()
        {
            if (started)
                return;
            started = true;
            MainLoop = new Thread(Run);
            MainLoop.Start();
        }

        private static double FpsCap = 60;

        private void Run()
        {
            try
            {
                var lastTime = DateTime.Now;
                while (Running)
                {
                    var deltaTime = (DateTime.Now - lastTime).TotalMilliseconds / 1000;
                    lastTime = DateTime.Now;
                    RunCycle(deltaTime);
                    var sleepTime = (int)(1000 / FpsCap - (DateTime.Now - lastTime).TotalMilliseconds);
                    if (sleepTime > 0)
                    {
                        Thread.Sleep(sleepTime);
                    }
                }
            }
            catch (Exception e)
            {
                try
                {
                    if (!this.WasClosed)
                    {
                        this.Invoke(new Action(this.Close));
                    }
                }
                catch { }
            }
        }

        private void RunCycle(double _deltaTime)
        {
            cardEditor1.Redraw(_deltaTime);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            WasClosed = true;
        }

        public static bool AllowCursorControl = true;

        private void Form1_Activated(object sender, EventArgs e)
        {
            AllowCursorControl = true;
        }

        private void Form1_Deactivate(object sender, EventArgs e)
        {
            AllowCursorControl = false;
        }

        public static void SetName (String _name)
        {
            TheForm.Text = _name;
        }
    }
}
