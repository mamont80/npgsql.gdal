using Npgsql.Gdal;
using OSGeo.OGR;
using System;
using System.IO;

namespace EWKB
{
    public class WkbWriter
    {
        public bool AsISOWkb = false;
        private ByteOrder encodingType;
        private bool emitZ;
        private bool emitM;
        private bool emitSRID;
        private double[] coord = new double[4];

        public WkbWriter():this(ByteOrder.LittleEndian, true)
        {
            
        }

        public WkbWriter(ByteOrder byteOrder = ByteOrder.LittleEndian, bool emitSRID = true)
        {
            encodingType = byteOrder;
            this.emitSRID = emitSRID;
        }

        public byte[] Write(ExtGeometry ge)
        { 
            MemoryStream ms = new MemoryStream();
            try
            {
                Write(ge, ms);
                return ms.ToArray();
            }
            finally
            { 
                ms.Dispose();
            }
        }

        private void Write(ExtGeometry ge, Stream stream)
        {
            BinaryWriter writer = null;
            try
            {
                writer = encodingType == ByteOrder.LittleEndian
                    ? new BinaryWriter(stream)
                    : new BEBinaryWriter(stream);
                WriteGeometry(writer, ge, emitSRID);
            }
            finally
            {
                if (writer != null)
                    ((IDisposable)writer).Dispose();
            }
        }
        private void WriteGeometry(BinaryWriter writer, ExtGeometry ge, bool includeSRID)
        {
            var tp = ge.Geometry.GetGeometryType();
            switch (tp)
            {
                case wkbGeometryType.wkbPoint:
                case wkbGeometryType.wkbPointM:
                case wkbGeometryType.wkbPoint25D:
                case wkbGeometryType.wkbPointZM:
                    WriteGeometryPoint(writer, ge, includeSRID);
                    break;
                case wkbGeometryType.wkbLineString:
                case wkbGeometryType.wkbLineStringM:
                case wkbGeometryType.wkbLineString25D:
                case wkbGeometryType.wkbLineStringZM:
                    WriteGeometryLineString(writer, ge, includeSRID);
                    break;
                case wkbGeometryType.wkbPolygon:
                case wkbGeometryType.wkbPolygonM:
                case wkbGeometryType.wkbPolygon25D:
                case wkbGeometryType.wkbPolygonZM:
                    WriteGeometryPolygon(writer, ge, includeSRID);
                    break;
                case wkbGeometryType.wkbMultiPoint:
                case wkbGeometryType.wkbMultiPointM:
                case wkbGeometryType.wkbMultiPoint25D:
                case wkbGeometryType.wkbMultiPointZM:
                case wkbGeometryType.wkbMultiLineString:
                case wkbGeometryType.wkbMultiLineStringM:
                case wkbGeometryType.wkbMultiLineString25D:
                case wkbGeometryType.wkbMultiLineStringZM:
                case wkbGeometryType.wkbMultiPolygon:
                case wkbGeometryType.wkbMultiPolygonM:
                case wkbGeometryType.wkbMultiPolygon25D:
                case wkbGeometryType.wkbMultiPolygonZM:
                case wkbGeometryType.wkbGeometryCollection:
                case wkbGeometryType.wkbGeometryCollectionM:
                case wkbGeometryType.wkbGeometryCollection25D:
                case wkbGeometryType.wkbGeometryCollectionZM:
                    WriteGeometryMulti(writer, ge, includeSRID);
                    break;
                default:
                    throw new ArgumentException("Unknown geometry type: " + tp);
            }
        }

        private void WriteGeometryPoint(BinaryWriter writer, ExtGeometry ge, bool includeSRID)
        {
            WriteHeader(writer, ge, includeSRID);
            if (ge.Geometry.IsEmpty()) 
                WriteCoordinatesNan(writer); 
            else WriteCoordinates(writer, ge.Geometry, 0);
        }
        private void WriteGeometryLineString(BinaryWriter writer, ExtGeometry ge, bool includeSRID)
        {
            WriteHeader(writer, ge, includeSRID);
            var cnt = ge.Geometry.GetPointCount();
            WriteCount(writer, cnt);
            for (int i = 0; i < cnt; i++)
            { 
                WriteCoordinates(writer, ge.Geometry, i);
            }
        }
        private void WriteGeometryPolygon(BinaryWriter writer, ExtGeometry ge, bool includeSRID)
        {
            WriteHeader(writer, ge, includeSRID);
            var cnt = ge.Geometry.GetGeometryCount();
            WriteCount(writer, cnt);
            for (int i = 0; i < cnt; i++)
            {
                WriteGeometryLinearRing(writer, ge.Geometry.GetGeometryRef(i));
            }
        }
        private void WriteGeometryMulti(BinaryWriter writer, ExtGeometry ge, bool includeSRID)
        {
            WriteHeader(writer, ge, includeSRID);
            var cnt = ge.Geometry.GetGeometryCount();
            WriteCount(writer, cnt);
            for (int i = 0; i < cnt; i++)
            {
                var g2 = ge.Geometry.GetGeometryRef(i);
                WriteGeometry(writer, new ExtGeometry() { Geometry = g2, SRID = ge.SRID }, false);
            }
        }

        private void WriteGeometryLinearRing(BinaryWriter writer, Geometry g)
        {
            var cnt = g.GetPointCount();
            WriteCount(writer, cnt);
            for (int i = 0; i < cnt; i++)
            {
                WriteCoordinates(writer, g, i);
            }
        }
        private void WriteCount(BinaryWriter writer, int count)
        { 
            writer.Write(count);
        }
        private void WriteCoordinatesNan(BinaryWriter writer)
        {
            writer.Write(double.NaN);
            writer.Write(double.NaN);
            if (emitM) writer.Write(double.NaN);
            if (emitZ) writer.Write(double.NaN);
        }
        private void WriteCoordinates(BinaryWriter writer, Geometry g, int index)
        {
            if (emitM || emitZ)
            {
                g.GetPointZM(index, coord);
                writer.Write(coord[0]);
                writer.Write(coord[1]);
                if (emitZ)
                {
                    writer.Write(coord[2]);
                    if (emitM) writer.Write(coord[3]);
                }
                else 
                {
                    writer.Write(coord[2]);
                }
            }
            else {
                g.GetPoint_2D(index, coord);
                writer.Write(coord[0]);
                writer.Write(coord[1]);
            }
        }
        private void WriteHeader(BinaryWriter writer, ExtGeometry ge, bool includeSRID)
        {
            //Byte Order
            writer.Write((byte)encodingType);
            var gt = ge.Geometry.GetGeometryType();
            emitZ = false;
            emitM = false;
            switch (gt)
            {
                case wkbGeometryType.wkbMultiPoint25D:
                case wkbGeometryType.wkbPoint25D:
                case wkbGeometryType.wkbMultiLineString25D:
                case wkbGeometryType.wkbLineString25D:
                case wkbGeometryType.wkbMultiPolygon25D:
                case wkbGeometryType.wkbPolygon25D:
                case wkbGeometryType.wkbGeometryCollection25D:
                    emitZ = true;
                    break;
                case wkbGeometryType.wkbMultiPointM:
                case wkbGeometryType.wkbPointM:
                case wkbGeometryType.wkbMultiLineStringM:
                case wkbGeometryType.wkbLineStringM:
                case wkbGeometryType.wkbMultiPolygonM:
                case wkbGeometryType.wkbPolygonM:
                case wkbGeometryType.wkbGeometryCollectionM:
                    emitM = true; 
                    break;
                case wkbGeometryType.wkbMultiPointZM:
                case wkbGeometryType.wkbPointZM:
                case wkbGeometryType.wkbMultiLineStringZM:
                case wkbGeometryType.wkbLineStringZM:
                case wkbGeometryType.wkbMultiPolygonZM:
                case wkbGeometryType.wkbPolygonZM:
                case wkbGeometryType.wkbGeometryCollectionZM:
                    emitM = true;
                    emitZ = true;
                    break;
            }



            //TODO: use "is" check, like in "WKTWriter.AppendGeometryTaggedText"?
            WkbGeometryTypes geometryType;
            var nativeType = ge.Geometry.GetGeometryType();
            switch (nativeType)
            {
                case wkbGeometryType.wkbPoint:
                case wkbGeometryType.wkbPointM:
                case wkbGeometryType.wkbPoint25D:
                case wkbGeometryType.wkbPointZM:
                    geometryType = WkbGeometryTypes.wkbPoint;
                    break;
                case wkbGeometryType.wkbLineString:
                case wkbGeometryType.wkbLineStringM:
                case wkbGeometryType.wkbLineString25D:
                case wkbGeometryType.wkbLineStringZM:
                    geometryType = WkbGeometryTypes.wkbLineString;
                    break;
                case wkbGeometryType.wkbPolygon:
                case wkbGeometryType.wkbPolygonM:
                case wkbGeometryType.wkbPolygon25D:
                case wkbGeometryType.wkbPolygonZM:
                    geometryType = WkbGeometryTypes.wkbPolygon;
                    break;
                case wkbGeometryType.wkbMultiPoint:
                case wkbGeometryType.wkbMultiPointM:
                case wkbGeometryType.wkbMultiPoint25D:
                case wkbGeometryType.wkbMultiPointZM:
                    geometryType = WkbGeometryTypes.wkbMultiPoint;
                    break;
                case wkbGeometryType.wkbMultiLineString:
                case wkbGeometryType.wkbMultiLineStringM:
                case wkbGeometryType.wkbMultiLineString25D:
                case wkbGeometryType.wkbMultiLineStringZM:
                    geometryType = WkbGeometryTypes.wkbMultiLineString;
                    break;
                case wkbGeometryType.wkbMultiPolygon:
                case wkbGeometryType.wkbMultiPolygonM:
                case wkbGeometryType.wkbMultiPolygon25D:
                case wkbGeometryType.wkbMultiPolygonZM:
                    geometryType = WkbGeometryTypes.wkbMultiPolygon;
                    break;
                case wkbGeometryType.wkbGeometryCollection:
                case wkbGeometryType.wkbGeometryCollectionM:
                case wkbGeometryType.wkbGeometryCollection25D:
                case wkbGeometryType.wkbGeometryCollectionZM:
                    geometryType = WkbGeometryTypes.wkbGeometryCollection;
                    break;
                default:
                    throw new ArgumentException("Unknown geometry type: "+ nativeType);
            }

            //Modify WKB Geometry type
            uint intGeometryType = (uint)geometryType & 0xff;
            if (emitZ)
            {
                if (AsISOWkb) intGeometryType += 1000;
                else intGeometryType |= 0x80000000;
            }

            if (emitM)
            {
                if (AsISOWkb) intGeometryType += 2000;
                else intGeometryType |= 0x40000000;
            }

            // Check if includeSRID is valid
            var writeSRID = includeSRID && emitSRID && ge.SRID.HasValue && !AsISOWkb;

            // Flag for SRID if needed
            if (writeSRID)
                intGeometryType |= 0x20000000;

            //
            writer.Write(intGeometryType);

            //Write SRID if needed
            if (writeSRID)
                writer.Write(ge.SRID.Value);
        }

    }
}
