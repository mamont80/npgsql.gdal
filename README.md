The package is an Npgsql plugin that allows you to interact with spatial data provided by the PostgreSQL PostGIS extension. On the .NET side, the plugin adds support for geometry types from the GDAL(OGR) library, allowing you to read and write them directly to PostgreSQL.

GDAL is connected via the nuget package [MaxRev.Gdal.Core](https://www.nuget.org/packages/MaxRev.Gdal.Core)

To use the NetTopologySuite plugin, add a dependency on this package and create an NpgsqlDataSource.
There are two ways to interact with PostGIS:
1) dataSourceBuilder.UseGdal();
The types Gdal.OGR: Geometry will be used. There will be no projection information. When writing to the database, the value will be srid=0.

```csharp
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
```

2) dataSourceBuilder.UseGdalExt();
You need to use the ExtGeometry class, which contains 2 properties: Geometry and SRID.

```csharp
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
```