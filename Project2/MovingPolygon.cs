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
	public class MovingPolygon
    {
        private List<HyperPoint<float>> _points;

        public MovingPolygon(List<HyperPoint<float>> points)
        {
            _points = points;
        }

        public List<HyperPoint<float>> Points
        {
            get { return _points; }
        }


        //Tests whether point p is in polygon. ref: http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html 
        public bool IsInPolygon(HyperPoint<float> p, Matrix<float> m)
        {
			if (_points != null && _points.Count > 0)
			{
				int nvert = _points.Count;

				int i, j;
				bool c = false;
				for (i = 0, j = nvert - 1; i < nvert; j = i++)
				{
					HyperPoint<float> verti = ConvertToPolygonSpace(_points[i], m);
					HyperPoint<float> vertj = ConvertToPolygonSpace(_points[j], m);

					if (((verti.Y > p.Y) != (vertj.Y > p.Y)) &&
					    (p.X < (vertj.X - verti.X)*(p.Y - verti.Y)/(vertj.Y - verti.Y) + verti.X))
						c = !c;
				}
				return c;
			}
			else
			{
				return false;
			}
        }

        public void Draw(Matrix<float> m)
        {
			if (_points != null && _points.Count > 0)
            {
                GL.Begin(PrimitiveType.LineLoop);
                for (int i = 0; i < _points.Count; i++)
                {
	                HyperPoint<float> p = _points[i];
	                p = ConvertToPolygonSpace(p, m);
					GLMath2.Vertex2(p);
                }
                GL.End();
            }
        }

		private HyperPoint<float> ConvertToPolygonSpace(HyperPoint<float> p, Matrix<float> m)
		{
			p = new HyperPoint<float>(p, 1f);
			p = (HyperPoint<float>)(m * p);
			p = p.GetLowerDim(2);
			return p;
		} 

    }
}
