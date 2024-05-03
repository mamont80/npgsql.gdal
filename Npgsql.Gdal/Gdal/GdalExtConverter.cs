using System.Threading;
using System.Threading.Tasks;
using EWKB;
using Npgsql.Internal;

namespace Npgsql.Gdal
{

    sealed class GdalExtConverter : PgStreamingConverter<ExtGeometry>
    {
        //readonly PostGisReader _reader;
        //readonly PostGisWriter _writer;


        public override ExtGeometry Read(PgReader reader)
        {
            WkbReader r = new WkbReader();
            var eg = r.Read(reader.GetStream());
            return eg;
        }

        // PostGisReader/PostGisWriter doesn't support async
        public override ValueTask<ExtGeometry> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        {
            return new ValueTask<ExtGeometry>(Read(reader));
        }

        public override Size GetSize(SizeContext context, ExtGeometry value, ref object writeState)
        {
            WkbWriter w = new WkbWriter(ByteOrder.LittleEndian, false);
            var buf = w.Write(value);
            return buf.Length;
        }

        public override void Write(PgWriter writer, ExtGeometry value)
        {
            WkbWriter w = new WkbWriter(ByteOrder.LittleEndian, false);
            var buf = w.Write(value);
            var str = writer.GetStream(allowMixedIO: true);
            str.Write(buf, 0, buf.Length);
        }

        public override ValueTask WriteAsync(PgWriter writer, ExtGeometry value, CancellationToken cancellationToken = default)
        {
            Write(writer, value);
            return default;
        }
    }
}