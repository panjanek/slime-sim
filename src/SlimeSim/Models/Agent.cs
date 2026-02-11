using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Mathematics;

namespace SlimeSim.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 80)]
    public struct Agent
    {
        [FieldOffset(0)]
        public Vector2 position;

        [FieldOffset(8)]
        public float angle;

        [FieldOffset(12)]
        public int type;

        [FieldOffset(16)]
        public float energy;

        [FieldOffset(20)]
        public uint age;

        [FieldOffset(24)]
        public int state;

        [FieldOffset(28)]
        public int nnOffset;

        [FieldOffset(32)]
        public int meals;

        [FieldOffset(36)]
        public int deaths;

        [FieldOffset(40)]
        public float energySpent;

        [FieldOffset(44)]
        public int flag;

        [FieldOffset(48)]
        public Vector2i currPixel;

        [FieldOffset(56)]
        public Vector2i prevPixel;

        [FieldOffset(64)]
        public float memory0;

        [FieldOffset(68)]
        public float memory1;

        [FieldOffset(72)]
        public float deltaAngle;

        [FieldOffset(76)]
        public uint survivalDuration;

        public void SetPosition(Vector2 pos)
        {
            position = pos;
            currPixel = new Vector2i((int)pos.X, (int)pos.Y);
            prevPixel = currPixel;
        }
    }
}
