using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Dash.Engine.Graphics
{
    /// <summary>
    /// Provides methods for quickly manipulating bitmap data.
    /// </summary>
    public class LockedBitmap
    {
        public Bitmap Source { get; private set; }
        public IntPtr Pointer { get; private set; }

        public byte[] Pixels { get; private set; }
        public int Depth { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public BitmapData Data { get; private set; }

        public LockedBitmap(Bitmap source)
        {
            this.Source = source;
            LockBits();
        }

        /// <summary>
        /// Lock bitmap data
        /// </summary>
        public void LockBits()
        {
            try
            {
                // Get width and height of bitmap
                Width = Source.Width;
                Height = Source.Height;

                // get total locked pixels count
                int PixelCount = Width * Height;

                // Create rectangle to lock
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, Width, Height);

                // get source bitmap pixel format size
                Depth = Image.GetPixelFormatSize(Source.PixelFormat);

                // Check if bpp (Bits Per Pixel) is 8, 24, or 32
                if (Depth != 8 && Depth != 24 && Depth != 32)
                {
                    throw new ArgumentException("Only 8, 24 and 32 bpp images are supported.");
                }

                // Lock bitmap and return bitmap data
                Data = Source.LockBits(rect, ImageLockMode.ReadWrite,
                                             Source.PixelFormat);

                // create byte array to copy pixel values
                int step = Depth / 8;
                Pixels = new byte[PixelCount * step];
                Pointer = Data.Scan0;

                // Copy data from pointer to array
                Marshal.Copy(Pointer, Pixels, 0, Pixels.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Get the color of the specified pixel
        /// </summary>
        public Color GetPixel(int x, int y)
        {
            Color clr = Color.Empty;

            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            if (Depth == 32) // For 32 bpp get Red, Green, Blue and Alpha
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                byte a = Pixels[i + 3]; // a
                clr = new Color(r, g, b, a);
            }
            if (Depth == 24) // For 24 bpp get Red, Green and Blue
            {
                byte b = Pixels[i];
                byte g = Pixels[i + 1];
                byte r = Pixels[i + 2];
                clr = new Color(r, g, b);
            }
            if (Depth == 8)
            // For 8 bpp get color value (Red, Green and Blue values are the same)
            {
                byte c = Pixels[i];
                clr = new Color(c, c, c);
            }
            return clr;
        }

        public byte GetComponent(int x, int y, int component)
        {
            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            return Pixels[i + component];
        }

        /// <summary>
        /// Set the color of the specified pixel
        /// </summary>
        public void SetPixel(int x, int y, Color color)
        {
            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = ((y * Width) + x) * cCount;

            if (Depth == 32) // For 32 bpp set Red, Green, Blue and Alpha
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
                Pixels[i + 3] = color.A;
            }
            if (Depth == 24) // For 24 bpp set Red, Green and Blue
            {
                Pixels[i] = color.B;
                Pixels[i + 1] = color.G;
                Pixels[i + 2] = color.R;
            }
            if (Depth == 8)
            // For 8 bpp set color value (Red, Green and Blue values are the same)
            {
                Pixels[i] = color.B;
            }
        }

        public void SetComponent(int x, int y, int component, byte value)
        {
            // Get color components count
            int cCount = Depth / 8;

            // Get start index of the specified pixel
            int i = (((y * Width) + x) * cCount) + component;

            if (i > Pixels.Length - cCount)
                throw new IndexOutOfRangeException();

            Pixels[i] = value;
        }

        public void Clear(byte r, byte g, byte b, byte a)
        {
            Color color = new Color(r, g, b, a);
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    SetPixel(x, y, color);

            Save();
        }

        public void Clear24(byte r, byte g, byte b)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    SetComponent(x, y, 0, b);
                    SetComponent(x, y, 1, g);
                    SetComponent(x, y, 2, r);
                }

            Save();
        }

        public LockedBitmap DownScale(int newWidth, int newHeight)
        {
            LockedBitmap newBitmap = new LockedBitmap(new Bitmap(newWidth, newHeight, Source.PixelFormat));

            if (Source.PixelFormat != PixelFormat.Format32bppArgb)
                throw new Exception("DownsScale32 only works on 32 bit images");

            float xscale = (float)Data.Width / newWidth;
            float yscale = (float)Data.Height / newHeight;

            byte r = 0, g = 0, b = 0, a = 0;
            float summedR = 0f;
            float summedG = 0f;
            float summedB = 0f;
            float summedA = 0f;

            int left, right, top, bottom; //the area of old pixels covered by the new bitmap

            float targetStartX, targetEndX;
            float targetStartY, targetEndY;

            float leftF, rightF, topF, bottomF; //edges of new pixel in old pixel coords
            float weight;
            float weightScale = xscale * yscale;
            float totalColourWeight = 0f;

            for (int m = 0; m < newHeight; m++)
            {
                for (int n = 0; n < newWidth; n++)
                {
                    leftF = n * xscale;
                    rightF = (n + 1) * xscale;

                    topF = m * yscale;
                    bottomF = (m + 1) * yscale;

                    left = (int)leftF;
                    right = (int)rightF;

                    top = (int)topF;
                    bottom = (int)bottomF;

                    if (left < 0) left = 0;
                    if (top < 0) top = 0;
                    if (right >= Data.Width) right = Data.Width - 1;
                    if (bottom >= Data.Height) bottom = Data.Height - 1;

                    summedR = 0f;
                    summedG = 0f;
                    summedB = 0f;
                    summedA = 0f;
                    totalColourWeight = 0f;

                    for (int j = top; j <= bottom; j++)
                    {
                        for (int i = left; i <= right; i++)
                        {
                            targetStartX = Math.Max(leftF, i);
                            targetEndX = Math.Min(rightF, i + 1);

                            targetStartY = Math.Max(topF, j);
                            targetEndY = Math.Min(bottomF, j + 1);

                            weight = (targetEndX - targetStartX) * (targetEndY - targetStartY);

                            Color newColors = GetPixel(i, j);
                            r = newColors.R;
                            g = newColors.G;
                            b = newColors.B;
                            a = newColors.A;

                            summedA += weight * a;

                            if (a != 0)
                            {
                                summedR += weight * r;
                                summedG += weight * g;
                                summedB += weight * b;
                                totalColourWeight += weight;
                            }

                        }
                    }

                    summedR /= totalColourWeight;
                    summedG /= totalColourWeight;
                    summedB /= totalColourWeight;
                    summedA /= weightScale;

                    if (summedR < 0) summedR = 0f;
                    if (summedG < 0) summedG = 0f;
                    if (summedB < 0) summedB = 0f;
                    if (summedA < 0) summedA = 0f;

                    if (summedR >= 256) summedR = 255;
                    if (summedG >= 256) summedG = 255;
                    if (summedB >= 256) summedB = 255;
                    if (summedA >= 256) summedA = 255;

                    newBitmap.SetPixel(n, m, new Color((int)summedR, (int)summedG, (int)summedB, (int)summedA));
                }
            }

            // Unlock old bitmap
            UnlockBits();
            return newBitmap;
        }

        /// <summary>
        /// Returns true when the pixel specified is black.
        /// </summary>
        public static bool EmptyPixelAt(LockedBitmap lockedBitmap, int x, int y)
        {
            Color color = lockedBitmap.GetPixel(x, y);
            return color.R == 0 && color.G == 0 && color.B == 0;
        }

        /// <summary>
        /// Returns true when the pixel specified has an alpha 
        /// lower than or equal to the specified tolerance.
        /// </summary>
        public static bool EmptyAlphaPixelAt(LockedBitmap lockedBitmap, int x, int y, byte tolerance)
        {
            return lockedBitmap.GetComponent(x, y, 3) <= tolerance;
        }

        public static void BlitMask(LockedBitmap source, LockedBitmap target, System.Drawing.Rectangle srcRect, int destX, int destY)
        {
            if (source.Depth != 24 || target.Depth != 32)
                throw new Exception("BlitMask requires the source to be 24-bit and the target to be 32-bit");

            int targetStartX = Math.Max(destX, 0);
            int targetEndX = Math.Min(destX + srcRect.Width, target.Width);
            int targetStartY = Math.Max(destY, 0);
            int targetEndY = Math.Min(destY + srcRect.Height, target.Height);

            int copyW = targetEndX - targetStartX;
            int copyH = targetEndY - targetStartY;

            if (copyW < 0 || copyH < 0)
                return;

            int sourceStartX = srcRect.X + targetStartX - destX;
            int sourceStartY = srcRect.Y + targetStartY - destY;

            for (int sx = sourceStartX, dx = targetStartX; dx < targetEndX; sx++, dx++)
                for (int sy = sourceStartY, dy = targetStartY; dy < targetEndY; sy++, dy++)
                {
                    Color sourcePixel = source.GetPixel(sx, sy);
                    int lume = sourcePixel.R + sourcePixel.G + sourcePixel.B;
                    lume /= 3;

                    if (lume > 255)
                        lume = 255;

                    target.SetComponent(dx, dy, 3, (byte)lume);
                }
        }

        public static void Blit(LockedBitmap source, LockedBitmap target, System.Drawing.Rectangle srcRect, int destX, int destY)
        {
            for (int sx = srcRect.X, dx = destX; sx < srcRect.X + srcRect.Width; sx++, dx++)
                for (int sy = srcRect.Y, dy = destY; sy < srcRect.Y + srcRect.Height; sy++, dy++)
                {
                    Color sourcePixel = source.GetPixel(sx, sy);
                    target.SetPixel(dx, dy, sourcePixel);
                }
        }

        public void Save()
        {
            // Copy data from byte array to pointer
            Marshal.Copy(Pixels, 0, Pointer, Pixels.Length);
            UnlockBits();
            Data = Source.LockBits(new System.Drawing.Rectangle(0, 0, Source.Width, Source.Height), ImageLockMode.ReadWrite, Source.PixelFormat);
        }

        /// <summary>
        /// Unlock bitmap data
        /// </summary>
        public void UnlockBits()
        {
            // Unlock bitmap data
            Source.UnlockBits(Data);
        }
    }
}
