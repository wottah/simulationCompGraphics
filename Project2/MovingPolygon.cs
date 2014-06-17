using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using micfort.GHL.Math2;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Project2
{
    class MovingPolygon : IDrawable
    {
        private List<HyperPoint<float>> points;

        public MovingPolygon(List<HyperPoint<float>> points)
        {
            this.points = points;
        }

        //Tests whether point p is in polygon. ref: http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html 
        public bool IsInPolygon(HyperPoint<float> p)
        {
            int nvert = points.Count;

            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((points[i].Y > p.Y) != (points[j].Y > p.Y)) &&
                 (p.X < (points[j].X - points[i].X) * (p.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
                    c = !c;
            }
            return c;
        }
    }
}
