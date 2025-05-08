using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace UPG_SP_2024
{
    // new form window for displaying chart
    public partial class ProbeChart : Form
    {
        private Chart chart;
        public event Action? chartClosed;
        public ProbeChart()
        {
            this.Text = "Field Strength Over Time";
            this.Size = new Size(600, 400);

            chart = new Chart();
            chart.Dock = DockStyle.Fill;

            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Title = "Time (s)";
            chartArea.AxisY.Title = "Field Strength";
            chart.ChartAreas.Add(chartArea);

            Series series = new Series
            {
                Name = "Field Strength",
                ChartType = SeriesChartType.Line
            };
            chart.Series.Add(series);

            this.Controls.Add(chart);
        }

        // adding values for axis x and y 
        public void UpdateChart(List<float> fieldStrength)
        {
            Series series = chart.Series["Field Strength"];
            series.Points.Clear();

            for (int i = 0; i < fieldStrength.Count; i++)
            {
                series.Points.AddXY(i * 0.05, fieldStrength[i]); 
            }
        }

        // handle closing the chart
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            chartClosed?.Invoke();  // invokes event
            chart.Dispose();       // Dispose of chart resources
            base.OnFormClosing(e);
        }
    }
}
