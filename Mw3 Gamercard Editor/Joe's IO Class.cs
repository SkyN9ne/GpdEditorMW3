/*
 * This document is liscensed under Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported liscense by Admin Joe of Xbox360Content.com
 * You are free to: Share (Copy, Distribute, Transmit work), Remix (Adapt the work)
 * 
 * Under the following conditions: 
 * Attribution (You must attribute the work in the manner specified by the author or liscense)
 * Noncommericial (You may not use this work for commercial purposes)
 * Share Alike (If you alter, transform, or build upon this work, you may distribute the resulting work under the same or simaler liscense to this one)
 * 
 * With the understanding that:
 * Waiver (Any of the above conditions can be waived if you get permission from the copyright holder)
 * Public Domain (Where the work or any of its elements is in the public domain under applicable law, that status is in no way affected by the license
 * 
 * Other Rights -- In no way are any of the following rights affected by the liscense:
 * Your fair dealing or fair use rights, or other applicablecoyright exceptions and limitations
 * The author's moral rights
 * Rights other persons may have either in the work itself or in how the work is used, such as publicity or privacy rights.
 * 
 * Notice &#8212; For any reuse or distribution, you must make clear to others the license terms of this work. The best way to do this is with a link to this web page.
 *
 * For more about this liscense please go here: http://creativecommons.org/licenses/by-nc-sa/3.0/legalcode
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

public class Endian
{
    public enum Type
    {
        Big,
        Little
    }

    /// <summary>
    /// Works as only an IDisposable class, using Open() and
    /// Close() was insane and slow. It now opens the 
    /// BinaryReader and BinaryWriter on Launch, i'm
    /// eventually going to move to a userfriendly version (when
    /// I get the time) where you don't have to put io.In
    /// or io.Out
    /// </summary>
    public class IO : IDisposable
    {
        #region calls
        private bool
            isfile = false,
            isOpen = false;
        private Stream stream = null;
        private string filepath = "";
        private Type Type = Type.Little;
        private Reader _in = null;
        private Writer _out = null;
        #endregion

        public bool Opened { get { return isOpen; } }
        public Reader In { get { return _in; } }
        public Writer Out { get { return _out; } }
        public Stream Stream { get { return stream; } }

        public IO(string FilePath, Type EndianStyle)
        {
            this.Type = EndianStyle;
            this.filepath = FilePath;
            this.isfile = true;
            Open();
        }
        public IO(MemoryStream MemoryStream, Type EndianStyle)
        {
            this.Type = EndianStyle;
            this.stream = MemoryStream;
            this.isfile = false;
            Open();
        }
        public IO(Stream Stream, Type EndianStyle)
        {
            this.Type = EndianStyle;
            this.stream = Stream;
            this.isfile = false;
            Open();
        }
        public IO(byte[] Buffer, Type EndianStyle)
        {
            this.Type = EndianStyle;
            this.stream = new MemoryStream(Buffer);
            this.isfile = false;
            Open();
        }

        public UInt32 SwapUInt32(UInt32 inValue)
        {
            return (UInt32)(((inValue & 0xff000000) >> 24) |
                     ((inValue & 0x00ff0000) >> 8) |
                     ((inValue & 0x0000ff00) << 8) |
                     ((inValue & 0x000000ff) << 24));
        }

        void Open()
        {
            if (isOpen == true)
                return;
            if (filepath == null)
                return;
            try
            {
                if (isfile)
                    stream = new FileStream(filepath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            }
            catch (NullReferenceException)
            {
                return;
            }
            catch (Exception)
            { return; }
            _in = new Reader(stream, Type);
            _out = new Writer(stream, Type);

            isOpen = true;
        }

        #region Disposition
        ~IO()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool DisposeManaged)
        {
            try
            {
                stream.Close();
                _in.Close();
                _out.Close();
                this.isOpen = false;
                if (DisposeManaged)
                {
                    stream.Dispose();
                    _in.Dispose();
                    _out.Dispose();
                }
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (NullReferenceException)
            {
                return;
            }
        }
        #endregion
    }

    public class Reader : BinaryReader, IDisposable
    {
        public Type endianstyle;

        public Reader(Stream stream, Type endianstyle)
            : base(stream)
        {
            this.endianstyle = endianstyle;
        }

        /// <summary>
        /// searches recursivally for a hex string (in bytes) so add 0x infront of every hex value (ext like this...
        /// 
        /// if it is E0 34 B1
        /// 
        /// then you would make a new byte[] a = {0xE0,0x34,0xB1};
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns>null if can't find it, returns offset otherwise</returns>
        public int SearchforHex(string Hexstring)
        {
            return SearchforHex(Hexstring, 0);
        }
        /// <summary>
        /// searches recursivally for a hex string (in bytes) so add 0x infront of every hex value (ext like this...
        /// 
        /// if it is E0 34 B1
        /// 
        /// then you would make a new byte[] a = {0xE0,0x34,0xB1};
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns>null if can't find it, returns offset otherwise</returns>
        public int SearchforHex(string Hexstring, int Offset)
        {
            /* Problems Noted:
             * This method has been giving me headaches
             * theoretically, when you compare the byte[]
             * hex, and byte[] temp, it should be the same
             * (and would increase the time taken), but no.
             * It won't even align if you keep them as byte[]'s
             * but if you make them strings they work...
             * I hate when this happens, i'll fix it later for
             * a speed improvement.
             */
            Hexstring = Hexstring.Replace(" ", string.Empty);
            byte[] hex = Endian.Conversions.HexToBytes(Hexstring);
            string temp;
            long i = Offset;
            int count = hex.Length;
            for (; i <= base.BaseStream.Length; i++)
            {
                base.BaseStream.Position = i;
                temp = Endian.Conversions.BytesToHexString(base.ReadBytes(count), false);
                if (Hexstring == temp)
                    return (int)base.BaseStream.Position - count;
            }
            return 0;
        }
        public int SearchforString(string String, bool Unicode)
        {
            return SearchforString(String, Unicode, 0);
        }
        public int SearchforString(string String, bool Unicode, int Offset)
        {
            byte[] bytes;
            if(!Unicode)
                bytes = String.GetBytes(false);
            else
                bytes = String.GetBytes(true);
            return this.SearchforHex(Endian.Conversions.BytesToHexString(bytes, false), Offset);
        }

        /// <summary>
        /// My version of reading a string
        /// </summary>
        /// <returns></returns>
        public override string ReadString()
        {
            StringBuilder sb = new StringBuilder();
            for (long i = base.BaseStream.Position; i < base.BaseStream.Length; i++)
            {
                char character = base.ReadChar();
                if (char.IsLetterOrDigit(character) || char.IsPunctuation(character) || char.IsWhiteSpace(character))
                    sb.Append(character);
                else
                    break;
            }
            return sb.ToString();
        }

        public override ushort ReadUInt16()
        {
            return ReadUInt16(endianstyle);
        }
        public ushort ReadUInt16(Type Type)
        {
            byte[] buffer = base.ReadBytes(2);

            if (Type == Type.Big)
                Array.Reverse(buffer);

            return BitConverter.ToUInt16(buffer, 0);
        }

        public int ReadInt24()
        {
            return this.ReadInt24(endianstyle);
        }
        public int ReadInt24(Endian.Type EndianType)
        {
            byte[] sourceArray = base.ReadBytes(3);
            byte[] destinationArray = new byte[4];
            Array.Copy(sourceArray, 0, destinationArray, 0, 3);
            if (EndianType == Endian.Type.Big)
            {
                Array.Reverse(destinationArray);
            }
            return BitConverter.ToInt32(destinationArray, 0);
        }

        public override int ReadInt32()
        {
            return ReadInt32(endianstyle);
        }
        public int ReadInt32(Type Type)
        {
            byte[] buffer = base.ReadBytes(4);

            if (Type == Type.Big)
                Array.Reverse(buffer);

            return BitConverter.ToInt32(buffer, 0);
        }

        public override uint ReadUInt32()
        {
            return ReadUInt32(endianstyle);
        }
        public uint ReadUInt32(Type Type)
        {
            byte[] buffer = base.ReadBytes(4);

            if (Type == Type.Big)
                Array.Reverse(buffer);

            return BitConverter.ToUInt32(buffer, 0);
        }

        public override ulong ReadUInt64()
        {
            return ReadUInt64(endianstyle);
        }
        public ulong ReadUInt64(Type Type)
        {
            byte[] buffer = base.ReadBytes(8);

            if (Type == Type.Big)
                Array.Reverse(buffer);

            return BitConverter.ToUInt64(buffer, 0);
        }

        public override float ReadSingle()
        {
            return ReadSingle(endianstyle);
        }
        public float ReadSingle(Type Type)
        {
            byte[] buffer = base.ReadBytes(4);

            if (Type == Type.Big)
                Array.Reverse(buffer);

            return BitConverter.ToSingle(buffer, 0);
        }

        public override double ReadDouble()
        {
            return ReadDouble(endianstyle);
        }
        public double ReadDouble(Type Type)
        {
            byte[] buffer = base.ReadBytes(4);

            if (Type == Type.Big)
                Array.Reverse(buffer);

            return BitConverter.ToDouble(buffer, 0);
        }

        public string ReadAsciiString(int Length)
        {
            return ReadAsciiString(Length, endianstyle);
        }
        public string ReadAsciiString(int Length, Type Type)
        {
            string newString = "";
            int howMuch = 0;
            for (int x = 0; x < Length; x++)
            {
                char tempChar;
                try { tempChar = (char)ReadByte(); }
                catch { return ""; }
                howMuch++;
                if (tempChar != '\0')
                    newString += tempChar;
                else
                    break;
            }

            int size = (Length - howMuch) * sizeof(byte);
            BaseStream.Seek(size, SeekOrigin.Current);

            return newString;
        }

        public string ReadUnicodeString(int Length)
        {
            return ReadUnicodeString(Length, endianstyle);
        }
        public string ReadUnicodeString(int Length, Type Type)
        {
            string newString = "";
            int howMuch = 0;
            for (int x = 0; x < Length; x++)
            {
                char tempChar = (char)ReadUInt16(Type);
                howMuch++;
                if (tempChar != '\0')
                    newString += tempChar;
                else
                    break;
            }

            int size = (Length - howMuch) * sizeof(UInt16);
            BaseStream.Seek(size, SeekOrigin.Current);
            return newString;
        }

        public override byte[] ReadBytes(int count)
        {
            return ReadBytes(count, endianstyle);
        }
        public byte[] ReadBytes(int count, Type type)
        {
            byte[] X = base.ReadBytes(count);
            if (type == Type.Big)
                Array.Reverse(X);
            return X;
        }

        public int PeekByte()
        {
            try
            {
                int i = base.ReadByte();
                base.BaseStream.Position -= 1;
                return i;
            }
            catch { return -1; }
        }

        #region Disposition
        ~Reader()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool DisposeManaged)
        {
            base.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion
    }

    public class Writer : BinaryWriter, IDisposable
    {

        Type endianstyle;

        public Writer(Stream stream, Type endianstyle)
            : base(stream)
        {
            this.endianstyle = endianstyle;
        }

        public override void Write(short value)
        {
            Write(value, endianstyle);
        }
        public void Write(short value, Type Type)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (Type == Type.Big)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(ushort value)
        {
            Write(value, endianstyle);
        }
        public void Write(ushort value, Type Type)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (Type == Type.Big)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(int value)
        {
            Write(value, endianstyle);
        }
        public void Write(int value, Type Type)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (Type == Type.Big)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(uint value)
        {
            Write(value, endianstyle);
        }
        public void Write(uint value, Type Type)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (Type == Type.Big)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(long value)
        {
            Write(value, endianstyle);
        }
        public void Write(long value, Type Type)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (Type == Type.Big)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(ulong value)
        {
            Write(value, endianstyle);
        }
        public void Write(ulong value, Type Type)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (Type == Type.Big)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(float value)
        {
            Write(value, endianstyle);
        }
        public void Write(float value, Type Type)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (Type == Type.Big)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public override void Write(double value)
        {
            Write(value, endianstyle);
        }
        public void Write(double value, Type Type)
        {
            byte[] buffer = BitConverter.GetBytes(value);
            if (Type == Type.Big)
                Array.Reverse(buffer);

            base.Write(buffer);
        }

        public void WriteAsciiString(string String, int Length)
        {
            WriteAsciiString(String, Length, endianstyle);
        }
        public void WriteAsciiString(string String, int Length, Type Type)
        {
            int strLen = String.Length;
            for (int x = 0; x < strLen; x++)
            {
                if (x > Length)
                    break;

                byte val = (byte)String[x];
                Write(val);
            }

            int nullSize = (Length - strLen) * sizeof(byte);
            if (nullSize > 0)
                Write(new byte[nullSize]);
        }

        public void WriteUnicodeString(string String, int Length)
        {
            WriteUnicodeString(String, Length, endianstyle);
        }
        public void WriteUnicodeString(string String, int Length, Type Type)
        {
            int strLen = String.Length;
            for (int x = 0; x < strLen; x++)
            {
                if (x > Length)
                    break;

                ushort val = (ushort)String[x];
                Write(val, Type);
            }

            int nullSize = (Length - strLen) * sizeof(ushort);
            if (nullSize > 0)
                Write(new byte[nullSize]);
        }

        #region Disposition
        ~Writer()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected void Dispose(bool DisposeManaged)
        {
            base.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion
    }

    public static class Conversions
    {
        public static string HexToAscii(byte[] hex)
        {
            string ascii = "";
            for (int i = 0; i < hex.Length; i++)
            {
                string temp = hex[i].ToString("X");
                if (temp.Length == 1)
                    temp = "0" + temp;
                ascii += temp;
            }
            return ascii;
        }

        public static string BytesToHexString(byte[] buffer, bool AddSpace)
        {
            if (AddSpace)
            {
                string a = BitConverter.ToString(buffer).Replace("-", " ");
                return a;
            }
            else
            {
                string a = BitConverter.ToString(buffer).Replace("-", string.Empty);
                return a;
            }
        }

        public static byte[] HexToBytes(string txt)
        {
            txt = txt.Replace(" ", string.Empty).Replace("-", string.Empty);
            List<byte> list = new List<byte>();
            for (int i = 0; i < (txt.Length / 2); i++)
            {
                if (txt.Length > (i + 1))
                {
                    txt.Substring(i * 2, 2);
                    list.Add(byte.Parse(txt.Substring(i * 2, 2), NumberStyles.HexNumber));
                }
            }
            return list.ToArray();
        }

        public static byte[] AsciiToHex(string ascii)
        {
            byte[] bytes = new byte[ascii.Length / 2];
            for (int i = 0; i < ascii.Length / 2; i++)
            {
                byte temp = byte.Parse(ascii.Substring(i * 2, 2), NumberStyles.HexNumber);
                bytes[i] = temp;
            }
            return bytes;
        }

        public static byte[] FlipBytesBy8(byte[] input)
        {
            byte[] flippedBytes = new byte[input.Length];
            int posInput = input.Length - 8;
            int posFlipped = 0;
            for (int i = 0; i < input.Length / 8; i++)
            {
                for (int x = 0; x < 8; x++)
                {
                    flippedBytes[posFlipped + x] = input[posInput + x];
                }

                posInput -= 8;
                posFlipped += 8;
            }

            return flippedBytes;
        }

        public static string LongToHex(long Decimal)
        {
            return Decimal.ToString("X").Replace("-", string.Empty);
        }
    }
}

public static class Extensions
{
    public static byte[] GetBytes(this string str, bool Unicode)
    {
        List<byte> bytes = new List<byte>();
        foreach (char c in str.ToCharArray())
            if (!Unicode)
                bytes.Add((byte)c);
            else
                bytes.AddRange(new byte[2] { (byte)c, 0 });
        if (Unicode)
            bytes.RemoveAt(bytes.ToArray().Length - 1);
        return bytes.ToArray();
    }
}