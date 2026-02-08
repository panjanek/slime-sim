using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace SlimeSim.Gpu
{
    public static class TextureUtil
    {
        public static void CopyTexture2D(int srcTex, int dstTex, int width, int height)
        {
            GL.CopyImageSubData(
                srcTex, ImageTarget.Texture2D, 0,  // src
                0, 0, 0,
                dstTex, ImageTarget.Texture2D, 0,  // dst
                0, 0, 0,
                width, height, 1);
        }

        public static int CreateFloatTexture(int width, int height)
        {
            int tex = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, tex);

            GL.TexStorage2D(TextureTarget2d.Texture2D, 1,
                SizedInternalFormat.Rgba32f, width, height);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapS,
                (int)TextureWrapMode.Repeat);

            GL.TexParameter(TextureTarget.Texture2D,
                TextureParameterName.TextureWrapT,
                (int)TextureWrapMode.Repeat);


            ClearTexture(tex);

            return tex;
        }

        public static void ClearTexture(int tex)
        {
            // Clear to all zeros
            float[] clearColor = new float[] { 0f, 0f, 0f, 0f };
            GL.ClearTexImage(
                tex,
                0,
                PixelFormat.Rgba,
                PixelType.Float,
                clearColor);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static int CreateFboForTextures(int textureA, int textureB, int textureC)
        {
            int fbo = GL.GenFramebuffer();
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment0,
                TextureTarget.Texture2D,
                textureA,
                0
            );

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment1,
                TextureTarget.Texture2D,
                textureB,
                0
            );

            GL.FramebufferTexture2D(
                FramebufferTarget.Framebuffer,
                FramebufferAttachment.ColorAttachment2,
                TextureTarget.Texture2D,
                textureC,
                0
            );

            GL.DrawBuffers(3, new[]
            {
                DrawBuffersEnum.ColorAttachment0,
                DrawBuffersEnum.ColorAttachment1,
                DrawBuffersEnum.ColorAttachment2
            });

            var status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferErrorCode.FramebufferComplete)
                throw new Exception($"FBO incomplete: {status}");

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            return fbo;
        }

        public static void SaveBufferToFile(byte[] pixels, int width, int height, string fileName)
        {
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i + 3] = 255;   // force A = 255 for BGRA
            }

            using (Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var data = bmp.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb
                );

                System.Runtime.InteropServices.Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
                bmp.UnlockBits(data);
                bmp.Save(fileName, ImageFormat.Png);
            }
        }

        public static void FlipVertical(byte[] buffer, int width, int height)
        {
            int stride = width * 4;
            byte[] tempRow = new byte[stride];

            for (int y = 0; y < height / 2; y++)
            {
                int top = y * stride;
                int bottom = (height - 1 - y) * stride;

                System.Buffer.BlockCopy(buffer, top, tempRow, 0, stride);
                System.Buffer.BlockCopy(buffer, bottom, buffer, top, stride);
                System.Buffer.BlockCopy(tempRow, 0, buffer, bottom, stride);
            }
        }
    }
}
