using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using SlimeSim.Models;

namespace SlimeSim.Gpu
{
    public class SolverProgram
    {
        public int AgentsBuffer => agentsBuffer;

        public int GreenTex => greenTexB;

        public int BlueTex => blueTexB;

        public int RedTex => redTexB;

        private int moveProgram;

        private int markHeadProgram;

        private int markTailProgram;

        private int collisionsProgram;

        private int configBuffer;

        private int agentsBuffer;

        private int networkBuffer;

        private int greenTexA = 0;

        private int greenTexB = 0;

        private int blueTexA = 0;

        private int blueTexB = 0;

        private int redTexA = 0;

        private int redTexB = 0;

        private int currentAgentsCount = 0;

        private int currentNetworkLen = 0;

        private int currentWidth = 0;

        private int currentHeight = 0;

        private int maxGroupsX;

        private int blurProgram;

        private int blurInGreenLocation;

        private int blurInBlueLocation;

        private int blurInRedLocation;

        private int blurTexelSizeLocation;

        private int blurKernelRedLocation;

        private int blurKernelGreenLocation;

        private int blurKernelBlueLocation;

        private int fboA;

        private int fboB;

        private int vao;

        private int vbo;

        private int trackingBuffer;

        private Agent trackedAgent;

        public SolverProgram()
        {
            moveProgram = ShaderUtil.CompileAndLinkComputeShader("move.comp");
            markTailProgram = ShaderUtil.CompileAndLinkComputeShader("mark_tail.comp");
            markHeadProgram = ShaderUtil.CompileAndLinkComputeShader("mark_head.comp");
            collisionsProgram = ShaderUtil.CompileAndLinkComputeShader("collision.comp");
            GpuUtil.CreateBuffer(ref configBuffer, 1, Marshal.SizeOf<ShaderConfig>());

            blurProgram = ShaderUtil.CompileAndLinkRenderShader("blur.vert", "blur.frag");
            blurInGreenLocation = GL.GetUniformLocation(blurProgram, "inGreen");
            if (blurInGreenLocation == -1) throw new Exception("Uniform 'inGreen' not found. Shader optimized it out?");
            blurInBlueLocation = GL.GetUniformLocation(blurProgram, "inBlue");
            if (blurInBlueLocation == -1) throw new Exception("Uniform 'inBlue' not found. Shader optimized it out?");
            blurInRedLocation = GL.GetUniformLocation(blurProgram, "inRed");
            if (blurInRedLocation == -1) throw new Exception("Uniform 'inRed' not found. Shader optimized it out?");
            blurTexelSizeLocation = GL.GetUniformLocation(blurProgram, "uTexelSize");
            if (blurTexelSizeLocation == -1) throw new Exception("Uniform 'uTexelSize' not found. Shader optimized it out?");
            blurKernelRedLocation = GL.GetUniformLocation(blurProgram, "uKernelRed");
            if (blurKernelRedLocation == -1) throw new Exception("Uniform 'uKernelRed' not found. Shader optimized it out?");
            blurKernelGreenLocation = GL.GetUniformLocation(blurProgram, "uKernelGreen");
            if (blurKernelGreenLocation == -1) throw new Exception("Uniform 'uKernelGreen' not found. Shader optimized it out?");
            blurKernelBlueLocation = GL.GetUniformLocation(blurProgram, "uKernelBlue");
            if (blurKernelBlueLocation == -1) throw new Exception("Uniform 'uKernelBlue' not found. Shader optimized it out?");

            (vao, vbo) = PolygonUtil.CreateQuad();

            GL.GetInteger((OpenTK.Graphics.OpenGL.GetIndexedPName)All.MaxComputeWorkGroupCount, 0, out maxGroupsX);

            //tracking buffer - initialized once
            GL.GenBuffers(1, out trackingBuffer);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, trackingBuffer);
            GL.BufferData(BufferTarget.ShaderStorageBuffer, Marshal.SizeOf<Agent>(), IntPtr.Zero, BufferUsageHint.DynamicDraw);
        }

        public void Run(ref ShaderConfig config, float[] kernelRed, float[] kernelGreen, float[] kernelBlue)
        {
            lock (this)
            {
                PrepareBuffers(config, currentNetworkLen);
                UploadConfig(ref config);

                // -------------------- move agents -----------------------
                GL.UseProgram(moveProgram);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, configBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, networkBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 6, trackingBuffer);
                GL.BindImageTexture(2, greenTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(3, blueTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(4, redTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.DispatchCompute(DispatchGroupsX(config.agentsCount), 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

                // ---------------------- mark agent position on texture -----------------------
                GL.UseProgram(markTailProgram);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, configBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
                GL.BindImageTexture(2, greenTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(3, blueTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(4, redTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.DispatchCompute(DispatchGroupsX(config.agentsCount), 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

                GL.UseProgram(markHeadProgram);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, configBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
                GL.BindImageTexture(2, greenTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(3, blueTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(4, redTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.DispatchCompute(DispatchGroupsX(config.agentsCount), 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

                // ---------------------- colisions -----------------------
                GL.UseProgram(collisionsProgram);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, configBuffer);
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
                GL.BindImageTexture(2, greenTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(3, blueTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.BindImageTexture(4, redTexA, 0, false, 0, TextureAccess.ReadWrite, SizedInternalFormat.Rgba32f);
                GL.DispatchCompute(DispatchGroupsX(config.agentsCount), 1, 1);
                GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);

                // ----------------------------- blur ------------------------------------------
                GL.Viewport(0, 0, config.width, config.height); //this is important for the blur.frag, later must be set to real viewport
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboB);

                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.ScissorTest);
                GL.Disable(EnableCap.Blend);

                GL.UseProgram(blurProgram);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, greenTexA);
                GL.Uniform1(blurInGreenLocation, 0);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, blueTexA);
                GL.Uniform1(blurInBlueLocation, 1);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, redTexA);
                GL.Uniform1(blurInRedLocation, 2);
                GL.Uniform2(blurTexelSizeLocation, 1.0f / config.width, 1.0f / config.height);
                GL.Uniform1(blurKernelRedLocation, 25, kernelRed);
                GL.Uniform1(blurKernelGreenLocation, 25, kernelGreen);
                GL.Uniform1(blurKernelBlueLocation, 25, kernelBlue);
                PolygonUtil.RenderTriangles(vao);

                // Swap
                (greenTexA, greenTexB) = (greenTexB, greenTexA);
                (blueTexA, blueTexB) = (blueTexB, blueTexA);
                (redTexA, redTexB) = (redTexB, redTexA);
                (fboA, fboB) = (fboB, fboA);
            }
        }

        private int DispatchGroupsX(int count) => Math.Clamp((count + ShaderUtil.LocalSizeX - 1) / ShaderUtil.LocalSizeX, 1, maxGroupsX);

        private void UploadConfig(ref ShaderConfig config)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, configBuffer);
            GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, Marshal.SizeOf<ShaderConfig>(), ref config);
        }

        public void UploadAgents(ShaderConfig config, Agent[] agents, float[] network)
        {
            lock (this)
            {
                PrepareBuffers(config, network.Length);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, agentsBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, agents.Length * Marshal.SizeOf<Agent>(), agents);

                GL.BindBuffer(BufferTarget.ShaderStorageBuffer, networkBuffer);
                GL.BufferSubData(BufferTarget.ShaderStorageBuffer, 0, network.Length * Marshal.SizeOf<float>(), network);
            }
        }

        public void DownloadAgents(Agent[] agents)
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, agentsBuffer);
            GL.GetBufferSubData(BufferTarget.ShaderStorageBuffer, IntPtr.Zero, agents.Length * Marshal.SizeOf<Agent>(), agents);
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
        }

        public Agent DownloadTrackedAgent()
        {
            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, trackingBuffer);

            GL.GetBufferSubData(
                BufferTarget.ShaderStorageBuffer,
                IntPtr.Zero,
                Marshal.SizeOf<Agent>(),
                ref trackedAgent
            );

            GL.BindBuffer(BufferTarget.ShaderStorageBuffer, 0);
            return trackedAgent;
        }

        private void PrepareBuffers(ShaderConfig config, int networkLen)
        {
            if (currentAgentsCount != config.agentsCount)
            {
                currentAgentsCount = config.agentsCount;
                GpuUtil.CreateBuffer(ref agentsBuffer, currentAgentsCount, Marshal.SizeOf<Agent>());
            }

            if (currentNetworkLen != networkLen)
            {
                currentNetworkLen = networkLen;
                GpuUtil.CreateBuffer(ref networkBuffer, currentNetworkLen, Marshal.SizeOf<float>());
            }

            if (currentWidth != config.width || currentHeight != config.height)
            {
                currentWidth = config.width;
                currentHeight = config.height;

                if (greenTexA != 0) GL.DeleteTexture(greenTexA);
                greenTexA = TextureUtil.CreateFloatTexture(config.width, config.height);
                if (greenTexB != 0) GL.DeleteTexture(greenTexB);
                greenTexB = TextureUtil.CreateFloatTexture(config.width, config.height);

                if (blueTexA != 0) GL.DeleteTexture(blueTexA);
                blueTexA = TextureUtil.CreateFloatTexture(config.width, config.height);
                if (blueTexB != 0) GL.DeleteTexture(blueTexB);
                blueTexB = TextureUtil.CreateFloatTexture(config.width, config.height);

                if (redTexA != 0) GL.DeleteTexture(redTexA);
                redTexA = TextureUtil.CreateFloatTexture(config.width, config.height);
                if (redTexB != 0) GL.DeleteTexture(redTexB);
                redTexB = TextureUtil.CreateFloatTexture(config.width, config.height);

                fboA = TextureUtil.CreateFboForTextures(greenTexA, blueTexA, redTexA);
                GL.ClearColor(0f, 0f, 0f, 0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                fboB = TextureUtil.CreateFboForTextures(greenTexB, blueTexB, redTexB);
                GL.ClearColor(0f, 0f, 0f, 0f);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }

        public void ClearTextures()
        {
            TextureUtil.ClearTexture(greenTexA);
            TextureUtil.ClearTexture(greenTexB);
            TextureUtil.ClearTexture(blueTexA);
            TextureUtil.ClearTexture(blueTexB);
            TextureUtil.ClearTexture(redTexA);
            TextureUtil.ClearTexture(redTexB);
        }

    }
}
