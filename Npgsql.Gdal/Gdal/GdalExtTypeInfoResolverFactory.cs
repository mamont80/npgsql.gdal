using Npgsql.Internal.Postgres;
using Npgsql.Internal;
using OSGeo.OGR;
using System;

namespace Npgsql.Gdal
{

    sealed class GdalExtTypeInfoResolverFactory : PgTypeInfoResolverFactory
    {

        public override IPgTypeInfoResolver CreateResolver() => new Resolver();
        public override IPgTypeInfoResolver CreateArrayResolver() => new ArrayResolver();

        class Resolver : IPgTypeInfoResolver
        {
            private TypeInfoMappingCollection _mappings;
            protected TypeInfoMappingCollection Mappings
            {
                get
                {
                    if (_mappings != null) return _mappings;
                    _mappings = AddMappings(new TypeInfoMappingCollection());
                    return _mappings;
                }
            }

            public PgTypeInfo GetTypeInfo(Type type, DataTypeName? dataTypeName, PgSerializerOptions options)
                => Mappings.Find(type, dataTypeName, options);

            static TypeInfoMappingCollection AddMappings(TypeInfoMappingCollection mappings)
            {
                mappings.AddType<ExtGeometry>("geometry",
                    (options, mapping, _) => mapping.CreateInfo(options, new GdalExtConverter()),
                    isDefault: true);
                mappings.AddType<ExtGeometry>("geography",
                    (options, mapping, _) => mapping.CreateInfo(options, new GdalExtConverter()),
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
                mappings.AddArrayType<ExtGeometry>("geometry");
                mappings.AddArrayType<ExtGeometry>("geography");

                return mappings;
            }
        }
    }
}