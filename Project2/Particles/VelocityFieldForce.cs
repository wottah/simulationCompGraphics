using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using micfort.GHL.Math2;

namespace Project2.Particles
{
	class VelocityFieldForce: IForce
	{
		private readonly List<int> _particles;
		private readonly ContinuousField _uField;
		private readonly ContinuousField _vField;
		private readonly ContinuousField _dField;
		private readonly float _positionFactor;
		private readonly HyperPoint<int>[] adjecent = new HyperPoint<int>[4]
			                                              {
				                                              new HyperPoint<int>(-1,0), 
															  new HyperPoint<int>(1,0),
															  new HyperPoint<int>(0,-1),
															  new HyperPoint<int>(0,1),
			                                              }; 

		public VelocityFieldForce(List<int> particles, ContinuousField uField, ContinuousField vField, ContinuousField dField, float positionFactor)
		{
			_particles = particles;
			_uField = uField;
			_vField = vField;
			_dField = dField;
			_positionFactor = positionFactor;
		}

		private List<HyperPoint<int>> FindBounderyCells(Particle p)
		{
			List<HyperPoint<int>> output = new List<HyperPoint<int>>();
			if (p.Polygon.Points.Count > 0)
			{	
				HyperPoint<float> min = new HyperPoint<float>(2), max = new HyperPoint<float>(2);
				HyperPoint<int> imin = new HyperPoint<int>(2), imax = new HyperPoint<int>(2);

				List<HyperPoint<float>> points = p.Polygon.Points;
				Matrix<float> m = p.WorldMatrix;

				min.X = points.Min(point => point.HGMult(m).X);
				max.X = points.Max(point => point.HGMult(m).X);
				min.Y = points.Min(point => point.HGMult(m).Y);
				max.Y = points.Max(point => point.HGMult(m).Y);
				min = min*_positionFactor;
				max = max*_positionFactor;

				imin.X = (int) Math.Floor(min.X);
				imax.X = (int) Math.Ceiling(max.X);
				imin.Y = (int) Math.Floor(min.Y);
				imax.Y = (int) Math.Ceiling(max.Y);

				m = Matrix<float>.Resize(new HyperPoint<float>(_positionFactor, _positionFactor)) * m;

				for (int i = imin.X; i <= imax.X; i++)
				{
					for (int j = imin.Y; j <= imax.Y; j++)
					{
						bool pin = p.Polygon.IsInPolygon(new HyperPoint<float>(i, j), m);
						if(pin)
						{
							bool[] result = new bool[4];
							for (int k = 0; k < adjecent.Length; k++)
							{
								result[k] = p.Polygon.IsInPolygon(new HyperPoint<float>(i, j)+adjecent[k].ConvertTo<float>(), m);
							}

							if(Array.Exists(result, x => x))
							{
								output.Add(new HyperPoint<int>(i, j));
							}
						}
					}
				}
			}
			return output;
		}

		private HyperPoint<float> calculateForces(Particle p, List<HyperPoint<int>> contactPoints)
		{
			HyperPoint<float> linearForce = new HyperPoint<float>(0f, 0f);
			float rotationForce = 0f;

			Matrix<float> m = p.WorldMatrix;
			m = Matrix<float>.Resize(new HyperPoint<float>(_positionFactor, _positionFactor)) * m;

			foreach (HyperPoint<int> contactPoint in contactPoints)
			{
				foreach (HyperPoint<int> adjecentPoint in adjecent)
				{
					if(p.Polygon.IsInPolygon((contactPoint+adjecentPoint).ConvertTo<float>(), m))
					{
						HyperPoint<float> force = new HyperPoint<float>(_uField[contactPoint + adjecentPoint], _vField[contactPoint + adjecentPoint]) * Math.Min(_dField[contactPoint + adjecentPoint], 1.0f);

						linearForce += force;
						rotationForce += HyperPoint<float>.Cross2D(contactPoint.ConvertTo<float>() - p.Position, force);
					}
				}
			}
			return new HyperPoint<float>(linearForce, rotationForce);
		}

		#region Implementation of IForce

		/// <summary>
		/// Calculate the force
		/// </summary>
		public void CalculateForce(List<Particle> particles)
		{
			foreach (int p in _particles)
			{
				List<HyperPoint<int>> contactPoints = FindBounderyCells(particles[p]);
				HyperPoint<float> forces = calculateForces(particles[p], contactPoints);
				particles[p].ForceAccumulator += forces.GetLowerDim(2);
				particles[p].AngularForce += forces[2];
				//HyperPoint<float> position = particles[p].Position*_positionFactor;
				//particles[p].ForceAccumulator += new HyperPoint<float>(_uField[position], _vField[position]) *  Math.Min(_dField[position], 1.0f);
			}
		}

		#endregion
	}
}

