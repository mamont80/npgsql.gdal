using EWKB;
using MaxRev.Gdal.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql.Gdal;
using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GdalEWKBTestConsole
{
    [TestClass]
    public class EWKBTests
    {

        private string ConnectionString = "Server=localhost;User id=postgres;password=123;Database=LayersDB;timeout=300;";
        [TestMethod]
        public void Test1()
        {
            string s;
            string s2;
            s = "0101000000e538d293adc651c0f3699a2254284540";//ST_AsBinary(ST_MakePoint(-71.1043443253471, 42.3150676015829), 'NDR')
            s2 = "POINT (-71.1043443253471 42.3150676015829)";

            EqualsGeometryToWKT(ReadWKB(HexStringToBytes(s)), s2);
            s = "0000000001c051c6ad93d238e540452854229a69f3";//ST_AsBinary(ST_MakePoint(-71.1043443253471, 42.3150676015829), 'XDR')
            EqualsGeometryToWKT(ReadWKB(HexStringToBytes(s)), s2);
            s = "0020000001000010e6c051c6ad93d238e540452854229a69f3";//ST_AsEWKB(ST_SetSRID(ST_MakePoint(-71.1043443253471, 42.3150676015829),4326), 'XDR')
            EqualsGeometryToWKT(ReadWKB(HexStringToBytes(s)), s2, 4326);
            s = "0101000020e6100000e538d293adc651c0f3699a2254284540";//ST_AsEWKB(ST_SetSRID(ST_MakePoint(-71.1043443253471, 42.3150676015829),4326), 'NDR')
            EqualsGeometryToWKT(ReadWKB(HexStringToBytes(s)), s2, 4326);

            s = "POINT (-71.1043443253471 42.3150676015829)";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "LINESTRING (30 10, 10 30, 40 40)";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "POLYGON ((35 10, 45 45, 15 40, 10 20, 35 10), (20 30, 35 35, 30 20, 20 30))";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "MULTIPOINT (10 40, 40 30, 20 20, 30 10)";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)), ((15 5, 40 10, 10 20, 5 10, 15 5)))";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "GEOMETRYCOLLECTION(POINT(4 6),LINESTRING(4 6,7 10))";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            //s = "POINT EMPTY";
            //EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            //EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "POLYGON EMPTY";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "LINESTRING EMPTY";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "MULTIPOLYGON EMPTY";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            s = "GEOMETRYCOLLECTION EMPTY";
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);


            //s = "POINT ZM (1 1 5 60)";
            //EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            //EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            //s = "POINT M (1 1 80)";
            //EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbNDR)), s);
            //EqualsGeometryToWKT(ReadWKB(GetWkbByWkt(s, wkbByteOrder.wkbXDR)), s);
            TestSqlEmpty("select ST_AsBinary(ST_GeomFromEWKT('POINT EMPTY'))");
            TestSqlEmpty("select ST_AsEWKB(ST_GeomFromEWKT('SRID=4269;POINT EMPTY'))", true);

            TestSqlEmpty("select ST_AsBinary(ST_GeomFromEWKT('POLYGON EMPTY'))");
            TestSqlEmpty("select ST_AsEWKB(ST_GeomFromEWKT('SRID=4269;POLYGON EMPTY'))", true);

            TestSqlEmpty("select ST_AsBinary(ST_GeomFromEWKT('LINESTRING EMPTY'))");
            TestSqlEmpty("select ST_AsEWKB(ST_GeomFromEWKT('SRID=4269;LINESTRING EMPTY'))", true);

            TestSqlEmpty("select ST_AsBinary(ST_GeomFromEWKT('GEOMETRYCOLLECTION EMPTY'))");
            TestSqlEmpty("select ST_AsEWKB(ST_GeomFromEWKT('SRID=4269;GEOMETRYCOLLECTION EMPTY'))", true);

            TestSql("POINT(-71.064544 42.28787)");
            TestSql("POINT(-71.064544 42.28787 1)");
            TestSql("LINESTRING (30 10, 10 30, 40 40)");
            TestSql("POLYGON ((30 10 1, 40 40 2, 20 40 3, 10 20 4, 30 10 1))");
            TestSql("POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))");
            TestSql("POLYGON ((35 10, 45 45, 15 40, 10 20, 35 10), (20 30, 35 35, 30 20, 20 30))");
            TestSql("MULTIPOINT (10 40, 40 30, 20 20, 30 10)");
            TestSql("MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))");
            TestSql("MULTIPOLYGON (((30 20, 45 40, 10 40, 30 20)), ((15 5, 40 10, 10 20, 5 10, 15 5)))");
            TestSql("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
            TestSql("GEOMETRYCOLLECTION(POINT(4 6),LINESTRING(4 6,7 10))");
            TestSql("LINESTRING (30 10 1, 10 30 2, 40 40 3)");
            TestSql("LINESTRING (30 10 1 1, 10 30 2 2, 40 40 3 3)");
        }
        private void TestSql(string wkt)
        {
            var buf = GetGeometryFromSql($"select ST_AsBinary(ST_GeomFromEWKT('{wkt}'))");
            var eg = ReadWKB(buf);
            var buf2 = GeometryToWKB(eg);
            IsEqualsArray(buf, buf2);

            buf = GetGeometryFromSql($"select ST_AsEWKB(ST_GeomFromEWKT('SRID=4269;{wkt}'))");
            eg = ReadWKB(buf);
            buf2 = GeometryToWKB(eg);
            IsEqualsArray(buf, buf2);
            Assert.AreEqual(eg.SRID!.Value, 4269);

            buf = GetGeometryFromSql($"select ST_AsEWKB(ST_GeomFromEWKT('SRID=4269;{wkt}'), 'XDR')");
            eg = ReadWKB(buf);
            buf2 = GeometryToWKB(eg, ByteOrder.BigEndian);
            IsEqualsArray(buf, buf2);
            Assert.AreEqual(eg.SRID!.Value, 4269);

        }
        private void TestSqlEmpty(string sql, bool compareSRID = false)
        {
            var buf = GetGeometryFromSql(sql);
            var eg = ReadWKB(buf);
            var buf2 = GeometryToWKB(eg);
            var eg2 = ReadWKB(buf2);
            Assert.AreEqual(eg.Geometry.IsEmpty(), true);
            Assert.AreEqual(eg2.Geometry.IsEmpty(), true);
            Assert.AreEqual(eg2.SRID, eg.SRID);
            if (compareSRID) Assert.AreEqual(eg2.SRID!.Value, eg.SRID!.Value);
        }
        private byte[] GeometryToWKB(ExtGeometry eg, ByteOrder byteOrder = ByteOrder.LittleEndian)
        {
            WkbWriter writer = new WkbWriter(byteOrder, eg.SRID != null);
            writer.AsISOWkb = (eg.SRID == null);
            var buf = writer.Write(eg);
            return buf;
        }
        private void IsEqualsArray(byte[] buf1, byte[] buf2)
        { 
            if (buf1.Length != buf2.Length) throw new ArgumentException("Arrays is not equals");
            for(int i = 0; i < buf1.Length; i++)
            {
                if (buf1[i] != buf2[i]) throw new ArgumentException("Arrays is not equals");
            }
        }
        
        private byte[] GetGeometryFromSql(string sql)
        {
            Npgsql.NpgsqlConnection conn = new Npgsql.NpgsqlConnection(ConnectionString);
            conn.Open();
            try
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var v = reader.GetValue(0);
                        if (v == null) return new byte[0];
                        return (v as byte[])!;
                    }
                    else return new byte[0];
                }
            }
            finally
            { 
                conn.Close();
            }
        }
        private byte[] GetWkbByWkt(string wkt, wkbByteOrder byteOrder = wkbByteOrder.wkbNDR)
        { 
            var g = Geometry.CreateFromWkt(wkt);
            var size = g.WkbSize();
            byte[] buf = new byte[size];
            g.ExportToWkb(buf, byteOrder);
            return buf;
        }
        private void EqualsGeometryToWKT(ExtGeometry ge, string wkt, int? epsg = null)
        {
            string wkt2;
            ge.Geometry.ExportToWkt(out wkt2);

            Assert.AreEqual(wktStandart(wkt), wktStandart(wkt2));
            Assert.AreEqual(epsg, ge.SRID);
        }
        private string wktStandart(string wkt)
        {
            return wkt.Replace(", ", ",").Replace("GEOMETRYCOLLECTION (", "GEOMETRYCOLLECTION(").Replace("POINT (", "POINT(").Replace("LINESTRING (", "LINESTRING(");
        }
        private ExtGeometry ReadWKB(byte[] buf)
        {
            WkbReader r = new WkbReader();
            var g = r.Read(buf);
            return g;
        }
        public static byte[] HexStringToBytes(string hex)
        {
            byte[] buf = new byte[hex.Length / 2];
            for (int i = 0, i1 = 0; i < hex.Length; i += 2, i1++)
            {
                buf[i1] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return buf;
        }

    }
}
