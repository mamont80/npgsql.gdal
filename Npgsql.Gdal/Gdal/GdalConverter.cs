using System.Threading;
using System.Threading.Tasks;
using EWKB;
using Npgsql.Internal;
using OSGeo.OGR;

namespace Npgsql.Gdal
{

    sealed class GdalConverter : PgStreamingConverter<Geometry>
    {
        //readonly PostGisReader _reader;
        //readonly PostGisWriter _writer;


        public override Geometry Read(PgReader reader)
        {
            WkbReader r = new WkbReader();
            var eg = r.Read(reader.GetStream());
            return eg.Geometry;
        }

        // PostGisReader/PostGisWriter doesn't support async
        public override ValueTask<Geometry> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        {
            return new ValueTask<Geometry>(Read(reader));
        }

        public override Size GetSize(SizeContext context, Geometry value, ref object writeState)
        {
            WkbWriter w = new WkbWriter(ByteOrder.LittleEndian, false);
            ExtGeometry eg = new ExtGeometry();
            eg.Geometry = value;
            var buf = w.Write(eg);
            return buf.Length;
        }

        public override void Write(PgWriter writer, Geometry value)
        {
            WkbWriter w = new WkbWriter(ByteOrder.LittleEndian, false);
            ExtGeometry eg = new ExtGeometry();
            eg.Geometry = value;
            var buf = w.Write(eg);
            var str = writer.GetStream(allowMixedIO: true);
            str.Write(buf, 0, buf.Length);
        }
        
        public override ValueTask WriteAsync(PgWriter writer, Geometry value, CancellationToken cancellationToken = default)
        {
            Write(writer, value);
            return default;
        }
    }
}