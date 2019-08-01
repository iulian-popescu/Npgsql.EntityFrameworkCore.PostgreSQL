using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    public class NpgsqlAnnotationCodeGenerator : AnnotationCodeGenerator
    {
        [CanBeNull] readonly Version _postgresVersion;

        public NpgsqlAnnotationCodeGenerator(
            [NotNull] AnnotationCodeGeneratorDependencies dependencies,
            [NotNull] IDbContextOptions options)
            : base(dependencies)
            => _postgresVersion = options.Extensions.OfType<NpgsqlOptionsExtension>().FirstOrDefault()?.PostgresVersion;

        public override bool IsHandledByConvention(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == RelationalAnnotationNames.DefaultSchema
                && string.Equals("public", (string)annotation.Value))
            {
                return true;
            }

            return false;
        }

        public override bool IsHandledByConvention(IIndex index, IAnnotation annotation)
        {
            Check.NotNull(index, nameof(index));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.IndexMethod
                && string.Equals("btree", (string)annotation.Value))
            {
                return true;
            }

            return false;
        }

        public override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            // The default by-convention value generation strategy is serial in pre-10 PostgreSQL,
            // and IdentityByDefault otherwise.
            if (annotation.Name == NpgsqlAnnotationNames.ValueGenerationStrategy)
            {
                Debug.Assert(property.ValueGenerated == ValueGenerated.OnAdd);
                var strategy = (NpgsqlValueGenerationStrategy)annotation.Value;

                return _postgresVersion.AtLeast(10, 0)
                    ? strategy == NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                    : strategy == NpgsqlValueGenerationStrategy.SerialColumn;
            }

            return false;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IModel model, IAnnotation annotation)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.PostgresExtensionPrefix, StringComparison.Ordinal))
            {
                var extension = new PostgresExtension(model, annotation.Name);

                return new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresExtension),
                    extension.Name);
            }

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.EnumPrefix, StringComparison.Ordinal))
            {
                var enumTypeDef = new PostgresEnum(model, annotation.Name);

                return enumTypeDef.Schema == "public"
                    ? new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum),
                        enumTypeDef.Name, enumTypeDef.Labels)
                    : new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresEnum),
                        enumTypeDef.Schema, enumTypeDef.Name, enumTypeDef.Labels);
            }

            if (annotation.Name.StartsWith(NpgsqlAnnotationNames.RangePrefix, StringComparison.Ordinal))
            {
                var rangeTypeDef = new PostgresRange(model, annotation.Name);

                if (rangeTypeDef.CanonicalFunction == null &&
                    rangeTypeDef.SubtypeOpClass == null &&
                    rangeTypeDef.Collation == null &&
                    rangeTypeDef.SubtypeDiff == null)
                {
                    return new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresRange),
                        rangeTypeDef.Schema == "public" ? null : rangeTypeDef.Schema,
                        rangeTypeDef.Name,
                        rangeTypeDef.Subtype);
                }

                return new MethodCallCodeFragment(nameof(NpgsqlModelBuilderExtensions.HasPostgresRange),
                    rangeTypeDef.Schema == "public" ? null : rangeTypeDef.Schema,
                    rangeTypeDef.Name,
                    rangeTypeDef.Subtype,
                    rangeTypeDef.CanonicalFunction,
                    rangeTypeDef.SubtypeOpClass,
                    rangeTypeDef.Collation,
                    rangeTypeDef.SubtypeDiff);
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(annotation, nameof(annotation));

            if (annotation.Name == NpgsqlAnnotationNames.Comment)
                return new MethodCallCodeFragment(nameof(NpgsqlEntityTypeBuilderExtensions.ForNpgsqlHasComment), annotation.Value);

            if (annotation.Name == NpgsqlAnnotationNames.UnloggedTable)
                return new MethodCallCodeFragment(nameof(NpgsqlEntityTypeBuilderExtensions.IsUnlogged), annotation.Value);

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IProperty property, IAnnotation annotation)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(annotation, nameof(annotation));

            switch (annotation.Name)
            {
            case NpgsqlAnnotationNames.ValueGenerationStrategy:
                switch ((NpgsqlValueGenerationStrategy)annotation.Value)
                {
                case NpgsqlValueGenerationStrategy.SerialColumn:
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseSerialColumn));
                case NpgsqlValueGenerationStrategy.IdentityAlwaysColumn:
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseIdentityAlwaysColumn));
                case NpgsqlValueGenerationStrategy.IdentityByDefaultColumn:
                    return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn));
                case NpgsqlValueGenerationStrategy.SequenceHiLo:
                    throw new Exception($"Unexpected {NpgsqlValueGenerationStrategy.SequenceHiLo} value generation strategy when scaffolding");
                default:
                    throw new ArgumentOutOfRangeException();
                }

            case NpgsqlAnnotationNames.Comment:
                return new MethodCallCodeFragment(nameof(NpgsqlPropertyBuilderExtensions.ForNpgsqlHasComment), annotation.Value);
            }

            return null;
        }

        public override MethodCallCodeFragment GenerateFluentApi(IIndex index, IAnnotation annotation)
        {
            if (annotation.Name == NpgsqlAnnotationNames.IndexMethod)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasMethod), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexOperators)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasOperators), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexCollation)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasCollation), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexSortOrder)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasSortOrder), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexNullSortOrder)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.HasNullSortOrder), annotation.Value);
            if (annotation.Name == NpgsqlAnnotationNames.IndexInclude)
                return new MethodCallCodeFragment(nameof(NpgsqlIndexBuilderExtensions.IncludeProperties), annotation.Value);

            return null;
        }
    }
}
