using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using micfort.GHL.Math2;

namespace Project2
{
	static class Extends
	{
		public static HyperPoint<float> HGMult(this HyperPoint<float> p, Matrix<float> m)
		{
			p = new HyperPoint<float>(p, 1f);
			p = (HyperPoint<float>)(m * p);
			p = p.GetLowerDim(2);
			return p;
		} 
	}
}
