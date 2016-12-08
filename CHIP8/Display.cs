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
        static Chip c = new Chip();

        // Threads
        Thread chipThread, soundThread;// = new Thread(audioWorker);


        // REFRESHRATE measured in Hz
        const int REFRESHRATE = 60 * 10, SCALE = 10;

        public Display()
        {
            InitializeComponent();

            KeyPreview = true;

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

            chipThread = new Thread(chipWorker);
            soundThread = new Thread(soundWorker);
        }

        public void display()
        {
            try
            {
                Graphics g = Graphics.FromImage(displayGrid.Image);
                // Draw black background
                g.Clear(Color.Black);

                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        // Fill the rectangle based on value in the pixel array
                        g.FillRectangle((c.display[(x + 64 * y)] == 1) ? Brushes.White : Brushes.Black, new Rectangle(x * SCALE, y * SCALE, SCALE, SCALE));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
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
                    //Debug.WriteLine("Sleeping for {0}ms.", ts.Milliseconds);
                    Thread.Sleep(ts.Milliseconds); // Can be changed to determine speed
                }
            }
        }

        private void soundWorker()
        {
            // Infinite loop for the audio worker to run on
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
            // Reset the keyPressed
            c.keyPressed = 0;
        }


        // GUI events
        private void startButton_Click(object sender, EventArgs e)
        {
            if(chipThread.IsAlive)
            {
                chipThread.Resume();
                soundThread.Resume();
            }
            else
            {
                chipThread.Start();
                soundThread.Start();
            }
        }

        private void pauseButton_Click(object sender, EventArgs e)
        {
            chipThread.Suspend();
            soundThread.Suspend();
        }

        private void stepButton_Click(object sender, EventArgs e)
        {
            // TODO
        }
    }
}