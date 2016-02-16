using System;
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

        private char keyPressed;

        //for opcodes involving random numbers
        private Random rand = new Random();

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
                    switch (opcode & 0x00FF)
                    {
                        case 0x00E0: //00E0	Clears the screen.
                            for (int i = 0; i < display.Length; i++)
                            {
                                display[i] = 0;
                            }
                            pc += (char)0x02;
                            needRedraw = true;
                            break;

                        case 0x00EE: //00EE	Returns from a subroutine.
                            if (stackpointer > 0)
                                stackpointer--;
                            Debug.WriteLine("INDEX:::" + stackpointer);
                            pc = (char)(stack[stackpointer] + 2);
                            Debug.WriteLine("RET TO: " + ((int)pc).ToString("X4"));
                            break;

                        default:
                            Debug.WriteLine("Unsupported Opcode!");
                            System.Windows.Forms.Application.Exit();
                            break;
                    }
                    break;

                case 0x1000: //1NNN	Jumps to address NNN.
                    pc = (char)(opcode & 0xFFF);
                    break;

                case 0x2000: //2NNN	Calls subroutine at NNN.
                    stack[stackpointer++] = pc;
                    pc = (char)(opcode & 0x0FFF);
                    break;

                case 0x3000: //3XNN	Skips the next instruction if VX equals NN.
                    if (V[(opcode & 0x0F00) >> 8] == (opcode & 0x00FF))
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
                    if (V[(opcode & 0x0F00) >> 8] == (opcode & 0x00F0) >> 4)
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
                            V[(opcode & 0x0F00) >> 8] = (char)V[(opcode & 0x00F0) >> 4];
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
                            int carry = V[(opcode & 0x0F00) >> 8] & V[(opcode & 0x00F0) >> 4];
                            if (carry == 1)
                            {
                                V[0xF] = (char)1;
                            }
                            else
                            {
                                V[0xF] = (char)0;
                            }
                            V[(opcode & 0x0F00) >> 8] = (char)((V[(opcode & 0x0F00) >> 8] + V[(opcode & 0x00F0) >> 4]) & 0xFF);
                            pc += (char)0x02;
                            break;

                        case 0x0005: //8XY5	VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there isn't.
                            pc += (char)0x02;
                            break;

                        case 0x0006: //8XY6	Shifts VX right by one. VF is set to the value of the least significant bit of VX before the shift.
                            V[0xF] = (char)(V[(opcode & 0x0F00) >> 8] & 0x1);
                            V[(opcode & 0x0F00) >> 8] = (char)(V[(opcode & 0x0F00) >> 8] >> 1);
                            pc += (char)0x02;
                            break;

                        case 0x0007: //8XY7	Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there isn't.

                            if (V[(opcode & 0x0F00) >> 8] > V[(opcode & 0x00F0) >> 4])
                            {
                                V[0xF] = (char)0;
                            }
                            else
                            {
                                V[0xF] = (char)1;
                            }

                            V[(opcode & 0x0F00) >> 8] = (char)((V[(opcode & 0x00F0) >> 4]) - (V[(opcode & 0x0F00) >> 8]) & 0xFF);

                            pc += (char)0x02;
                            break;

                        case 0x000E: //8XYE	Shifts VX left by one. VF is set to the value of the most significant bit of VX before the shift.
                            V[0xF] = (char)(V[(opcode & 0x0F00) >> 8] & 0x8);
                            V[(opcode & 0x0F00) >> 8] = (char)(V[(opcode & 0x0F00) >> 8] << 1);
                            pc += (char)0x02;
                            break;
                        default:
                            Debug.WriteLine("Unsupported Opcode!");
                            System.Windows.Forms.Application.Exit();
                            break;
                    }
                    break;

                case 0x9000: //9XY0	Skips the next instruction if VX doesn't equal VY.
                    if (V[(opcode & 0x0F00) >> 8] != V[(opcode & 0x00F0) >> 4])
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

                case 0xC000: //CXNN	Sets VX to the result of a bitwise and operation on a random number and NN.
                    V[(opcode & 0x0F00) >> 8] = (char)(rand.Next(0, 0xFF + 1) & (opcode & 0x00FF));
                    pc += (char)0x02;
                    break;

                case 0xD000: //DXYN: Draw a sprite (VX, VY) size (8, N). Sprite is located at I.
                    //Set variables for future reference
                    //int VX = , VY = , N = (opcode & 0x000F);

                    //Set collision flag to false by default
                    V[0xF] = (char)0x0;

                    //Draw via XOR
                    int pixel, line, totalX, totalY, index;
                    for (int y = 0; y < (opcode & 0x000F); y++)
                    {
                        line = memory[I + y];
                        for (int x = 0; x < 8; x++)
                        {
                            pixel = line & (0x80 >> x);
                            if (pixel != 0)
                            {
                                totalX = V[(opcode & 0x0F00) >> 8] + x;
                                totalY = V[(opcode & 0x00F0) >> 4] + y;

                                totalX = totalX % 64;
                                totalY = totalY % 32;

                                index = (totalY * 64) + totalX;

                                if (display[index] == 1)
                                    V[0xF] = (char)1;

                                display[index] ^= 1;
                            }
                        }
                    }

                    //Check collision and set V[0xF]
                    //Read the image from I
                    pc += (char)0x02;
                    needRedraw = true;
                    break;

                case 0xE000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x009E: //EX9E	Skips the next instruction if the key stored in VX is pressed.
                            if (V[(opcode & 0x0F00) >> 8] == keyPressed)
                            {
                                pc += (char)0x02;
                            }
                            else
                            {
                                pc += (char)0x04;
                            }
                            break;
                        case 0x00A1: //EXA1	Skips the next instruction if the key stored in VX isn't pressed.
                            if (V[(opcode & 0x0F00) >> 8] != keyPressed)
                            {
                                pc += (char)0x02;
                            }
                            else
                            {
                                pc += (char)0x04;
                            }
                            break;
                    }
                    break;

                case 0xF000:
                    switch (opcode & 0x00FF)
                    {
                        case 0x0007: //FX07	Sets VX to the value of the delay timer.
                            V[(opcode & 0x0F00) >> 8] = (char)delay_timer;
                            pc += (char)0x02;
                            break;

                        case 0x000A: //FX0A	A key press is awaited, and then stored in VX.
                            break;

                        case 0x0015: //FX15	Sets the delay timer to VX.
                            delay_timer = V[(opcode & 0x0F00) >> 8];
                            pc += (char)0x02;
                            break;

                        case 0x0018: //FX18	Sets the sound timer to VX.
                            break;

                        case 0x001E: //FX1E Adds VX to I.
                            I += V[(opcode & 0x0F00) >> 8];
                            pc += (char)0x02;
                            break;

                        case 0x0029: //FX29	Sets I to the location of the sprite for the character in VX. Characters 0-F (in hexadecimal) are represented by a 4x5 font.
                            break;

                        case 0x0033: //FX33	TODO
                            break;

                        case 0x0055: //FX55	Stores V0 to VX in memory starting at address I.
                            break;

                        case 0x0065: //FX65	Fills V0 to VX with values from memory starting at address I.
                            break;

                        default:
                            Debug.WriteLine("Unsupported Opcode!");
                            System.Windows.Forms.Application.Exit();
                            break;
                    }
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
        public void setKey(char k)
        {
            keyPressed = k;
        }

        public void loadProgram(string filename)
        {
            if (File.Exists(filename))
            {
                byte[] program = File.ReadAllBytes(filename);
                for (int i = 0; i < program.Length; i++)
                {
                    memory[0x200 + i] = (char)(program[i] & 0xFF);
                    //Debug.WriteLine("PROG: " +((int)(program[i])).ToString("X4"));
                }
            }
            else
            {
                Debug.WriteLine("FILE NOT FOUND");
            }
        }
    }
}
