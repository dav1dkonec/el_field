using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Reflection;
using System;
using System.Windows.Forms.DataVisualization.Charting;

namespace UPG_SP_2024
{
    public partial class MainForm : Form
    {

        private ProbeChart? chartForm;

        private System.Windows.Forms.Timer? chartTimer;

        private FlowLayoutPanel chargeListBox;
        private Button addButton;
        private Button removeButton;

        private int? selectedChargeIndex = null;
        private Label? selectedLabel = null;
        public static int GridSpacingX { get; set; } = 50;
        public static int GridSpacingY { get; set; } = 50;


        public MainForm()
        {
            InitializeComponent();
            //to prevent flickering of the window app
            typeof(Panel).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, drawingPanel, new object[] { true });
            

            this.drawingPanel.MouseDown += (o, e) =>
            {
                this.OnMouseDown(e);
            };

            this.drawingPanel.MouseMove += (o, e) =>
            {
                this.OnMouseMove(e);
            };

            this.drawingPanel.MouseUp += (o, e) =>
            {
                this.OnMouseUp(e);
            };

            this.drawingPanel.MouseWheel += (o, e) =>
            {
                this.OnMouseWheel(e);

            };

            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 300
            };

            splitContainer.Panel1.Controls.Add(drawingPanel);
            splitContainer.Panel2.Controls.Add(chargeListBox);

            Controls.Add(splitContainer);
            ShowChargeEditor();
        }

        public void RetrieveScenario(int scenario)
        {
            drawingPanel.InitializeScenario(scenario);
        }

        public void RetrieveFileScenario(string file)
        {
            drawingPanel.LoadScenarioFromFile(file);
        }

        // checks what charge it should work with for dragging, if not selected -> function for placing the second probe and initializing chart
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            foreach (var charge in drawingPanel.charges)
            {
                if (IsOnCharge(charge, e))
                {
                    drawingPanel.selectedCharge = charge;
                    break;
                }
            }

            if (drawingPanel.selectedCharge == null)

            {
                if (e.Button == MouseButtons.Left)
                {
                    chartForm = null;

                    float scaleX = drawingPanel.Width / 4.0f;
                    float scaleY = drawingPanel.Height / 4.0f;

                    Vector2D worldP = drawingPanel.ScreenToWorld(new Vector2D(e.X, e.Y), scaleX, scaleY);

                    drawingPanel.PlaceSecondProbe(worldP);

                    chartForm = new ProbeChart();
                    chartForm.Show();

                    chartTimer = new System.Windows.Forms.Timer();
                    chartTimer.Interval = 50;
                    chartTimer.Tick += (sender, e) =>
                    {
                        chartForm.UpdateChart(drawingPanel.elFieldChartValues);
                    };

                    chartForm.chartClosed += () =>
                    {
                        chartTimer.Stop();
                        chartTimer.Dispose();
                        chartForm = null;
                        chartTimer = null;
                    };
                    chartTimer.Start();
                }
            }
        }

        // chcecks if cursor is on charge
        private bool IsOnCharge(Charge c, MouseEventArgs e)
        {
            float scaleX = this.Width / 4;
            float scaleY = this.Height / 4;
            float scale = Math.Min(scaleX, scaleY);

            float radius = Math.Abs(c.magnitute) * scale * 0.13f;

            Vector2D cScreenPosition = drawingPanel.WorldToScreen(c.position, scaleX, scaleY);


            return Math.Sqrt(Math.Pow(cScreenPosition.X - e.X, 2) + Math.Pow(cScreenPosition.Y - e.Y, 2)) < radius;
        }

        // drags selected charge 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (drawingPanel.selectedCharge != null)
            {
                float scaleX = drawingPanel.Width / 4.0f;
                float scaleY = drawingPanel.Height / 4.0f;

                // covert mouse coordinates to world
                drawingPanel.selectedCharge.position = new Vector2D(
                    (e.X - drawingPanel.Width / 2) / scaleX,
                    -(e.Y - drawingPanel.Height / 2) / scaleY
                );

                drawingPanel.InvalidateIntensityMap();
                drawingPanel.Invalidate();
            }

        }

        // when charge released, selected is set null
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            drawingPanel.selectedCharge = null;
        }

        // changes the size of charge with mousewheel
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            foreach (var charge in drawingPanel.charges)
            {
                if (IsOnCharge(charge, e))
                {
                    drawingPanel.selectedCharge = charge;
                }
            }

            float delta = e.Delta > 0 ? 0.05f : -0.05f;

            drawingPanel.selectedCharge.magnitute = Math.Max(-4.0f, Math.Min(4.0f, drawingPanel.selectedCharge.magnitute + delta));

            drawingPanel.InvalidateIntensityMap();
            drawingPanel.Invalidate();
            drawingPanel.selectedCharge = null;
        }

        // removal event
        private void RemoveButton_Click(object sender, EventArgs e)
        {
            // check for charge
            if (selectedChargeIndex.HasValue)
            {
                drawingPanel.charges.RemoveAt(selectedChargeIndex.Value); // remove
                selectedChargeIndex = null; // clear var
                selectedLabel = null;
                RefreshChargeList(); // actualization
                drawingPanel.Invalidate(); 
            }
            else
            {
                MessageBox.Show("No charge selected to remove.");
            }
        }

        // add event
        private void AddButton_Click(object sender, EventArgs e)
        {
            var chargeEditor = new ChargeEditor();
            if (chargeEditor.ShowDialog() == DialogResult.OK)
            {
                var newCharge = chargeEditor.GetCharge();
                drawingPanel.charges.Add(newCharge);
                RefreshChargeList();
            }
        }

        private void ShowChargeEditor()
        {
            // main panel (aligned to right side)
            var editorPanel = new Panel
            {
                Dock = DockStyle.Right,
                Width = 220
            };

            // split into two parts
            var layoutPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2
            };

            // proportion: 95 charges, 5 buttons
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 93));
            layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 7));

            // labels of charges
            chargeListBox = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };

            // buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(3)
            };

            // add charge button
            addButton = new Button
            {
                Text = "Add Charge",
                Width = 100
            };
            addButton.Click += AddButton_Click;

            // remove charge button
            removeButton = new Button
            {
                Text = "Remove Charge",
                Width = 100
            };
            removeButton.Click += RemoveButton_Click;

            // sorting to panels
            buttonPanel.Controls.Add(addButton);
            buttonPanel.Controls.Add(removeButton);

            
            layoutPanel.Controls.Add(chargeListBox, 0, 0);
            layoutPanel.Controls.Add(buttonPanel, 0, 1);

            // final sort into editor panel
            editorPanel.Controls.Add(layoutPanel);

            // adding editor panel to controls
            Controls.Add(editorPanel);
        }



        public void RefreshChargeList()
        {
            
            chargeListBox.Controls.Clear();

            
            for (int i = 0; i < drawingPanel.charges.Count; i++)
            {
                var charge = drawingPanel.charges[i];

                // create label
                var chargeLabel = new Label
                {
                    Text = $"X: {charge.position.X:F2}\nY: {charge.position.Y:F2}\nMagnitute: {charge.magnitute:F2}",
                    AutoSize = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(5),
                    Margin = new Padding(5),
                    Tag = i // save index as tag
                };

                // click event for selection (and removal if wanted)
                chargeLabel.Click += (s, e) =>
                {
                    if (selectedLabel != null)
                    {
                        selectedLabel.BorderStyle = BorderStyle.FixedSingle;
                        selectedLabel.BackColor = default;
                    }

                    selectedLabel = chargeLabel;
                    selectedChargeIndex = (int)chargeLabel.Tag; // load index from tag
                    chargeLabel.BorderStyle = BorderStyle.Fixed3D; // mark selected label
                    chargeLabel.BackColor = Color.LightGreen;
                };

                // add label to chargelistbox items
                chargeListBox.Controls.Add(chargeLabel);
            }

            
        }





        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    AllocConsole();
        //}

        //[DllImport("kernel32.dll", SetLastError = true)]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //static extern bool AllocConsole();
    }
}