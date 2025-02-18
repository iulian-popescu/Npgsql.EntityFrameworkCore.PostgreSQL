﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
using Npgsql.NameTranslation;
using NpgsqlTypes;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Storage
{
    public class NpgsqlTypeMappingTest
    {
        #region Date/Time

        [Fact]
        public void GenerateSqlLiteral_returns_date_literal()
            => Assert.Equal("DATE '2015-03-12'",
                GetMapping("date").GenerateSqlLiteral(new DateTime(2015, 3, 12)));

        [Fact]
        public void GenerateSqlLiteral_returns_timestamp_literal()
        {
            var mapping = GetMapping("timestamp");
            Assert.Equal("TIMESTAMP '1997-12-17 07:37:16'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Utc)));
            Assert.Equal("TIMESTAMP '1997-12-17 07:37:16'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Local)));
            Assert.Equal("TIMESTAMP '1997-12-17 07:37:16'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Unspecified)));
            Assert.Equal("TIMESTAMP '1997-12-17 07:37:16.345'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, 345)));
            Assert.Equal("TIMESTAMP '9999-12-31 23:59:59.999999'",
                mapping.GenerateSqlLiteral(DateTime.MaxValue));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_timestamptz_datetime_literal()
        {
            var mapping = GetMapping("timestamptz");
            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16 UTC'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Utc)));
            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16 UTC'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Unspecified)));

            var offset = TimeZoneInfo.Local.BaseUtcOffset;
            var offsetStr = (offset < TimeSpan.Zero ? '-' : '+') + offset.ToString(@"hh\:mm");
            Assert.StartsWith($"TIMESTAMPTZ '1997-12-17 07:37:16{offsetStr}",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, DateTimeKind.Local)));

            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16.345678 UTC'",
                mapping.GenerateSqlLiteral(new DateTime(1997, 12, 17, 7, 37, 16, 345, DateTimeKind.Utc).AddTicks(6780)));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_timestamptz_datetimeoffset_literal()
        {
            var mapping = GetMapping("timestamptz");
            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16+02:00'",
                mapping.GenerateSqlLiteral(new DateTimeOffset(1997, 12, 17, 7, 37, 16, TimeSpan.FromHours(2))));
            Assert.Equal("TIMESTAMPTZ '1997-12-17 07:37:16.345+02:00'",
                mapping.GenerateSqlLiteral(new DateTimeOffset(1997, 12, 17, 7, 37, 16, 345, TimeSpan.FromHours(2))));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_time_literal()
        {
            var mapping = GetMapping("time");
            Assert.Equal("TIME '04:05:06.123456'",
                mapping.GenerateSqlLiteral(new TimeSpan(0, 4, 5, 6, 123).Add(TimeSpan.FromTicks(4560))));
            Assert.Equal("TIME '04:05:06.000123'",
                mapping.GenerateSqlLiteral(new TimeSpan(0, 4, 5, 6).Add(TimeSpan.FromTicks(1230))));
            Assert.Equal("TIME '04:05:06.123'", mapping.GenerateSqlLiteral(new TimeSpan(0, 4, 5, 6, 123)));
            Assert.Equal("TIME '04:05:06'", mapping.GenerateSqlLiteral(new TimeSpan(4, 5, 6)));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_timetz_literal()
        {
            var mapping = GetMapping("timetz");
            Assert.Equal("TIMETZ '04:05:06.123456+3'",
                mapping.GenerateSqlLiteral(new DateTimeOffset(2015, 3, 12, 4, 5, 6, 123, TimeSpan.FromHours(3))
                    .AddTicks(4560)));
            Assert.Equal("TIMETZ '04:05:06.789+3'", mapping.GenerateSqlLiteral(new DateTimeOffset(2015, 3, 12, 4, 5, 6, 789, TimeSpan.FromHours(3))));
            Assert.Equal("TIMETZ '04:05:06-3'", mapping.GenerateSqlLiteral(new DateTimeOffset(2015, 3, 12, 4, 5, 6, TimeSpan.FromHours(-3))));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_interval_literal()
        {
            var mapping = GetMapping("interval");
            Assert.Equal("INTERVAL '3 04:05:06.007008'", mapping.GenerateSqlLiteral(new TimeSpan(3, 4, 5, 6, 7)
                .Add(TimeSpan.FromTicks(80))));
            Assert.Equal("INTERVAL '3 04:05:06.007'", mapping.GenerateSqlLiteral(new TimeSpan(3, 4, 5, 6, 7)));
            Assert.Equal("INTERVAL '3 04:05:06'", mapping.GenerateSqlLiteral(new TimeSpan(3, 4, 5, 6)));
            Assert.Equal("INTERVAL '04:05:06'", mapping.GenerateSqlLiteral(new TimeSpan(4, 5, 6)));
            Assert.Equal("INTERVAL '-3 04:05:06.007'", mapping.GenerateSqlLiteral(new TimeSpan(-3, -4, -5, -6, -7)));
        }

        #endregion Date/Time

        #region Networking

        [Fact]
        public void GenerateSqlLiteral_returns_macaddr_literal()
            => Assert.Equal("MACADDR '001122334455'", GetMapping("macaddr").GenerateSqlLiteral(PhysicalAddress.Parse("00-11-22-33-44-55")));

        [Fact]
        public void GenerateCodeLiteral_returns_macaddr_literal()
            => Assert.Equal(@"System.Net.NetworkInformation.PhysicalAddress.Parse(""001122334455"")", CodeLiteral(PhysicalAddress.Parse("00-11-22-33-44-55")));

        [Fact]
        public void GenerateSqlLiteral_returns_macaddr8_literal()
            => Assert.Equal("MACADDR8 '0011223344556677'", GetMapping("macaddr8").GenerateSqlLiteral(PhysicalAddress.Parse("00-11-22-33-44-55-66-77")));

        [Fact]
        public void GenerateCodeLiteral_returns_macaddr8_literal()
            => Assert.Equal(@"System.Net.NetworkInformation.PhysicalAddress.Parse(""0011223344556677"")", CodeLiteral(PhysicalAddress.Parse("00-11-22-33-44-55-66-77")));

        [Fact]
        public void GenerateSqlLiteral_returns_inet_literal()
            => Assert.Equal("INET '192.168.1.1'", GetMapping("inet").GenerateSqlLiteral(IPAddress.Parse("192.168.1.1")));

        [Fact]
        public void GenerateCodeLiteral_returns_inet_literal()
            => Assert.Equal(@"System.Net.IPAddress.Parse(""192.168.1.1"")", CodeLiteral(IPAddress.Parse("192.168.1.1")));

        [Fact]
        public void GenerateSqlLiteral_returns_cidr_literal()
            => Assert.Equal("CIDR '192.168.1.0/24'", GetMapping("cidr").GenerateSqlLiteral((IPAddress.Parse("192.168.1.0"), 24)));

        [Fact]
        public void GenerateCodeLiteral_returns_cidr_literal()
            => Assert.Equal(@"new System.ValueTuple<System.Net.IPAddress, int>(System.Net.IPAddress.Parse(""192.168.1.0""), 24)", CodeLiteral((IPAddress.Parse("192.168.1.0"), 24)));

        #endregion Networking

        #region Geometric

        [Fact]
        public void GenerateSqlLiteral_returns_point_literal()
            => Assert.Equal("POINT '(3.5,4.5)'", GetMapping("point").GenerateSqlLiteral(new NpgsqlPoint(3.5, 4.5)));

        [Fact]
        public void GenerateCodeLiteral_returns_point_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlPoint(3.5, 4.5)", CodeLiteral(new NpgsqlPoint(3.5, 4.5)));

        [Fact]
        public void GenerateSqlLiteral_returns_line_literal()
            => Assert.Equal("LINE '{3.5,4.5,10}'", GetMapping("line").GenerateSqlLiteral(new NpgsqlLine(3.5, 4.5, 10)));

        [Fact]
        public void GenerateCodeLiteral_returns_line_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlLine(3.5, 4.5, 10.0)", CodeLiteral(new NpgsqlLine(3.5, 4.5, 10)));

        [Fact]
        public void GenerateSqlLiteral_returns_lseg_literal()
            => Assert.Equal("LSEG '[(3.5,4.5),(5.5,6.5)]'", GetMapping("lseg").GenerateSqlLiteral(new NpgsqlLSeg(3.5, 4.5, 5.5, 6.5)));

        [Fact]
        public void GenerateCodeLiteral_returns_lseg_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlLSeg(3.5, 4.5, 5.5, 6.5)", CodeLiteral(new NpgsqlLSeg(3.5, 4.5, 5.5, 6.5)));

        [Fact]
        public void GenerateSqlLiteral_returns_box_literal()
            => Assert.Equal("BOX '((2,1),(4,3))'", GetMapping("box").GenerateSqlLiteral(new NpgsqlBox(1, 2, 3, 4)));

        [Fact]
        public void GenerateCodeLiteral_returns_box_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlBox(1.0, 2.0, 3.0, 4.0)", CodeLiteral(new NpgsqlBox(1, 2, 3, 4)));

        [Fact]
        public void GenerateSqlLiteral_returns_path_closed_literal()
            => Assert.Equal("PATH '((1,2),(3,4))'", GetMapping("path").GenerateSqlLiteral(new NpgsqlPath(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            )));

        [Fact]
        public void GenerateCodeLiteral_returns_closed_path_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlPath(new NpgsqlPoint[] { new NpgsqlTypes.NpgsqlPoint(1.0, 2.0), new NpgsqlTypes.NpgsqlPoint(3.0, 4.0) }, false)", CodeLiteral(new NpgsqlPath(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            )));

        [Fact]
        public void GenerateSqlLiteral_returns_path_open_literal()
            => Assert.Equal("PATH '[(1,2),(3,4)]'", GetMapping("path").GenerateSqlLiteral(new NpgsqlPath(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            ) { Open = true }));

        [Fact]
        public void GenerateCodeLiteral_returns_open_path_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlPath(new NpgsqlPoint[] { new NpgsqlTypes.NpgsqlPoint(1.0, 2.0), new NpgsqlTypes.NpgsqlPoint(3.0, 4.0) }, true)", CodeLiteral(new NpgsqlPath(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            ) { Open = true }));

        [Fact]
        public void GenerateSqlLiteral_returns_polygon_literal()
            => Assert.Equal("POLYGON '((1,2),(3,4))'", GetMapping("polygon").GenerateSqlLiteral(new NpgsqlPolygon(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            )));

        [Fact]
        public void GenerateCodeLiteral_returns_polygon_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlPolygon(new NpgsqlPoint[] { new NpgsqlTypes.NpgsqlPoint(1.0, 2.0), new NpgsqlTypes.NpgsqlPoint(3.0, 4.0) })", CodeLiteral(new NpgsqlPolygon(
                new NpgsqlPoint(1, 2),
                new NpgsqlPoint(3, 4)
            )));

        [Fact]
        public void GenerateSqlLiteral_returns_circle_literal()
            => Assert.Equal("CIRCLE '<(3.5,4.5),5.5>'", GetMapping("circle").GenerateSqlLiteral(new NpgsqlCircle(3.5, 4.5, 5.5)));

        [Fact]
        public void GenerateCodeLiteral_returns_circle_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlCircle(3.5, 4.5, 5.5)", CodeLiteral(new NpgsqlCircle(3.5, 4.5, 5.5)));

        #endregion Geometric

        #region Misc

        [Fact]
        public void GenerateSqlLiteral_returns_bool_literal()
            => Assert.Equal("TRUE", GetMapping("bool").GenerateSqlLiteral(true));

        [Fact]
        public void GenerateSqlLiteral_returns_varbit_literal()
            => Assert.Equal("VARBIT B'10'", GetMapping("varbit").GenerateSqlLiteral(new BitArray(new[] { true, false })));

        [Fact]
        public void GenerateCodeLiteral_returns_varbit_literal()
            => Assert.Equal("new System.Collections.BitArray(new bool[] { true, false })", CodeLiteral(new BitArray(new[] { true, false })));

        [Fact]
        public void GenerateSqlLiteral_returns_bit_literal()
            => Assert.Equal("BIT B'10'", GetMapping("bit").GenerateSqlLiteral(new BitArray(new[] { true, false })));

        [Fact]
        public void GenerateCodeLiteral_returns_bit_literal()
            => Assert.Equal("new System.Collections.BitArray(new bool[] { true, false })", CodeLiteral(new BitArray(new[] { true, false })));

        [Fact]
        public void GenerateSqlLiteral_returns_array_literal()
            => Assert.Equal("ARRAY[3,4]::integer[]", GetMapping(typeof(int[])).GenerateSqlLiteral(new[] { 3, 4 }));

        [Fact]
        public void GenerateSqlLiteral_returns_array_empty_literal()
            => Assert.Equal("ARRAY[]::smallint[]", GetMapping(typeof(short[])).GenerateSqlLiteral(new short[0]));

        [Fact]
        public void ValueComparer_int_array()
        {
            // This exercises array's comparer when the element doesn't have a comparer, but it implements
            // IEquatable<T>
            var source = new[] { 2, 3, 4 };

            var comparer = GetMapping(typeof(int[])).Comparer;
            var snapshot = (int[])comparer.Snapshot(source);
            Assert.Equal(source, snapshot);
            Assert.True(comparer.Equals(source, snapshot));
            snapshot[1] = 8;
            Assert.False(comparer.Equals(source, snapshot));
        }

        [Fact]
        public void ValueComparer_hstore_array()
        {
            // This exercises array's comparer when the element has its own non-null comparer
            var source = new[]
            {
                new Dictionary<string, string> { { "k1", "v1" } },
                new Dictionary<string, string> { { "k2", "v2" } },
            };

            var comparer = GetMapping(typeof(Dictionary<string, string>[])).Comparer;
            var snapshot = (Dictionary<string, string>[])comparer.Snapshot(source);
            Assert.Equal(source, snapshot);
            Assert.True(comparer.Equals(source, snapshot));
            snapshot[1]["k2"] = "v8";
            Assert.False(comparer.Equals(source, snapshot));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_bytea_literal()
            => Assert.Equal(@"BYTEA E'\\xDEADBEEF'", GetMapping("bytea").GenerateSqlLiteral(new byte[] { 222, 173, 190, 239 }));

        [Fact]
        public void GenerateSqlLiteral_returns_hstore_literal()
            => Assert.Equal(@"HSTORE '""k1""=>""v1"",""k2""=>""v2""'",
                GetMapping("hstore").GenerateSqlLiteral(new Dictionary<string, string>
                {
                    { "k1", "v1" },
                    { "k2", "v2" }
                }));

        [Fact]
        public void ValueComparer_hstore()
        {
            var source = new Dictionary<string, string>
            {
                { "k1", "v1" },
                { "k2", "v2" }
            };

            var comparer = GetMapping("hstore").Comparer;
            var snapshot = (Dictionary<string, string>)comparer.Snapshot(source);
            Assert.Equal(source, snapshot);
            Assert.True(comparer.Equals(source, snapshot));
            snapshot.Remove("k1");
            Assert.False(comparer.Equals(source, snapshot));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_jsonb_literal()
            => Assert.Equal(@"'{""a"":1}'", GetMapping("jsonb").GenerateSqlLiteral(@"{""a"":1}"));

        [Fact]
        public void GenerateSqlLiteral_returns_json_literal()
            => Assert.Equal(@"'{""a"":1}'", GetMapping("json").GenerateSqlLiteral(@"{""a"":1}"));

        [Fact]
        public void GenerateSqlLiteral_returns_enum_literal()
        {
            var mapping = new NpgsqlEnumTypeMapping(
                "dummy_enum",
                null,
                typeof(DummyEnum),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlSnakeCaseNameTranslator());

            Assert.Equal("'sad'::dummy_enum", mapping.GenerateSqlLiteral(DummyEnum.Sad));
        }

        [Fact]
        public void GenerateSqlLiteral_returns_enum_uppercase_literal()
        {
            var mapping = new NpgsqlEnumTypeMapping(
                "DummyEnum",
                null,
                typeof(DummyEnum),
                new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
                new NpgsqlSnakeCaseNameTranslator());

            Assert.Equal(@"'sad'::""DummyEnum""", mapping.GenerateSqlLiteral(DummyEnum.Sad));
        }

        enum DummyEnum
        {
            // ReSharper disable once UnusedMember.Local
            Happy,
            Sad
        };

        [Fact]
        public void GenerateSqlLiteral_returns_tid_literal()
            => Assert.Equal(@"TID '(0,1)'", GetMapping("tid").GenerateSqlLiteral(new NpgsqlTid(0, 1)));

        #endregion Misc

        #region Ranges

        [Fact]
        public void GenerateSqlLiteral_returns_range_empty_literal()
        {
            var value = NpgsqlRange<int>.Empty;
            var literal = GetMapping("int4range").GenerateSqlLiteral(value);
            Assert.Equal("'empty'::int4range", literal);
        }

        [Fact]
        public void GenerateCodeLiteral_returns_range_empty_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlRange<int>(0, false, 0, false)", CodeLiteral(NpgsqlRange<int>.Empty));

        [Fact]
        public void GenerateSqlLiteral_returns_range_inclusive_literal()
        {
            var value = new NpgsqlRange<int>(4, 7);
            var literal = GetMapping("int4range").GenerateSqlLiteral(value);
            Assert.Equal("'[4,7]'::int4range", literal);
        }

        [Fact]
        public void GenerateCodeLiteral_returns_range_inclusive_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlRange<int>(4, true, 7, true)", CodeLiteral(new NpgsqlRange<int>(4, 7)));

        [Fact]
        public void GenerateSqlLiteral_returns_range_inclusive_exclusive_literal()
        {
            var value = new NpgsqlRange<int>(4, false, 7, true);
            var literal = GetMapping("int4range").GenerateSqlLiteral(value);
            Assert.Equal("'(4,7]'::int4range", literal);
        }

        [Fact]
        public void GenerateCodeLiteral_returns_range_inclusive_exclusive_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlRange<int>(4, false, 7, true)", CodeLiteral(new NpgsqlRange<int>(4, false, 7, true)));

        [Fact]
        public void GenerateSqlLiteral_returns_range_infinite_literal()
        {
            var value = new NpgsqlRange<int>(0, false, true, 7, true, false);
            var literal = GetMapping("int4range").GenerateSqlLiteral(value);
            Assert.Equal("'(,7]'::int4range", literal);
        }

        [Fact]
        public void GenerateCodeLiteral_returns_range_infinite_literal()
            => Assert.Equal("new NpgsqlTypes.NpgsqlRange<int>(0, false, true, 7, true, false)", CodeLiteral(new NpgsqlRange<int>(0, false, true, 7, true, false)));

        #endregion Ranges

        #region Full text search

        [Fact]
        public void GenerateSqlLiteral_returns_tsquery_literal() => Assert.Equal(
            @"TSQUERY '''a'' & ''b'''",
            GetMapping("tsquery").GenerateSqlLiteral(NpgsqlTsQuery.Parse("a & b")));

        [Fact]
        public void GenerateSqlLiteral_returns_tsvector_literal() => Assert.Equal(
            @"TSVECTOR '''a'' ''b'''",
            GetMapping("tsvector").GenerateSqlLiteral(NpgsqlTsVector.Parse("a b")));

        [Fact]
        public void GenerateSqlLiteral_returns_ranking_normalization_literal() => Assert.Equal(
            $"{(int)NpgsqlTsRankingNormalization.DivideByLength}",
            GetMapping(typeof(NpgsqlTsRankingNormalization))
                .GenerateSqlLiteral(NpgsqlTsRankingNormalization.DivideByLength));

        #endregion Full text search

        #region Support

        static readonly NpgsqlTypeMappingSource Mapper = new NpgsqlTypeMappingSource(
            new TypeMappingSourceDependencies(
                new ValueConverterSelector(new ValueConverterSelectorDependencies()),
                Array.Empty<ITypeMappingSourcePlugin>()
            ),
            new RelationalTypeMappingSourceDependencies(Array.Empty<IRelationalTypeMappingSourcePlugin>()),
            new NpgsqlSqlGenerationHelper(new RelationalSqlGenerationHelperDependencies()),
            new NpgsqlOptions()
        );

        static RelationalTypeMapping GetMapping(string storeType) => Mapper.FindMapping(storeType);

        static RelationalTypeMapping GetMapping(Type clrType) => (RelationalTypeMapping)Mapper.FindMapping(clrType);

        static readonly CSharpHelper CsHelper = new CSharpHelper(Mapper);

        static string CodeLiteral(object value) => CsHelper.UnknownLiteral(value);

        #endregion Support
    }
}
