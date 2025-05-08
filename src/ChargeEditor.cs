using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPG_SP_2024
{
    public partial class ChargeEditor : Form
    {
        private TextBox positionX;
        private TextBox positionY;
        private TextBox magnitude;

        public ChargeEditor(Charge? charge = null)
        {
            InitializeComponent();

            positionX = new TextBox { PlaceholderText = "(-2, 2)" };
            positionY = new TextBox { PlaceholderText = "(-2, 2)" };
            magnitude = new TextBox { PlaceholderText = "(-4, 4)" };

            var layout = new FlowLayoutPanel { Dock = DockStyle.Fill };
            layout.Controls.Add(new Label { Text = "Position X:" });
            layout.Controls.Add(positionX);
            layout.Controls.Add(new Label { Text = "Position Y:" });
            layout.Controls.Add(positionY);
            layout.Controls.Add(new Label { Text = "Magnitude:" });
            layout.Controls.Add(magnitude);

            var okButton = new Button { Text = "OK" };
            okButton.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            layout.Controls.Add(okButton);
            this.Controls.Add(layout);

            if (charge != null)
            {
                positionX.Text = charge.position.X.ToString();
                positionY.Text = charge.position.Y.ToString();
                magnitude.Text = charge.magnitute.ToString();
            }
        }

        public Charge GetCharge()
        {
            var x = float.Parse(positionX.Text);
            var y = float.Parse(positionY.Text);
            var magnitudeV = float.Parse(magnitude.Text);

            return new Charge(new Vector2D(x, y), magnitudeV);
        }
    }
}


