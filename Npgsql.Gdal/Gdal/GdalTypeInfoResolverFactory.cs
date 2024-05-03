using Npgsql.Internal.Postgres;
using Npgsql.Internal;
using OSGeo.OGR;
using System;

namespace Npgsql.Gdal {

    sealed class GdalTypeInfoResolverFactory : PgTypeInfoResolverFactory
    {

        public override IPgTypeInfoResolver CreateResolver() => new Resolver();
        public override IPgTypeInfoResolver CreateArrayResolver() => new ArrayResolver();

        class Resolver : IPgTypeInfoResolver
        {
            private TypeInfoMappingCollection _mappings;
            protected TypeInfoMappingCollection Mappings
            {
                get { 
                    if (_mappings != null) return _mappings;
                    _mappings = AddMappings(new TypeInfoMappingCollection());
                    return _mappings;
                }
            }

            public PgTypeInfo GetTypeInfo(Type type, DataTypeName? dataTypeName, PgSerializerOptions options)
                => Mappings.Find(type, dataTypeName, options);

            static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
            {
                mappings.AddType<Geometry>("geometry",
                    (options, mapping, _) => mapping.CreateInfo(options, new GdalConverter()),
                    isDefault: true);
                mappings.AddType<Geometry>("geography",
                    (options, mapping, _) => mapping.CreateInfo(options, new GdalConverter()),
                    isDefault: true);

                return mappings;
            }
        }

        sealed class ArrayResolver : Resolver, IPgTypeInfoResolver
        {
            public new PgTypeInfo GetTypeInfo(Type type, DataTypeName? dataTypeName, PgSerializerOptions options)
                => Mappings.Find(type, dataTypeName, options);

            static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
            {
                mappings.AddArrayType<Geometry>("geometry");
                mappings.AddArrayType<Geometry>("geography");

                return mappings;
            }
        }
    }
}