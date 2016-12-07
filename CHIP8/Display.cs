using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace CHIP8
{
    public partial class Display : Form
    {
        // Init chip
        Chip c = new Chip();

        // REFRESHRATE measured in Hz
        const int REFRESHRATE = 60, SCALE = 10;

        public Display()
        {
            InitializeComponent();
            // Load the program into the chip
            c.loadFont();

            // Let the user choose their file
            using (OpenFileDialog fileChooserDialog = new OpenFileDialog())
            {
                if (fileChooserDialog.ShowDialog() == DialogResult.OK)
                {
                    c.loadProgram(fileChooserDialog.FileName);
                }
            }

            //@"C:\Users\Nico\Documents\Visual Studio 2015\Projects\CHIP8\GAMES\pong.ch8");

            // Init for window
            displayGrid.Image = new Bitmap(displayGrid.Width, displayGrid.Height);

            new Thread(chipWorker).Start();
            new Thread(audioWorker).Start();
        }

        public void display()
        {
            using (Graphics g = Graphics.FromImage(displayGrid.Image))
            {
                // Draw black background
                g.Clear(Color.Black);

                int count = 0;

                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        //pixels[i % 64, (int)Math.Floor((i / 64.0))]
                        g.FillRectangle((c.display[count] == 1) ? Brushes.White : Brushes.Black, new Rectangle(x * SCALE, y * SCALE, SCALE, SCALE));
                        count++;
                    }
                }
            }
            displayGrid.Invalidate();
        }

        private void chipWorker()
        {
            // Variables for reference within loop
            double msPerFrame = 1000.0 / REFRESHRATE;
            DateTime frameStart, frameEnd;
            TimeSpan ts;

            // Infinite loop for the chip to run on
            for (;;)
            {
                frameStart = DateTime.Now;
                frameEnd = frameStart.AddMilliseconds(msPerFrame);

                c.run();
                if (c.needRedraw)
                {
                    // Debug.WriteLine("REDRAWING");
                    display();
                    c.needRedraw = false;
                }

                ts = frameEnd.Subtract(frameStart);
                if (ts.Milliseconds > 0.0)
                {
                    Debug.WriteLine("Sleeping for {0}ms.", ts.Milliseconds);
                    Thread.Sleep(ts.Milliseconds); // Can be changed to determine speed
                }
            }
        }

        private void audioWorker()
        {
            for (;;)
            {
                if (c.sound_timer > 0)
                {
                    // TODO need to modify this, it stops the game for a bit
                    Console.Beep(2000, 17);
                }
            }
        }

        private void Display_FormClosed(object sender, FormClosedEventArgs e)
        {
            Environment.Exit(1);
        }

        // Keypress events
        private void Display_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Map the key to chip8
            c.keyPressed = (ushort)"1234qwerasdfzxcv".IndexOf(e.KeyChar);
        }

        private void Display_KeyUp(object sender, KeyEventArgs e)
        {
            c.keyPressed = 0;
        }
    }
}