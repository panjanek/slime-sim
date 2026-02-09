using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Compute.OpenCL;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SlimeSim.Models;

namespace SlimeSim.Gpu
{
    public class DisplayProgram
    {
        private int pointsProgram;

        private int pointsProjLocation;

        private int pointsZoomLocation;

        private int pointsOffsetLocation;

        private int dispProgram;

        private int greenImageLocation;

        private int blueImageLocation;

        private int redImageLocation;

        private int dispTexSizeLocation;

        private int dispProjLocation;

        private int dispMinLocation;

        private int dispMaxLocation;

        private int dummyVao;

        private Vector2 offset = new Vector2(0, 0);

        private Vector2 size = new Vector2(1, 1);
        public DisplayProgram() 
        {
            pointsProgram = ShaderUtil.CompileAndLinkRenderShader("points.vert", "points.frag");
            pointsProjLocation = GL.GetUniformLocation(pointsProgram, "projection");
            if (pointsProjLocation == -1) throw new Exception("Uniform 'projection' not found. Shader optimized it out?");
            pointsZoomLocation = GL.GetUniformLocation(pointsProgram, "zoom");
            if (pointsZoomLocation == -1) throw new Exception("Uniform 'zoom' not found. Shader optimized it out?");
            pointsOffsetLocation = GL.GetUniformLocation(pointsProgram, "offset");
            if (pointsOffsetLocation == -1) throw new Exception("Uniform 'offset' not found. Shader optimized it out?");

            dispProgram = ShaderUtil.CompileAndLinkRenderShader("display.vert", "display.frag");
            greenImageLocation = GL.GetUniformLocation(dispProgram, "uGreenImage");
            if (greenImageLocation == -1) throw new Exception("Uniform 'uGreenImage' not found. Shader optimized it out?");
            blueImageLocation = GL.GetUniformLocation(dispProgram, "uBlueImage");
            if (blueImageLocation == -1) throw new Exception("Uniform 'uBlueImage' not found. Shader optimized it out?");
            redImageLocation = GL.GetUniformLocation(dispProgram, "uRedImage");
            if (redImageLocation == -1) throw new Exception("Uniform 'uRedImage' not found. Shader optimized it out?");
            dispProjLocation = GL.GetUniformLocation(dispProgram, "projection");
            if (dispProjLocation == -1) throw new Exception("Uniform 'projection' not found. Shader optimized it out?");
            dispTexSizeLocation = GL.GetUniformLocation(dispProgram, "texSize");
            if (dispTexSizeLocation == -1) throw new Exception("Uniform 'texSize' not found. Shader optimized it out?");

            dispMinLocation = GL.GetUniformLocation(dispProgram, "worldMin");
            if (dispMinLocation == -1) throw new Exception("Uniform 'worldMin' not found. Shader optimized it out?");
            dispMaxLocation = GL.GetUniformLocation(dispProgram, "worldMax");
            if (dispMaxLocation == -1) throw new Exception("Uniform 'worldMax' not found. Shader optimized it out?");

            GL.GenVertexArrays(1, out dummyVao);
            GL.BindVertexArray(dummyVao);
        }

        public void Draw(Simulation simulation, Matrix4 projectionMatrix, int agentsBuffer, int greenTex, int blueTex, int redTex, Vector2 worldMin, Vector2 worldMax, float zoom, bool showPointers)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //draw texture
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Blend);
            GL.UseProgram(dispProgram);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, greenTex);
            GL.Uniform1(greenImageLocation, 0);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, blueTex);
            GL.Uniform1(blueImageLocation, 1);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, redTex);
            GL.Uniform1(redImageLocation, 2);
            GL.Uniform2(dispTexSizeLocation, new Vector2(simulation.shaderConfig.width, simulation.shaderConfig.height));
            GL.UniformMatrix4(dispProjLocation, false, ref projectionMatrix);
            GL.Uniform2(dispMinLocation, worldMin);
            GL.Uniform2(dispMaxLocation, worldMax);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
            

            //draw points
            if (showPointers)
            {
                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                        //GL.Enable(EnableCap.FramebufferSrgb);
                        GL.Enable(EnableCap.ProgramPointSize);
                        GL.Enable(EnableCap.Blend);
                        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                        GL.BlendEquation(BlendEquationMode.FuncAdd);

                        GL.Enable(EnableCap.PointSprite);
                        GL.UseProgram(pointsProgram);
                        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, agentsBuffer);
                        GL.BindVertexArray(dummyVao);
                        GL.UniformMatrix4(pointsProjLocation, false, ref projectionMatrix);
                        GL.Uniform1(pointsZoomLocation, zoom);
                        GL.Uniform2(pointsOffsetLocation, new Vector2(x * simulation.shaderConfig.width, y * simulation.shaderConfig.height));
                        GL.DrawArrays(PrimitiveType.Points, 0, simulation.agents.Length);
                    }
            }
        }
    }
}
