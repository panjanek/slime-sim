using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SlimeSim.Models
{
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    public struct ShaderConfig
    {
        public ShaderConfig() { }

        [FieldOffset(0)]
        public int agentsCount = 5000;

        [FieldOffset(4)]
        public int width = 1920 * 1;

        [FieldOffset(8)]
        public int height = 1080 * 1;

        [FieldOffset(12)]
        public float dt = 0.1f;

        [FieldOffset(16)]
        public float t;

        [FieldOffset(20)]
        public int generationDuration = 5000;

        [FieldOffset(24)]
        public float initialEnergy = 300;

        [FieldOffset(28)]
        public float plantEnergy = 100;

        [FieldOffset(32)]
        public float killEnergy = 100;

        [FieldOffset(36)]
        public int plantRegrowDuration = 1000;

        [FieldOffset(40)]
        public int trackedIdx;

        [FieldOffset(44)]
        public float blueMaxVelocity = 0.3f;

        [FieldOffset(48)]
        public float redMaxVelocity = 0.5f;

        [FieldOffset(52)]
        public int pad0;

        [FieldOffset(56)]
        public int pad1;

        [FieldOffset(60)]
        public int pad2;
    }
}
