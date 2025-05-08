using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;

namespace UPG_SP_2024
{

    /// <summary>
    /// The main panel with the custom visualization
    /// </summary>
    public class DrawingPanel : Panel
    {
        private const float Epsilon0 = 8.854e-12f; //Vacuum permitivity
        private const float K = 1 / (float)(4 * Math.PI * Epsilon0);
        private Vector2D probePosition;   // probe's position
        private float probeRadius = 1.0f; // radius of the probe
        private int scenario;
        private System.Windows.Forms.Timer timer;
        private DateTime startTime;
        private float probeAngle; // current angle of the probe
        private Bitmap? intensityBitmap;
        private float[,] precomputedIntensities;
        private bool intensitiesComputed = false;
        public List<Charge> charges = new List<Charge>();
        public Charge? selectedCharge;
        private Dictionary<PointF, Vector2D[]> chargeFieldCache = new Dictionary<PointF, Vector2D[]>();
        private DateTime? secondProbeStartTime = null;
        public List<float> elFieldChartValues = new List<float>(); // values for chart's y axis 
        private Vector2D? secondProbePosition = null;
        public float secondProbeRadius;
        private float secondProbeAngle;
        private float secondProbeStartAngle;
        /// <summary>Initializes a new instance of the <see cref="DrawingPanel" /> class.</summary>
        public DrawingPanel()
        {
            //this.ClientSize = new System.Drawing.Size(800, 600);

            // Timer for displayed updates
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 50;
            timer.Tick += Timer_Tick;
            startTime = DateTime.Now;
        }

        // Initializing scenario
        public void InitializeScenario(int scenario)
        {
            this.scenario = scenario;
            SetScenarioConfiguration();
            
            timer.Start();
        }

        public void LoadScenarioFromFile(string path)
        {
            try
            {
                string jsonContent = File.ReadAllText(path);

                var options = new JsonSerializerOptions
                {
                    Converters = { new Vector2DJsonConverter() }
                };

                //loads charges from file with defined position convertor options in Vector2DJsonConvertor 
                var loadedCharges = JsonSerializer.Deserialize<List<Charge>>(jsonContent, options);

                if (loadedCharges != null)
                {
                    charges.Clear();
                    charges.AddRange(loadedCharges);

                    Console.WriteLine($"Added {charges.Count} charges from file.");
                    timer.Start();
                }
                else
                {
                    Console.WriteLine("No charges found in the file.");
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error loading charges: {e.Message}");
            }
        }



        // Method adding particular scenario configuration
        public void SetScenarioConfiguration()
        {

            switch (this.scenario)
            {
                // default scenario: +1C charge at origin
                case 0:
                    charges.Add(new Charge(new Vector2D(0, 0), 1.0f));
                    break;

                // scnario 1: two +1 charges, one at (-1,0), other at (1,0)
                case 1:

                    charges.Add(new Charge(new Vector2D(-1, 0), 1.0f));
                    charges.Add(new Charge(new Vector2D(1, 0), 1.0f));
                    break;

                // scenario 2: two charges, one -1C charge at (-1, 0), other +2C charge at (1,0)
                case 2:

                    charges.Add(new Charge(new Vector2D(-1, 0), -1.0f));
                    charges.Add(new Charge(new Vector2D(1, 0), 2.0f));
                    break;

                // scenario 3: four chages, first +1 charge at (-1,-1), second +2C charge ar (1, -1), third -3C charge at (1, 1) and fourth -4C charge at (-1, 1)
                case 3:
                    charges.Add(new Charge(new Vector2D(-1, -1), 1.0f));
                    charges.Add(new Charge(new Vector2D(1, -1), 2.0f));
                    charges.Add(new Charge(new Vector2D(1, 1), -3.0f));
                    charges.Add(new Charge(new Vector2D(-1, 1), -4.0f));
                    
                    break;

                case 4:
                    charges.Add(new Charge(new Vector2D(-1, 0), 1.0f));
                    charges.Add(new Charge(new Vector2D(1, 0), 1.0f));
                    break;

                // default scenario
                default:
                    charges.Add(new Charge(new Vector2D(0, 0), 1.0f));
                    break;


            }
        }
        // Method timer event handler for updating probe positoion
        private void Timer_Tick(object? sender, EventArgs e)
        {
            probeAngle += (float)(Math.PI / 6 * 0.05);

            double elapsed = (DateTime.Now - startTime).TotalSeconds;

            if (probeAngle > 2 * Math.PI)
            {
                probeAngle -= (float)(2 * Math.PI);
            }

            probePosition = new Vector2D(
                probeRadius * (float)Math.Cos(probeAngle),
                probeRadius * (float)Math.Sin(probeAngle)
                );

            // scenario with dynamic charge values
            if(scenario == 4)
            {
                charges[0].magnitute = 1.0f + 0.5f * (float)Math.Sin(Math.PI * elapsed);
                charges[1].magnitute = 1.0f - 0.5f * (float)Math.Sin(Math.PI * elapsed);
    
            }

            if(secondProbePosition != null)
            {
                secondProbeAngle += (float)(Math.PI / 6 * 0.05);

                if(secondProbeAngle > 2 * Math.PI)
                {
                    secondProbeAngle -= (float)(2 * Math.PI);
                }

                secondProbePosition = new Vector2D(secondProbeRadius * (float)Math.Cos(secondProbeAngle),
                                                   secondProbeRadius * (float)Math.Sin(secondProbeAngle));
            }
            
            this.Invalidate();
        }
        /// <summary>TODO: Custom visualization code comes into this method</summary>
        /// <remarks>Raises the <see cref="E:System.Windows.Forms.Control.Paint">Paint</see> event.</remarks>
        /// <param name="e">A <see cref="T:System.Windows.Forms.PaintEventArgs">PaintEventArgs</see> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Calling the base class OnPaint
            base.OnPaint(e);

            // world scale to screen coordinates
            float scaleX = this.Width / 4; // world space is defined from -2 to 2 on both axes centerd at (0,0)
            float scaleY = this.Height / 4;
            float scale = Math.Min(scaleX, scaleY);

            if(scenario != 4)
            {
                if(intensityBitmap != null && selectedCharge == null)
                {
                    g.DrawImage(intensityBitmap, 0, 0);
                }
                else
                {
                    DrawIntensityMap(g, scaleX, scaleY);
                    g.DrawImage(intensityBitmap, 0, 0);
                }
            }
            else
            {
                DrawIntensityMap(g, scaleX, scaleY);
                g.DrawImage(intensityBitmap, 0, 0);
            }

            DrawGrids(g, scaleX, scaleY);

            // Draw each charge as a circle
            foreach (var charge in charges)
            {

                Vector2D chargePos = WorldToScreen(charge.position, scaleX, scaleY);

              

                float radius = Math.Abs(charge.magnitute) * scale * 0.13f; // Size of the charge depends on its magnitute
                Color color = charge.magnitute > 0 ? Color.Red : Color.Blue;



                g.FillEllipse(new SolidBrush(color), chargePos.X - radius, chargePos.Y - radius, 2 * radius, 2 * radius);
                g.DrawEllipse(Pens.Black, chargePos.X - radius, chargePos.Y - radius, 2 * radius, 2 * radius);

                // Display charge of specified charge
                string label = $"{(charge.magnitute > 0 ? '+' : "")}{charge.magnitute:0.0}C";
                
                Font fnt = new Font("Arial", 1 * radius / 2);
                g.DrawString(label, fnt, Brushes.Black, chargePos.X - (int)(g.MeasureString(label, fnt).Width) / 2, chargePos.Y - 0.4f * radius);
            }

            // second probe calculation and display
            if(secondProbePosition != null)
            {
                Vector2D screenP = WorldToScreen(new Vector2D(secondProbePosition.Value.X, secondProbePosition.Value.Y), scaleX, scaleY);
                g.FillEllipse(Brushes.Yellow, screenP.X, screenP.Y, scale * 0.1f, scale * 0.1f);

                Vector2D field = CalcElectricFieldAt(new Vector2D(secondProbePosition.Value.X, secondProbePosition.Value.Y));
                DrawElectricField(g, screenP, field, scaleX, scaleY, Color.Yellow);
                float scalar = field.Magnitude();
                elFieldChartValues.Add(scalar);

                string s = $"{scalar:G2}";
                Font f = new Font("Menlo", 9, FontStyle.Bold);
                g.DrawString(s, f, Brushes.Black, screenP.X - (int)(g.MeasureString(s, f).Width / 2), screenP.Y - 2 * (int)(g.MeasureString(s, f).Height));
            }


            // calculate probe screen position
            Vector2D probeScreenPosition = WorldToScreen(new Vector2D(probePosition.X, probePosition.Y), scaleX, scaleY);
            g.FillEllipse(Brushes.Orange, probeScreenPosition.X, probeScreenPosition.Y, scale * 0.1f, scale * 0.1f); // draw probe

            Vector2D electricField = CalcElectricFieldAt(probePosition); // magnitute of electric field
            DrawElectricField(g, probeScreenPosition, electricField, scaleX, scaleY, Color.Orange); // draw a vector of intensity

            // Display the magnitute of electric field
            float scalarElField = electricField.Magnitude();
            string E = $"{scalarElField:G2}";
            Font fnt1 = new Font("Menlo", 9, FontStyle.Bold);
            g.DrawString(E, fnt1, Brushes.Black, probeScreenPosition.X - (int)(g.MeasureString(E, fnt1).Width / 2), probeScreenPosition.Y - 2 * (int)(g.MeasureString(E, fnt1).Height));


        }

        // calculate intesity at certain point
        private Vector2D CalcElectricFieldAt(Vector2D point)
        {
            Vector2D total = new Vector2D(0, 0);

            // following formula of columb's law
            foreach (var charge in charges)
            {
                Vector2D x = new Vector2D(charge.position.X - point.X, charge.position.Y - point.Y);

                float distance = x.Magnitude();

                if (distance > 0)
                {
                    Vector2D field = x * charge.magnitute / (float)Math.Pow(distance, 3);
                    total += field;
                }
            }

            return total * K;
        }

        //private Vector2D CalcElectricFieldAt(PointF point)
        //{
        //    if (!chargeFieldCache.ContainsKey(point))
        //    {
        //        var fields = charges.Select(charge =>
        //        {
        //            Vector2D r = new Vector2D(charge.position.X - point.X, charge.position.Y - point.Y);
        //            float distance = r.Magnitude();
        //            if (distance > 0)
        //            {
        //                return r * (charge.magnitute / (float)Math.Pow(distance, 3));
        //            }
        //            return new Vector2D(0, 0);
        //        }).ToArray();

        //        chargeFieldCache[point] = fields;
        //    }

        //    Vector2D total = new Vector2D(0, 0);
        //    foreach (var field in chargeFieldCache[point])
        //    {
        //        total += field;
        //    }
        //    return total * K;
        //}

        // draw vectors of intensity on the background 
        private void DrawGrids(Graphics g, float scaleX, float scaleY)
        {
            
            int gridSpacingX = MainForm.GridSpacingX; // horizontal spacing in pixels
            int gridSpacingY = MainForm.GridSpacingY; // vertical spacing in pixels

            Pen gridPen = new Pen(Color.DarkBlue, 1);

            // vertical lines
            for (int x = -this.Width / 2; x < this.Width / 2 + gridSpacingX; x += gridSpacingX)
            {
                g.DrawLine(gridPen, x + this.Width / 2, 0, x + this.Width / 2, this.Height);
            }

            // horizontal lines
            for (int y = -this.Height / 2; y < this.Height / 2 + gridSpacingY; y += gridSpacingY)
            {
                g.DrawLine(gridPen, 0, y + this.Height / 2, this.Width, y + this.Height / 2);
            }
            // iteration 
            for (int x = -this.Width / 2; x < this.Width / 2 + gridSpacingX; x += gridSpacingX)
            {
                for (int y = -this.Height / 2; y < this.Height / 2 + gridSpacingY; y += gridSpacingY)
                {
                    // calc point in space
                    Vector2D worldPoint = new Vector2D(x / scaleX, y / scaleY);

                    // calc el. intensity at 'worldPoint'
                    Vector2D field = CalcElectricFieldAt(worldPoint);

                    // convert to screen position
                    Vector2D screenPoint = new Vector2D(
                        x + this.Width / 2,
                        -y + this.Height / 2
                    );

                    // draw vector
                    DrawElectricField(g, screenPoint, field, scaleX, scaleY, Color.Black);
                }
            }
        }


        // draw arrow in direction of electric field
        private void DrawElectricField(Graphics g, Vector2D start, Vector2D field, float scaleX, float scaleY, Color arrowColor)
        {
            float thickness = arrowColor == Color.Orange || arrowColor == Color.Yellow ? 4f : 0.5f;

            var pen = new Pen(arrowColor, thickness);


            pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Miter;
            pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;

            float tipLen = 10f;

            float scale = Math.Min(scaleX, scaleY);

            // assure arrow starts from probe's center

            Vector2D centeredStart = new Vector2D(start.X + (scale * 0.1f / 2f), start.Y + (scale * 0.1f / 2f));

            // when calculating arrow for probe, it will continue to use {centeredStart}, else {start}
            Vector2D expression = (arrowColor == Color.Orange || arrowColor == Color.Yellow ? centeredStart : start);

            // normalizing vector
            field = field.Normalize();

            Vector2D end = new Vector2D(
                expression.X + field.X * scaleX * (expression == centeredStart ? 0.2f : 0.1f),
                expression.Y - field.Y * scaleY * (expression == centeredStart ? 0.2f : 0.1f)
                );

            Vector2D vector = new Vector2D(end.X - expression.X, end.Y - expression.Y);

            Vector2D norm = vector.Normalize();

            // point on vector norm * tipLen units away  form end point
            float cx = end.X - norm.X * tipLen;
            float cy = end.Y - norm.Y * tipLen;

            float d = 0.3f * tipLen;

            //points from both sides of vector (norm * d) away from point (cx, cy)
            float ex = cx + norm.Y * d;
            float ey = cy - norm.X * d;
            float fx = cx - norm.Y * d;
            float fy = cy + norm.X * d;

            PointF s = new PointF((int)expression.X, (int)expression.Y);
            PointF e = new PointF((int)end.X, (int)end.Y);
            g.DrawLine(pen, s, e);

            g.DrawLines(pen, new PointF[]
            {
                new PointF(ex, ey),
                new PointF(end.X,end.Y),
                new PointF(fx, fy)
            });
        }

        // sets IntensityBitmap Bitmap value -> high intensity: darker blue, lower: lighter blue 
        private void DrawIntensityMap(Graphics g, float sclX, float sclY)
        {

            if(scenario == 4 || selectedCharge != null)
            {
                intensitiesComputed = false;
            }

            PrecomputeIntensities(sclX, sclY);

            int cellSize = 8;

            var intensityMap = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            var bm = intensityMap.LockBits(new Rectangle(0, 0, this.Width, this.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            byte[] rgb = new byte[bm.Height * bm.Stride];

            Marshal.Copy(bm.Scan0, rgb, 0, rgb.Length);

            for(int y = 0; y < this.Height; y += cellSize)
            {
                for(int x = 0; x < this.Width; x += cellSize)
                {

                    int gridX = Math.Min(x / cellSize, precomputedIntensities.GetLength(0) - 1);
                    int gridY = Math.Min(y / cellSize, precomputedIntensities.GetLength(1) - 1);
                    
                    float normIntensity = this.precomputedIntensities[gridX, gridY];
                    Color color = GetPixelColor(normIntensity);

                    for (int subY = y; subY < y + cellSize && subY < this.Height; subY++)
                    {
                        for (int subX = x; subX < x + cellSize && subX < this.Width; subX++)
                        {
                            int i = (subY * bm.Stride) + (subX * 3);
                            rgb[i] = color.B;
                            rgb[i + 1] = color.G;
                            rgb[i + 2] = color.R;
                        }
                    }
                }
            }

            Marshal.Copy(rgb, 0, bm.Scan0, rgb.Length);

            intensityMap.UnlockBits(bm);
            
            
            this.intensityBitmap = intensityMap;
        }

        // return color at given point according to its intensity
        private Color GetPixelColor(float t)
        {

            Color lightBlue = Color.FromArgb(173, 216, 230); // Light blue
            Color darkBlue = Color.FromArgb(0, 0, 139);     // Dark blue

            int r = (int)(lightBlue.R + t * (darkBlue.R - lightBlue.R));
            int g = (int)(lightBlue.G + t * (darkBlue.G - lightBlue.G));
            int b = (int)(lightBlue.B + t * (darkBlue.B - lightBlue.B));


            return Color.FromArgb(r, g, b);
        }


        // return maximum intensity of electric field
        private float GetMaxIntensity(float scaleX, float scaleY)
        {
            float maxIntensity = 0f;

            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {

                    Vector2D worldPoint = ScreenToWorld(new Vector2D(x, y), scaleX, scaleY);

                    Vector2D field = CalcElectricFieldAt(worldPoint);
                    float intensity = field.Magnitude();


                    if (intensity > maxIntensity)
                    {
                        maxIntensity = intensity;
                    }
                }

               
            }
            return maxIntensity > 0 ? maxIntensity : 1;
        }
            // fill array of normalized intensities <0;1> for method DrawIntensityMap()
            private void PrecomputeIntensities(float sclX, float sclY)
            {
                if (intensitiesComputed) return;



                int grid = 8;

                int gridX = this.Width / grid;
                int gridY = this.Height / grid;

                precomputedIntensities = new float[gridX, gridY];
                float maxIntensity = GetMaxIntensity(sclX, sclY);

                Parallel.For(0, gridY, y =>
                {
                    for (int x = 0; x < gridX; x++)
                    {
                        // compute intensity for middle of the grid
                        float worldX = (x * grid + grid / 2 - this.Width / 2) / sclX;
                        float worldY = -(y * grid + grid / 2 - this.Height / 2) / sclY;
                        Vector2D worldPoint = new Vector2D(worldX, worldY);

                        Vector2D field = CalcElectricFieldAt(worldPoint);
                        float intensity = field.Magnitude();

                        // normalization
                        precomputedIntensities[x, y] = (float)Math.Log(1 + intensity) / (float)Math.Log(1 + maxIntensity);
                    }
                });

                //for (int y = 0; y < this.Height / grid; y++)
                //{
                //    for (int x = 0; x < this.Width / grid; x++)
                //    {
                //        // compute intensity for middle of the grid
                //        float worldX = (x * grid + grid / 2 - this.Width / 2) / sclX;
                //        float worldY = -(y * grid + grid / 2 - this.Height / 2) / sclY;
                //        PointF worldPoint = new PointF(worldX, worldY);

                //        Vector2D field = CalcElectricFieldAt(worldPoint);
                //        float intensity = field.Magnitude();

                //        // Normalizace intenzity
                //        precomputedIntensities[x, y] = (float)Math.Log(1 + intensity) / (float)Math.Log(1 + maxIntensity);
                //    }
                //}

                intensitiesComputed = true;
            }

        // sets values for second probe
        public void PlaceSecondProbe(Vector2D position)
        {
            secondProbePosition = position;
            secondProbeRadius = position.Magnitude();
            secondProbeStartAngle = (float)Math.Atan2(position.Y, position.X); 
            secondProbeAngle = secondProbeStartAngle;
            elFieldChartValues.Clear();
            secondProbeStartTime = DateTime.Now;
            Invalidate();
        }

        // convert screen coordinates to world c.
        public Vector2D ScreenToWorld(Vector2D screenPosition, float sclX, float sclY)
        {
            float worldX = (screenPosition.X - this.Width / 2) / sclX;
            float worldY = -(screenPosition.Y - this.Height / 2) / sclY; 
            return new Vector2D(worldX, worldY);
        }

        // convert world coordinates to screen c.
        public Vector2D WorldToScreen(Vector2D worldPosition, float sclX, float sclY)
        {
            float screenX = worldPosition.X * sclX + this.Width / 2;
            float screenY = this.Height / 2 - worldPosition.Y * sclY; 
            return new Vector2D(screenX, screenY);
        }

        // allow recalculation of var. IntensitiesComputed
        public void InvalidateIntensityMap()
        {
            this.intensitiesComputed = false;
            this.intensityBitmap = null;  
            
        }


        /// <summary>
        /// Fires the event indicating that the panel has been resized. Inheriting controls should use this in favor of actually listening to the event, but should still call <span class="keyword">base.onResize</span> to ensure that the event is fired for external listeners.
        /// </summary>
        /// <param name="eventargs">An <see cref="T:System.EventArgs">EventArgs</see> that contains the event data.</param>
        protected override void OnResize(EventArgs eventargs)
        {
            this.intensitiesComputed = false;
            this.intensityBitmap = null;
            
            this.Invalidate();  //ensure repaint

            base.OnResize(eventargs);
        }
    }
}
