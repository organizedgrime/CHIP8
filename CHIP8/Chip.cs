using System.Diagnostics;
using System.IO;

namespace CHIP8
{
    class Chip
    {
        //initialization of memory
        private char[] memory = new char[4096], V = new char[16], stack = new char[16];
        private byte[] keys = new byte[16], display = new byte[64 * 32];
        private char I = (char)0x0, pc = (char)0x200;
        private int stackpointer = 0, sound_timer = 0, delay_timer = 0;

        private bool needRedraw = false;

        public void run()
        {
            //fetch opcode
            char opcode = (char)((memory[pc] << 8) | memory[pc + 1]);
            Debug.WriteLine("OPCODE: " + ((int)opcode).ToString("X4"));

            //decode opcode
            switch (opcode & 0xF000)
            {
                //switch through first bit
                case 0x0000:
                    switch (opcode & 0x000F)
                    {
                        case 0x0000: //00E0	Clears the screen.
                            for(int i = 0; i < display.Length; i++)
                            {
                                display[i] = 0;
                            }
                            pc += (char)0x02;
                            needRedraw = true;
                            break;

                        case 0x000E: //00EE	Returns from a subroutine.
                            pc = (char)(stack[stackpointer--] + 2);
                            break;

                        default:
                            Debug.WriteLine("Unsupported Opcode!");
                            System.Windows.Forms.Application.Exit();
                            break;
                    }
                    break;

                case 0x1000: //1NNN	Jumps to address NNN.
                    pc += (char)0x02;
                    break;

                case 0x2000: //2NNN	Calls subroutine at NNN.
                    stack[stackpointer++] = pc;
                    pc = (char)(opcode & 0x0FFF);
                    break;

                case 0x3000: //3XNN	Skips the next instruction if VX equals NN.
                    if(V[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
                    {
                        //skip next instruction
                        pc += (char)0x04;
                    }
                    else
                    {
                        pc += (char)0x02;
                    }
                    break;

                case 0x4000: //4XNN	Skips the next instruction if VX doesn't equal NN.
                    if (V[(opcode & 0x0F00) >> 8] != (opcode & 0x00FF))
                    {
                        //skip next instruction
                        pc += (char)0x04;
                    }
                    else
                    {
                        pc += (char)0x02;
                    }
                    break;

                case 0x5000: //5XY0	Skips the next instruction if VX equals VY.
                    if(V[(opcode & 0x0F00) >> 8] == (opcode & 0x00F0) >> 4)
                    {
                        //skip next instruction
                        pc += (char)0x04;
                    }
                    else
                    {
                        pc += (char)0x02;
                    }
                    break;

                case 0x6000: //6XNN	Sets VX to NN.
                    V[(opcode & 0x0F00) >> 8] = (char)(opcode & 0x00FF);
                    pc += (char)0x02;
                    break;

                case 0x7000: //7XNN	Adds NN to VX.
                    V[(opcode & 0x0F00) >> 8] += (char)(opcode & 0x00FF);
                    pc += (char)0x02;
                    break;

                case 0x8000:
                    //switch through last bit
                    switch (opcode & 0x000F)
                    {
                        case 0x0000: //8XY0	Sets VX to the value of VY.
                            V[(opcode & 0x0F00) >> 8] = V[(opcode & 0x00F0) >> 4];
                            pc += (char)0x02;
                            break;
                        case 0x0001: //8XY1	Sets VX to VX or VY.
                            V[(opcode & 0x0F00) >> 8] = (char)(V[(opcode & 0x0F00) >> 8] | V[(opcode & 0x00F0) >> 4]);
                            pc += (char)0x02;
                            break;
                        case 0x0002: //8XY2	Sets VX to VX and VY.
                            V[(opcode & 0x0F00) >> 8] = (char)(V[(opcode & 0x0F00) >> 8] & V[(opcode & 0x00F0) >> 4]);
                            pc += (char)0x02;
                            break;
                        case 0x0003: //8XY3	Sets VX to VX xor VY.
                            V[(opcode & 0x0F00) >> 8] = (char)(V[(opcode & 0x0F00) >> 8] ^ V[(opcode & 0x00F0) >> 4]);
                            pc += (char)0x02;
                            break;
                        case 0x0004: //8XY4	Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there isn't.
                            pc += (char)0x02;
                            break;
                        case 0x0005: //8XY5	VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            pc += (char)0x02;
                            break;
                        case 0x0006: //8XY6	Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.
                            pc += (char)0x02;
                            break;
                        case 0x0007: //8XY7	Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            pc += (char)0x02;
                            break;
                        case 0x000E: //8XYE	Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.
                            pc += (char)0x02;
                            break;
                        default:
                            Debug.WriteLine("Unsupported Opcode!");
                            System.Windows.Forms.Application.Exit();
                            break;
                    }
                    break;

                case 0x9000: //9XY0	Skips the next instruction if VX doesn't equal VY.
                    if(V[(opcode & 0x0F00) >> 8] != V[(opcode * 0x00F0) >> 4])
                    {
                        pc += (char)0x04;
                    }
                    else
                    {
                        pc += (char)0x02;
                    }
                    break;

                case 0xA000: //ANNN	Sets I to the address NNN.
                    I = (char)(opcode & 0x0FFF);
                    pc += (char)0x02;
                    break;

                case 0xD000: //DXYN: Draw a sprite (VX, VY) size (8, N). Sprite is located at I.
                    //Set variables for future reference
                    int VX = V[(opcode & 0x0F00) >> 8], VY = V[(opcode & 0x00F0) >> 4], N = (opcode & 0x000F);

                    //Set collision flag to false by default
                    V[0xF] = (char)0x0;

                    //Draw via XOR


                    //Check collision and set V[0xF]
                    //Read the image from I
                    pc += (char)0x02;
                    needRedraw = true;
                    break;
                default:
                    Debug.WriteLine("Unsupported Opcode!");
                    System.Windows.Forms.Application.Exit();
                    break;
            }
        }

        public bool needsRedraw()
        {
            return needRedraw;
        }

        public byte[] getDisplay()
        {
            return display;
        }

        public void removeDrawFlag()
        {
            needRedraw = false;
        }

        public void loadProgram(string filename)
        {
            if (File.Exists(filename))
            {
                byte[] program = File.ReadAllBytes(filename);
                for (int i = 0; i < program.Length; i++)
                {
                    memory[0x200 + i] = (char)(program[i] & 0xFF);
                    //Debug.WriteLine(String.Format(@"\x{0:x4}", (ushort)memory[0x200 + i]));
                }
            }
            else
            {
                Debug.WriteLine("FILE NOT FOUND");
            }
        }
    }
}
