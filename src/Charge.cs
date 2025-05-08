using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UPG_SP_2024
{
    public class Charge
    {
        public Vector2D position { get; set; }
        public float magnitute { get; set; }


        public Charge(Vector2D position, float magnitute)
        {
            this.position = position;
            this.magnitute = magnitute;
        }

    }
}
