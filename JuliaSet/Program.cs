using SFML;
using SFML.Graphics;
using SFML.Window;
using SFML.System;

using ColorMine.ColorSpaces;
using ColorMine.ColorSpaces.Conversions;

using System.Diagnostics;

namespace JuliaSet
{
    class Program
    {
        public const int WIDTH = 1280;
        public const int HEIGHT = 768;

        const int MAX_ITERATIONS = 1000;
        const int THREADS = 16;

        public static double scale = 1.0;
        public static Vector2D resolution = new Vector2D(WIDTH, HEIGHT);

        public static int samples = 1;

        static Complex constant = new Complex(-0.8, 0.156);

        static double[] cutoffs = { 0.0, 0.03, 0.05, 0.25, 0.5, 0.85, 1.00};
        static Color[] colors = {
                new Color(25, 24, 23),       // Red
                new Color(120, 90, 70),     // Orange
                new Color(130, 24, 23),     // Yellow
                new Color(250, 179, 100),       // Green
                new Color(43, 65, 98),       // Blue
                new Color(11, 110, 79),       // Indigo
                new Color(150, 110, 79),       // Indigo

            };

        static Random random = new Random(442442344);

        public static void Main(string[] args)
        {

            RenderWindow window = new RenderWindow(new VideoMode(WIDTH, HEIGHT), "Julia Set");
            window.Closed += (sender, e) => window.Close();
            window.KeyPressed += KeyPressed;

            Image image = new Image(WIDTH, HEIGHT);
            Texture texture = new Texture(image);
            Sprite sprite = new Sprite(texture);

            Font font = new Font("arial.ttf");
            Text text = new Text("", font);

            Stopwatch stopwatch = new Stopwatch();


            while (window.IsOpen)
            {
                window.DispatchEvents();
                window.Clear(Color.Black);
                stopwatch.Restart();
                SetImagePixel(image);
 
                texture.Update(image);
                sprite.Texture = texture;

                window.Draw(sprite);

                double fps = 1.0 / stopwatch.Elapsed.TotalSeconds;
                text.DisplayedString = $"FPS: {fps:0.00}";
                window.Draw(text);
                window.Display();
            }
        }

        private static void SetImagePixel(Image image)
        {
            int stripHeight = HEIGHT / THREADS;
            Parallel.For(0, THREADS, i =>
            {
                int startY = i * stripHeight;
                int endY = (i + 1) * stripHeight;
                for (int y = startY; y < endY; y++)
                {
                    for (int x = 0; x < WIDTH; x++)
                    {
                        Complex c = ScreenToComplex(new Vector2D(x, y), resolution);
                        Complex constant = new Complex(-0.8, 0.156);

                        double smooth = JuliaSet.GetIterationsSmootReduceFunctionOpti(c, constant, MAX_ITERATIONS);
                        smooth = smooth / MAX_ITERATIONS;
                        Color color = GetColorPalette(smooth);
                        image.SetPixel((uint)x, (uint)y, color);

                    }
                }

            });
        }
        private static void SetImagePixelSamples(Image image)
        {
            
            int stripHeight = HEIGHT / THREADS;
            Parallel.For(0, THREADS, i =>
            {
                int startY = i * stripHeight;
                int endY = (i + 1) * stripHeight;
                for (int y = startY; y < endY; y++)
                {
                    for (int x = 0; x < WIDTH; x++)
                    {
                        Color finalColor = new Color(0, 0, 0);
                        for (int s = 0; s < samples; s++)
                        {
                            
                            double offsetX = random.NextDouble();
                            double offsetY = random.NextDouble();

                            Complex c = ScreenToComplex(new Vector2D(x + offsetX, y + offsetY), resolution);
                           
                            double smooth = JuliaSet.GetIterationsSmootReduceFunctionOpti(c, constant, MAX_ITERATIONS);

                            smooth = smooth / MAX_ITERATIONS;
                            finalColor += GetColorPalette(smooth);
                        }
                        byte r = (byte)(finalColor.R / samples);
                        byte g = (byte)(finalColor.G / samples);
                        byte b = (byte)(finalColor.B / samples);
                        finalColor = new Color(r, g, b);
                        image.SetPixel((uint)x, (uint)y, finalColor);
                    }
                }

            });
        }

        public static Color GetColor(double smooth)
        {
            double hue = 360 * smooth / MAX_ITERATIONS;
            double saturation = 1;
            double value = 1;

            Hsv hsv = new Hsv { H = hue, S = saturation, V = value };
            Rgb rgb = hsv.To<Rgb>();

            byte r = (byte)rgb.R;
            byte g = (byte)rgb.G;
            byte b = (byte)rgb.B;
            return new Color(r, g, b);
        }
        public static Color GetColorPalette(double smooth)
        {
            if (smooth > 1)
            {
                Console.WriteLine(smooth);
            }

            int index = Array.FindIndex(cutoffs, c => smooth <= c);

            if (index == -1)
                index = colors.Length - 1;

            double factor = (smooth - cutoffs[index - 1]) / (cutoffs[index] - cutoffs[index - 1]);

            Color color = Lerp(colors[index - 1], colors[index], (float)factor);

            return color;
        }
        public static Color Lerp(Color a, Color b, double t)
        {
            t = Math.Max(0, Math.Min(1, t));

            byte r = (byte)(a.R + (b.R - a.R) * t);
            byte g = (byte)(a.G + (b.G - a.G) * t);
            byte bl = (byte)(a.B + (b.B - a.B) * t);
            return new Color(r, g, bl);
        }
        public static Color GetColorLab(double smooth)
        {
            var lab = new Lab { L = smooth, A = 0, B = 0 };
            var rgb = lab.To<Rgb>();
            return new Color((byte)rgb.R, (byte)rgb.G, (byte)rgb.B);
        }
        static Complex ScreenToComplex(Vector2D screenCoords, Vector2D resolution)
        {
            double resolutionX = resolution.X;
            double resolutionY = resolution.Y;

            double screenCoordsX = screenCoords.X;
            double screenCoordsY = screenCoords.Y;

            double real = 2.5 * (screenCoordsX - resolutionX / 2) / (resolutionX) * scale;
            double imaginary = 2.0 * (screenCoordsY - resolutionY / 2) / (resolutionY) * scale;
            return new Complex(real, imaginary);
        }

        private static void KeyPressed(object? sender, KeyEventArgs e)
        {
            if (e.Code == Keyboard.Key.W)
            {
                scale *= 0.9;
            }
            if (e.Code == Keyboard.Key.S)
            {
                scale *= 1.1;
            }
            if (e.Code == Keyboard.Key.Up)
            {
                JuliaSet.offsetY -= 0.1 * scale;
            }
            if (e.Code == Keyboard.Key.Down)
            {
                JuliaSet.offsetY += 0.1 * scale;
            }
            if (e.Code == Keyboard.Key.Left)
            {
                JuliaSet.offsetX -= 0.1 * scale;
            }
            if (e.Code == Keyboard.Key.Right)
            {
                JuliaSet.offsetX += 0.1 * scale;
            }
            if (e.Code == Keyboard.Key.Num1)
            {
                samples = 1;
            }
            if (e.Code == Keyboard.Key.Num2)
            {
                samples = 2;
            }
            if (e.Code == Keyboard.Key.Num3)
            {
                samples = 4;
            }
            if (e.Code == Keyboard.Key.Num4)
            {
                samples = 8;
            }
        }
    }
}