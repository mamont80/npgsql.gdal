using System.Drawing;
using OSGeo.OGR;

namespace Npgsql.Gdal.Tests
{
    [TestClass]
    public class NpgsqlTest
    {
        private string ConnectionString = "Server=localhost;User id=postgres;password=123;Database=LayersDB;timeout=300;";
        [TestMethod]
        public async Task TestGdal()
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);

            dataSourceBuilder.UseGdal();

            var dataSource = dataSourceBuilder.Build();
            using (var conn = await dataSource.OpenConnectionAsync())
            {
                var point = Geometry.CreateFromWkt("POINT (30 10)");
                var cm = conn.CreateCommand();
                cm.CommandText = "CREATE TEMP TABLE data (geom GEOMETRY)";
                await cm.ExecuteNonQueryAsync();
                using (var cmd = new NpgsqlCommand("INSERT INTO data (geom) VALUES (@p)", conn))
                {
                    cmd.Parameters.AddWithValue("@p", point);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand("SELECT geom FROM data", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    var g = (reader[0] as Geometry)!;
                    Assert.IsTrue(point.Equal(g));
                }
            }
        }
        [TestMethod]
        public async Task TestGdalExt()
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(ConnectionString);

            dataSourceBuilder.UseGdalExt();

            var dataSource = dataSourceBuilder.Build();
            using (var conn = await dataSource.OpenConnectionAsync())
            {

                Geometry point = Geometry.CreateFromWkt("POINT (30 10)");

                var cm = conn.CreateCommand();
                cm.CommandText = "CREATE TEMP TABLE data (geom GEOMETRY)";
                await cm.ExecuteNonQueryAsync();
                var pointExt = ExtGeometry.Create(point);
                using (var cmd = new NpgsqlCommand("INSERT INTO data (geom) VALUES (@p)", conn))
                {
                    cmd.Parameters.AddWithValue("@p", pointExt);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand("SELECT geom FROM data", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    reader.Read();
                    var eg = (reader[0] as ExtGeometry)!;
                    Assert.IsTrue(point.Equal(eg.Geometry));
                }
            }
        }

    }
}
