using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using micfort.GHL.Math2;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Project2.Particles;

namespace Project2
{
    class MovingPolygon
    {
        private List<HyperPoint<float>> _points;
        private bool _visible;

        public MovingPolygon(List<HyperPoint<float>> points)
        {
            _points = points;
            _visible = true;
        }

        public bool Visible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        //Tests whether point p is in polygon. ref: http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html 
        public bool IsInPolygon(HyperPoint<float> p, Matrix<float> m)
        {
            int nvert = _points.Count;

            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                HyperPoint<float> verti = (HyperPoint<float>)(m * _points[i]);
                HyperPoint<float> vertj = (HyperPoint<float>)(m*_points[j]) ;


                if (((verti.Y > p.Y) != (vertj.Y > p.Y)) &&
                 (p.X < (vertj.X - verti.X) * (p.Y - verti.Y) / (vertj.Y - verti.Y) + verti.X))
                    c = !c;
            }
            return c;
        }

        public void Draw(Matrix<float> m)
        {
            if (_visible)
            {
                if (_points.Count > 0)
                {
                    GL.LineWidth(1.0f);
                    GL.Begin(PrimitiveType.Lines);
                    GL.Color3(1f, 0f, 0f);
                    for (int i = 0; i < _points.Count; i++)
                    {
                        GLMath2.Vertex2((HyperPoint<float>)(m * _points[i]));
                        GLMath2.Vertex2((HyperPoint<float>)(m * _points[(i + 1) % _points.Count]));
                    }
                    GL.End();
                }
            }
        }


    }
}
