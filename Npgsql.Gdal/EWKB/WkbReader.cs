using Npgsql.Gdal;
using OSGeo.OGR;
using System;
using System.IO;

namespace EWKB
{
    public class WkbReader
    {
        public ExtGeometry Read(byte[] data)
        {
            using (Stream stream = new MemoryStream(data))
                return Read(stream);
        }

        /// <summary>
        /// Reads a <see cref="Geometry"/> in binary WKB format from an <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream to read from</param>
        /// <returns>The geometry read</returns>
        /// <exception cref="ParseException"> if the WKB data is ill-formed.</exception>
        public ExtGeometry Read(Stream stream)
        {
            using (var reader = new BiEndianBinaryReader(stream))
                return ReadGeometry(reader);
        }

        /// <summary>
        /// WKB Coordinate Systems
        /// </summary>
        protected enum CoordinateSystem
        {
            /// <summary>
            /// 2D coordinate system
            /// </summary>
            XY = 1,
            /// <summary>
            /// 3D coordinate system
            /// </summary>
            XYZ = 2,
            /// <summary>
            /// 2D coordinate system with additional measure value
            /// </summary>
            XYM = 3,
            /// <summary>
            /// 3D coordinate system with additional measure value
            /// </summary>
            XYZM = 4
        };

        private ExtGeometry ReadGeometry(BiEndianBinaryReader reader)
        {
            ReadByteOrder(reader);
            int? srid = null;
            CoordinateSystem cs;
            var geometryType = ReadGeometryType(reader, out cs, ref srid);

            switch (geometryType)
            {
                //Point
                case WkbGeometryTypes.wkbPoint:
                    return ReadGeometryPoint(reader, srid, cs);
                case WkbGeometryTypes.wkbLineString:
                    return ReadGeometryLineString(reader, srid, cs);
                case WkbGeometryTypes.wkbPolygon:
                    return ReadGeometryPolygon(reader, srid, cs);
                case WkbGeometryTypes.wkbMultiLineString:
                case WkbGeometryTypes.wkbMultiPoint:
                case WkbGeometryTypes.wkbMultiPolygon:
                case WkbGeometryTypes.wkbGeometryCollection:
                    return ReadGeometryMulti(reader, geometryType, srid, cs);
                default:
                    throw new ArgumentException("Geometry type not recognized. GeometryCode: " + geometryType);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        private void ReadByteOrder(BiEndianBinaryReader reader)
        {
            var byteOrder = (ByteOrder)reader.ReadByte();
            if (byteOrder != ByteOrder.BigEndian && byteOrder != ByteOrder.LittleEndian)
                throw new Exception($"Unknown geometry byte order (not LittleEndian or BigEndian): {byteOrder}");

            ((BiEndianBinaryReader)reader).Endianess = byteOrder;
        }

        private WkbGeometryTypes ReadGeometryType(BiEndianBinaryReader reader, out CoordinateSystem coordinateSystem, ref int? srid)
        {
            uint type = reader.ReadUInt32();
            uint subType = type & 0xFFFF;
            //Determine coordinate system
            if ((type & (0x80000000 | 0x40000000)) == (0x80000000 | 0x40000000))
                coordinateSystem = CoordinateSystem.XYZM;
            else if ((type & 0x40000000) == 0x40000000)
                coordinateSystem = CoordinateSystem.XYM;
            else if ((type & 0x80000000) == 0x80000000)
                coordinateSystem = CoordinateSystem.XYZ;
            else
                coordinateSystem = CoordinateSystem.XY;

            //Has SRID
            if ((type & 0x20000000) != 0)
            {
                srid = reader.ReadInt32();
            }

            //Get cs from prefix
            uint ordinate = (type & 0xffff) / 1000;
            switch (ordinate)
            {
                case 1:
                    coordinateSystem = CoordinateSystem.XYZ;
                    break;
                case 2:
                    coordinateSystem = CoordinateSystem.XYM;
                    break;
                case 3:
                    coordinateSystem = CoordinateSystem.XYZM;
                    break;
            }

            return (WkbGeometryTypes)((type & 0xffff) % 1000);
        }
        private ExtGeometry ReadGeometryPoint(BiEndianBinaryReader reader, int? srid, CoordinateSystem cs)
        {
            Geometry g = null;
            switch (cs)
            {
                case CoordinateSystem.XY:
                    g = new Geometry(wkbGeometryType.wkbPoint);
                    break;
                case CoordinateSystem.XYZ:
                    g = new Geometry(wkbGeometryType.wkbPoint25D);
                    break;
                case CoordinateSystem.XYM:
                    g = new Geometry(wkbGeometryType.wkbPointM);
                    break;
                case CoordinateSystem.XYZM:
                    g = new Geometry(wkbGeometryType.wkbPointZM);
                    break;
                default:
                    ErrorCSPoint();
                    break;
            }
            ReadCoordinate(reader, g, cs);
            
            return MakeExtGeometry(g, srid);
        }
        private ExtGeometry ReadGeometryLineString(BiEndianBinaryReader reader, int? srid, CoordinateSystem cs)
        {
            Geometry g = null;
            switch (cs)
            {
                case CoordinateSystem.XY:
                    g = new Geometry(wkbGeometryType.wkbLineString);
                    break;
                case CoordinateSystem.XYZ:
                    g = new Geometry(wkbGeometryType.wkbLineString25D);
                    break;
                case CoordinateSystem.XYM:
                    g = new Geometry(wkbGeometryType.wkbLineStringM);
                    break;
                case CoordinateSystem.XYZM:
                    g = new Geometry(wkbGeometryType.wkbLineStringZM);
                    break;
                default:
                    ErrorCSPoint();
                    break;
            }
            ReadCoordinateLineRing(reader, g, cs);
            
            return MakeExtGeometry(g, srid);
        }
        private ExtGeometry ReadGeometryPolygon(BiEndianBinaryReader reader, int? srid, CoordinateSystem cs)
        {
            Geometry g = null;
            switch (cs)
            {
                case CoordinateSystem.XY:
                    g = new Geometry(wkbGeometryType.wkbPolygon);
                    break;
                case CoordinateSystem.XYZ:
                    g = new Geometry(wkbGeometryType.wkbPolygon25D);
                    break;
                case CoordinateSystem.XYM:
                    g = new Geometry(wkbGeometryType.wkbPolygonM);
                    break;
                case CoordinateSystem.XYZM:
                    g = new Geometry(wkbGeometryType.wkbPolygonZM);
                    break;
                default:
                    ErrorCSPoint();
                    break;
            }
            var numRings = reader.ReadInt32();
            for (int i = 0; i < numRings; i++)
            {
                Geometry ring = new Geometry(wkbGeometryType.wkbLinearRing);
                ReadCoordinateLineRing(reader, ring, cs);
                g.AddGeometry(ring);
            }
            
            return MakeExtGeometry(g, srid);
        }

        private ExtGeometry ReadGeometryMulti(BiEndianBinaryReader reader, WkbGeometryTypes wkbGeomType, int? srid, CoordinateSystem cs)
        {
            Geometry g = null;
            switch (wkbGeomType) 
            {
                case WkbGeometryTypes.wkbMultiPoint:
                    switch (cs) 
                    {
                        case CoordinateSystem.XY:
                            g = new Geometry(wkbGeometryType.wkbMultiPoint);
                            break;
                        case CoordinateSystem.XYZ:
                            g = new Geometry(wkbGeometryType.wkbMultiPoint25D);
                            break;
                        case CoordinateSystem.XYM:
                            g = new Geometry(wkbGeometryType.wkbMultiPointM);
                            break;
                        case CoordinateSystem.XYZM:
                            g = new Geometry(wkbGeometryType.wkbMultiPointZM);
                            break;
                        default:
                            ErrorCSPoint();
                            break;
                    }
                    break;
                case WkbGeometryTypes.wkbMultiLineString:
                    switch (cs)
                    {
                        case CoordinateSystem.XY:
                            g = new Geometry(wkbGeometryType.wkbMultiLineString);
                            break;
                        case CoordinateSystem.XYZ:
                            g = new Geometry(wkbGeometryType.wkbMultiLineString25D);
                            break;
                        case CoordinateSystem.XYM:
                            g = new Geometry(wkbGeometryType.wkbMultiLineStringM);
                            break;
                        case CoordinateSystem.XYZM:
                            g = new Geometry(wkbGeometryType.wkbMultiLineStringZM);
                            break;
                        default:
                            ErrorCSPoint();
                            break;
                    }
                    break;
                case WkbGeometryTypes.wkbMultiPolygon:
                    switch (cs)
                    {
                        case CoordinateSystem.XY:
                            g = new Geometry(wkbGeometryType.wkbMultiPolygon);
                            break;
                        case CoordinateSystem.XYZ:
                            g = new Geometry(wkbGeometryType.wkbMultiPolygon25D);
                            break;
                        case CoordinateSystem.XYM:
                            g = new Geometry(wkbGeometryType.wkbMultiPolygonM);
                            break;
                        case CoordinateSystem.XYZM:
                            g = new Geometry(wkbGeometryType.wkbMultiPolygonZM);
                            break;
                        default:
                            ErrorCSPoint();
                            break;
                    }
                    break;
                case WkbGeometryTypes.wkbGeometryCollection:
                    switch (cs)
                    {
                        case CoordinateSystem.XY:
                            g = new Geometry(wkbGeometryType.wkbGeometryCollection);
                            break;
                        case CoordinateSystem.XYZ:
                            g = new Geometry(wkbGeometryType.wkbGeometryCollection25D);
                            break;
                        case CoordinateSystem.XYM:
                            g = new Geometry(wkbGeometryType.wkbGeometryCollectionM);
                            break;
                        case CoordinateSystem.XYZM:
                            g = new Geometry(wkbGeometryType.wkbGeometryCollectionZM);
                            break;
                        default:
                            ErrorCSPoint();
                            break;
                    }
                    break;
                default:
                    ErrorCSPoint();
                    break;
            }
            var cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                var ge2 = ReadGeometry(reader);
                g.AddGeometry(ge2.Geometry);
            }
            
            return MakeExtGeometry(g, srid);
        }

        private void ReadCoordinateLineRing(BiEndianBinaryReader reader, Geometry g, CoordinateSystem cs)
        {
            var cnt = reader.ReadInt32();
            for (int i = 0; i < cnt; i++)
            {
                ReadCoordinate(reader, g, cs);
            }
        }
        private void ReadCoordinate(BiEndianBinaryReader reader, Geometry g, CoordinateSystem cs)
        {
            double x, y, z, m;
            x = reader.ReadDouble();
            y = reader.ReadDouble();
            switch (cs)
            {
                case CoordinateSystem.XY:
                    g.AddPoint_2D(x, y);
                    break;
                case CoordinateSystem.XYZ:
                    z = reader.ReadDouble();
                    g.AddPoint(x, y, z);
                    break;
                case CoordinateSystem.XYM:
                    m = reader.ReadDouble();
                    g.AddPointM(x, y, m);
                    break;
                case CoordinateSystem.XYZM:
                    z = reader.ReadDouble();
                    m = reader.ReadDouble();
                    g.AddPointZM(x, y, z, m);
                    break;
                default:
                    ErrorCSPoint();
                    break;
            }
        }
        private ExtGeometry MakeExtGeometry(Geometry g, int? srid)
        {
            var r = new ExtGeometry();
            r.Geometry = g;
            r.SRID = srid;
            return r;
        }
        private void ErrorCSPoint()
        {
            throw new Exception("Unknow coordinate system of points");
        }
    }
}
