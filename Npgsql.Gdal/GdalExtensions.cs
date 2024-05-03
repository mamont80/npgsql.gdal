using Npgsql.Gdal;
using Npgsql.TypeMapping;

namespace Npgsql
{

    /// <summary>
    /// Extension allowing adding the Gdal plugin to an Npgsql type mapper.
    /// </summary>
    public static class GdalExtensions
    {
        /// <summary>
        /// Sets up GDAL mappings for the PostGIS types.
        /// </summary>
        /// <param name="mapper">The type mapper to set up (global or connection-specific).</param>
        public static INpgsqlTypeMapper UseGdal(
            this INpgsqlTypeMapper mapper)
        {
            mapper.AddTypeInfoResolverFactory(new GdalTypeInfoResolverFactory());
            return mapper;
        }
        public static INpgsqlTypeMapper UseGdalExt(
            this INpgsqlTypeMapper mapper)
        {
            mapper.AddTypeInfoResolverFactory(new GdalExtTypeInfoResolverFactory());
            return mapper;
        }
    }
}