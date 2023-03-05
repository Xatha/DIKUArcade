using System;
using System.Diagnostics;
using System.IO;
using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL;
using DIKUArcade.Entities;
using DIKUArcade.GUI;
using StbImageSharp;
using PixelFormat = OpenTK.Graphics.OpenGL.PixelFormat;

namespace DIKUArcade.Graphics {
    public class Texture {
        /// <summary>
        /// OpenGL texture handle
        /// </summary>
        public static double offsetX = 0.0;
        public static double offsetY = 0.0;
        private int textureId;

        public Texture(string filename) {
            Console.WriteLine("Texture!!!");
            // create a texture id
            textureId = GL.GenTexture();

            // bind this new texture id
            BindTexture();

            // find base path
            var dir = new DirectoryInfo(Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location));

            while (dir.Name != "bin")
            {
                dir = dir.Parent;
            }
            dir = dir.Parent;

            // load image file
            var path = Path.Combine(dir.FullName, filename);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Error: The file \"{path}\" does not exist.");
            }
            
            // Load image with StbImageSharp.
            // This is recommended by OpenTK.https://opentk.net/learn/chapter1/5-textures.html?tabs=load-texture-opentk4
            ImageResult image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
            
            // attach it to OpenGL context
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                image.Width, image.Height, 0, PixelFormat.Rgba,
                PixelType.UnsignedByte, image.Data);
            
            // set texture properties, filters, blending functions, etc.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.AlphaTest);

            GL.AlphaFunc(AlphaFunction.Gequal, 0.5f);

            // unbind the texture
            UnbindTexture();
        }

        public Texture(string filename, int currentStride, int stridesInImage)
        {
            if (currentStride < 0 || currentStride >= stridesInImage || stridesInImage < 0)
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid stride numbers: ({currentStride}/{stridesInImage})");
            }

            // create a texture id
            textureId = GL.GenTexture();

            // bind this new texture id
            BindTexture();

            // find base path
            var dir = new DirectoryInfo(Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location));

            while (dir.Name != "bin")
            {
                dir = dir.Parent;
            }

            dir = dir.Parent;

            // load image file
            var path = Path.Combine(dir.FullName.ToString(), filename);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Error: The file \"{path}\" does not exist.");
            }

            //TODO: Refactor.
            ImageResult image = ImageResult.FromStream(File.OpenRead(path), ColorComponents.RedGreenBlueAlpha);
            
            int segmentWidth = image.Width / stridesInImage;
            int segmentHeight = image.Height;
            int currentX = 0;
            int currentY = 0;
            int xOffset = 0;
            int endX = segmentWidth;

            byte[][] segments = new byte[stridesInImage][];

            int segmentIndex = 0;
            
            for (int segmentOffset = 0; segmentOffset < stridesInImage; segmentOffset++)
            {
                byte[] segment = new byte[segmentWidth * segmentHeight * 4];

                while (currentY < segmentHeight)
                {
                    while (currentX < endX)
                    {
                        int index = (currentY * image.Width + currentX) * 4;
                    
                        segment[segmentIndex++] = image.Data[index];
                        segment[segmentIndex++] = image.Data[index + 1];
                        segment[segmentIndex++] = image.Data[index + 2];
                        segment[segmentIndex++] = image.Data[index + 3];
                    
                        currentX += 1;
                    }
                    currentX = xOffset;
                    currentY++;
                }
                currentX = endX;
                xOffset = endX;
                endX += segmentWidth;
                currentY = 0;

                segmentIndex = 0;
                segments[segmentOffset] = segment;
            }

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
                segmentWidth, segmentHeight, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba,
                PixelType.UnsignedByte, segments[currentStride]);

            // set texture properties, filters, blending functions, etc.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);

            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.AlphaTest);

            GL.AlphaFunc(AlphaFunction.Gequal, 0.5f);

            // unbind the texture
            UnbindTexture();
        }

        private void BindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, textureId);
        }

        private void UnbindTexture()
        {
            GL.BindTexture(TextureTarget.Texture2D, 0); // 0 is invalid texture id
        }

        private Matrix4 CreateMatrix(Shape shape)
        {
            // ensure that rotation is performed around the center of the shape
            // instead of the bottom-left corner
            var halfX = shape.Extent.X / 2.0f;
            var halfY = shape.Extent.Y / 2.0f;

            return Matrix4.CreateTranslation(-halfX, -halfY, 0.0f) *
                   Matrix4.CreateRotationZ(shape.Rotation) *
                   Matrix4.CreateTranslation(shape.Position.X + halfX, shape.Position.Y + halfY,
                       0.0f);
        }
        
        // Render things that are affected by a camera (if the game has one)
        private Matrix4 CreateMatrix(Shape shape, Camera camera)
        {
            // ensure that rotation is performed around the center of the shape
            // instead of the bottom-left corner
            var halfX = shape.Extent.X / 2.0f;
            var halfY = shape.Extent.Y / 2.0f;

            return Matrix4.CreateTranslation(
                -halfX - camera.Offset.X,
                -halfY - camera.Offset.Y,
                0.0f) *
                   Matrix4.CreateRotationZ(shape.Rotation) *
                   Matrix4.CreateTranslation(shape.Position.X + halfX + camera.Offset.X,
                    shape.Position.Y + halfY + camera.Offset.Y,
                       0.0f);
        }

        public void Render(Shape shape, Camera camera)
        {

            // bind this texture
            BindTexture();

            // render this texture
            Matrix4 modelViewMatrix = CreateMatrix(shape, camera);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelViewMatrix);

            GL.Translate(camera.Offset.X, camera.Offset.Y, 0);
            //GL.Scale(camera.Scale, camera.Scale, 1f);
            GL.Color4(1f, 1f, 1f, 1f);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 1); GL.Vertex2(0.0f, 0.0f);                      // Top Left
            GL.TexCoord2(0, 0); GL.Vertex2(0.0f, shape.Extent.Y);            // Bottom Left
            GL.TexCoord2(1, 0); GL.Vertex2(shape.Extent.X, shape.Extent.Y);  // Bottom Right
            GL.TexCoord2(1, 1); GL.Vertex2(shape.Extent.X, 0.0f);            // Top Right

            GL.End();

            // unbind this texture
            UnbindTexture();
        }
        
        public void Render(Shape shape)
        {

            // bind this texture
            BindTexture();

            // render this texture
            Matrix4 modelViewMatrix = CreateMatrix(shape);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelViewMatrix);

            GL.Color4(1f, 1f, 1f, 1f);
            GL.Begin(PrimitiveType.Quads);

            GL.TexCoord2(0, 1); GL.Vertex2(0.0f, 0.0f);                      // Top Left
            GL.TexCoord2(0, 0); GL.Vertex2(0.0f, shape.Extent.Y);            // Bottom Left
            GL.TexCoord2(1, 0); GL.Vertex2(shape.Extent.X, shape.Extent.Y);  // Bottom Right
            GL.TexCoord2(1, 1); GL.Vertex2(shape.Extent.X, 0.0f);            // Top Right

            GL.End();

            // unbind this texture
            UnbindTexture();
        }
    }
}
