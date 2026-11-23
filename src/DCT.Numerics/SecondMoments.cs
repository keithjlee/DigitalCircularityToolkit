using System;

namespace DigitalCircularityToolkit.Numerics
{
    /// <summary>
    /// Accumulates the mass, first and second moments of weighted geometry in one pass (ALIGNMENT_SPEC §2), for the
    /// principal axes of a shape rather than of its sample points:
    /// <list type="bullet">
    /// <item>triangles, weighted by area: ∫ x xᵀ dA = A/12 · (Σ vᵢvᵢᵀ + s sᵀ), s = v₀ + v₁ + v₂; exact for the surface,
    /// so the result does not depend on how finely it is meshed;</item>
    /// <item>segments, weighted by length: ∫ x xᵀ ds = L · ((aaᵀ + bbᵀ)/3 + (abᵀ + baᵀ)/6);</item>
    /// <item>points, weighted by 1 (or a given weight).</item>
    /// </list>
    /// Coordinates are taken relative to the first point added, for numerical stability far from the origin.
    /// Signed weights are allowed (a planar region as a fan of signed triangles).
    /// </summary>
    public sealed class SecondMoments
    {
        private bool _hasOrigin;
        private double _ox, _oy, _oz;
        private double _w, _sx, _sy, _sz, _xx, _xy, _xz, _yy, _yz, _zz;

        /// <summary>Total weight (area, length or count).</summary>
        public double Weight => _w;

        private void Origin(double x, double y, double z)
        {
            if (_hasOrigin) return;
            _ox = x; _oy = y; _oz = z;
            _hasOrigin = true;
        }

        public void AddPoint(double x, double y, double z, double weight = 1)
        {
            Origin(x, y, z);
            x -= _ox; y -= _oy; z -= _oz;
            _w += weight;
            _sx += weight * x; _sy += weight * y; _sz += weight * z;
            _xx += weight * x * x; _xy += weight * x * y; _xz += weight * x * z;
            _yy += weight * y * y; _yz += weight * y * z; _zz += weight * z * z;
        }

        public void AddSegment(double ax, double ay, double az, double bx, double by, double bz)
        {
            Origin(ax, ay, az);
            ax -= _ox; ay -= _oy; az -= _oz;
            bx -= _ox; by -= _oy; bz -= _oz;
            double dx = bx - ax, dy = by - ay, dz = bz - az;
            double l = Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (l == 0) return;
            _w += l;
            _sx += l * (ax + bx) / 2; _sy += l * (ay + by) / 2; _sz += l * (az + bz) / 2;
            double t = l / 3, h = l / 6;
            _xx += t * (ax * ax + bx * bx) + h * 2 * ax * bx;
            _yy += t * (ay * ay + by * by) + h * 2 * ay * by;
            _zz += t * (az * az + bz * bz) + h * 2 * az * bz;
            _xy += t * (ax * ay + bx * by) + h * (ax * by + bx * ay);
            _xz += t * (ax * az + bx * bz) + h * (ax * bz + bx * az);
            _yz += t * (ay * az + by * bz) + h * (ay * bz + by * az);
        }

        /// <summary>A triangle weighted by its area.</summary>
        public void AddTriangle(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz)
        {
            double ux = bx - ax, uy = by - ay, uz = bz - az, vx = cx - ax, vy = cy - ay, vz = cz - az;
            double nx = uy * vz - uz * vy, ny = uz * vx - ux * vz, nz = ux * vy - uy * vx;
            AddTriangle(ax, ay, az, bx, by, bz, cx, cy, cz, 0.5 * Math.Sqrt(nx * nx + ny * ny + nz * nz));
        }

        /// <summary>A triangle with a given (possibly negative) area, e.g. one triangle of a signed fan over a polygon.</summary>
        public void AddTriangle(double ax, double ay, double az, double bx, double by, double bz, double cx, double cy, double cz, double area)
        {
            Origin(ax, ay, az);
            ax -= _ox; ay -= _oy; az -= _oz;
            bx -= _ox; by -= _oy; bz -= _oz;
            cx -= _ox; cy -= _oy; cz -= _oz;
            if (area == 0) return;
            double sx = ax + bx + cx, sy = ay + by + cy, sz = az + bz + cz;
            _w += area;
            _sx += area * sx / 3; _sy += area * sy / 3; _sz += area * sz / 3;
            double k = area / 12;
            _xx += k * (ax * ax + bx * bx + cx * cx + sx * sx);
            _yy += k * (ay * ay + by * by + cy * cy + sy * sy);
            _zz += k * (az * az + bz * bz + cz * cz + sz * sz);
            _xy += k * (ax * ay + bx * by + cx * cy + sx * sy);
            _xz += k * (ax * az + bx * bz + cx * cz + sx * sz);
            _yz += k * (ay * az + by * bz + cy * cz + sy * sz);
        }

        /// <summary>Weighted centroid (absolute coordinates).</summary>
        public (double X, double Y, double Z) Centroid =>
            _w == 0 ? (_ox, _oy, _oz) : (_ox + _sx / _w, _oy + _sy / _w, _oz + _sz / _w);

        /// <summary>Covariance (second moments about the centroid) divided by the weight.</summary>
        public double[,] Covariance()
        {
            if (_w == 0) return new double[3, 3];
            double mx = _sx / _w, my = _sy / _w, mz = _sz / _w;
            double xx = _xx / _w - mx * mx, yy = _yy / _w - my * my, zz = _zz / _w - mz * mz;
            double xy = _xy / _w - mx * my, xz = _xz / _w - mx * mz, yz = _yz / _w - my * mz;
            return new double[,] { { xx, xy, xz }, { xy, yy, yz }, { xz, yz, zz } };
        }

        /// <summary>Principal axes: eigenvalues descending, unit eigenvectors as rows.</summary>
        public void Principal(out double[] values, out double[][] vectors) =>
            SymmetricEigen3.Decompose(Covariance(), out values, out vectors);
    }
}
