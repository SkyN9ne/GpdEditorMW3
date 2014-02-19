/*  Copyright (C) 2013 Ranbir Aulakh

    Visit: http://elegantdevs.com  Thank you.
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents;

namespace Mw3_Gamercard_Editor
{
    public partial class Form1 : Office2007Form
    {
        string filepath;
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonX1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select your Modern Warefare 3 GPD";
                ofd.Multiselect = false;
                if (ofd.ShowDialog() == DialogResult.OK)
                    filepath = ofd.FileName;
                MessageBox.Show("Loaded, please select the options you want, and hit Mod");
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            using (Endian.IO io = new Endian.IO(filepath, Endian.Type.Big))
            {
                if (cod4prestige.Checked)
                {
                    io.Out.BaseStream.Position = 0x3504; // Cod4 Prestige
                    io.Out.Write(Endian.Conversions.HexToBytes("0000000A")); //10th Cod4
                }
                if (cod4level.Checked)
                {
                    io.Out.BaseStream.Position = 0x3604; // Cod4 Prestige
                    io.Out.Write(Endian.Conversions.HexToBytes("00000037")); //10th Cod4
                }
                if (blackopsprestige.Checked)
                {
                    io.Out.BaseStream.Position = 0x2F04; // Black Ops Prestige
                    io.Out.Write(Endian.Conversions.HexToBytes("0000000F")); //15th Prestige Black Ops
                }
                if (blackopslevel.Checked)
                {
                    io.Out.BaseStream.Position = 0x3004; // Black Ops Levels
                    io.Out.Write(Endian.Conversions.HexToBytes("00000032")); //50 Levels Black Ops
                }
                if (wawprestige.Checked)
                {
                    io.Out.BaseStream.Position = 0x3304; // waw prestige
                    io.Out.Write(Endian.Conversions.HexToBytes("0000000A")); //waw 55 levels
                }
                if (wawlevels.Checked)
                {
                    io.Out.BaseStream.Position = 0x3404; // waw levels
                    io.Out.Write(Endian.Conversions.HexToBytes("0000003C")); //waw 65 levels
                }
                if (mw2prestige.Checked)
                {
                    io.Out.BaseStream.Position = 0x3104; // mw2 Prestige
                    io.Out.Write(Endian.Conversions.HexToBytes("0000000A")); //10th Mw2
                }
                if (mw2levels.Checked)
                {
                    io.Out.BaseStream.Position = 0x3204; // Mw2 Levels
                    io.Out.Write(Endian.Conversions.HexToBytes("00000046")); //70 Levels Mw2
                }
            }
            MessageBox.Show("File has been sucessfully modded, please inject back into your Profile and Resign\n\n~iRanbirHD (Zero Dev)");
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            using (Form2 frm = new Form2())
                frm.ShowDialog();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.xbox360content.com/forums");
        }
    }
}
