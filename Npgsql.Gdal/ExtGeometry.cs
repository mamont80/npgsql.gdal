using OSGeo.OGR;
using System;

namespace Npgsql.Gdal
{
    public class ExtGeometry : IEquatable<ExtGeometry>
    {
        public Geometry Geometry = null;
        public int? SRID = null;
        public static ExtGeometry Create(Geometry geom)
        {
            return Create(geom, 0);
        }
        public static ExtGeometry Create(Geometry geom, int srid)
        {
            var r = new ExtGeometry();
            r.Geometry = geom;
            r.SRID = srid;
            return r;
        }
        public bool Equals(ExtGeometry other)
        {
            if (Geometry != null && other.Geometry != null)
            {
                if (!other.Geometry.Equal(Geometry)) return false;
            }
            else if (Geometry != null && other.Geometry == null || Geometry == null && other.Geometry != null)
            {
                return false;
            }
            return SRID.Equals(other.SRID);
        }
    }
}
