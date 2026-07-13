using System;
using System.Text;
using FixMath.NET;

namespace GameScene.Simulation.Core
{
    /// <summary>
    /// Stable little-endian state serializer and FNV-1a 64-bit hash accumulator.
    /// Field order is part of the simulation protocol.
    /// </summary>
    public sealed class CanonicalStateWriter
    {
        private const ulong OffsetBasis = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;
        private ulong hash = OffsetBasis;

        public ulong Hash => hash;

        public void WriteBoolean(bool value) => WriteByte(value ? (byte)1 : (byte)0);
        public void WriteInt32(int value) => WriteUInt32(unchecked((uint)value));
        public void WriteInt64(long value) => WriteUInt64(unchecked((ulong)value));
        public void WriteFixed64(Fix64 value) => WriteInt64(value.RawValue);
        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteInt32(-1);
                return;
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            WriteInt32(bytes.Length);
            for (int index = 0; index < bytes.Length; index++) WriteByte(bytes[index]);
        }

        public void WriteUInt32(uint value)
        {
            for (int index = 0; index < sizeof(uint); index++)
            {
                WriteByte(unchecked((byte)(value >> (index * 8))));
            }
        }

        public void WriteUInt64(ulong value)
        {
            for (int index = 0; index < sizeof(ulong); index++)
            {
                WriteByte(unchecked((byte)(value >> (index * 8))));
            }
        }

        private void WriteByte(byte value)
        {
            hash = unchecked((hash ^ value) * Prime);
        }
    }
}
