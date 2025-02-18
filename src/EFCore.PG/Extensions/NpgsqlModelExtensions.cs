using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class NpgsqlModelExtensions
    {
        public const string DefaultHiLoSequenceName = "EntityFrameworkHiLoSequence";

        #region HiLo

        /// <summary>
        ///     Returns the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The name to use for the default hi-lo sequence. </returns>
        public static string GetNpgsqlHiLoSequenceName([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.HiLoSequenceName]
               ?? DefaultHiLoSequenceName;

        /// <summary>
        ///     Sets the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The value to set. </param>
        public static void SetNpgsqlHiLoSequenceName([NotNull] this IMutableModel model, [CanBeNull] string name)
        {
            Check.NullButNotEmpty(name, nameof(name));

            model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.HiLoSequenceName, name);
        }

        /// <summary>
        ///     Sets the name to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="name"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetNpgsqlHiLoSequenceName(
            [NotNull] this IConventionModel model, [CanBeNull] string name, bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(name, nameof(name));

            model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.HiLoSequenceName, name, fromDataAnnotation);
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the default hi-lo sequence name.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default hi-lo sequence name. </returns>
        public static ConfigurationSource? GetNpgsqlHiLoSequenceNameConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceName)?.GetConfigurationSource();

        /// <summary>
        ///     Returns the schema to use for the default hi-lo sequence.
        ///     <see cref="NpgsqlPropertyBuilderExtensions.UseHiLo" />
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The schema to use for the default hi-lo sequence. </returns>
        public static string GetNpgsqlHiLoSequenceSchema([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.HiLoSequenceSchema];

        /// <summary>
        ///     Sets the schema to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetNpgsqlHiLoSequenceSchema([NotNull] this IMutableModel model, [CanBeNull] string value)
        {
            Check.NullButNotEmpty(value, nameof(value));

            model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema, value);
        }

        /// <summary>
        ///     Sets the schema to use for the default hi-lo sequence.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetNpgsqlHiLoSequenceSchema(
            [NotNull] this IConventionModel model, [CanBeNull] string value, bool fromDataAnnotation = false)
        {
            Check.NullButNotEmpty(value, nameof(value));

            model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema, value, fromDataAnnotation);
        }

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the default hi-lo sequence schema.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default hi-lo sequence schema. </returns>
        public static ConfigurationSource? GetNpgsqlHiLoSequenceSchemaConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema)?.GetConfigurationSource();

        #endregion

        #region Value Generation Strategy

        /// <summary>
        ///     Returns the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
        ///     of keys in the model, unless the property has a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The default <see cref="NpgsqlValueGenerationStrategy" />. </returns>
        public static NpgsqlValueGenerationStrategy? GetNpgsqlValueGenerationStrategy([NotNull] this IModel model)
            => (NpgsqlValueGenerationStrategy?)model[NpgsqlAnnotationNames.ValueGenerationStrategy];

        /// <summary>
        ///     Attempts to set the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
        ///     of keys in the model that don't have a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        public static void SetNpgsqlValueGenerationStrategy([NotNull] this IMutableModel model, NpgsqlValueGenerationStrategy? value)
            => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value);

        /// <summary>
        ///     Attempts to set the <see cref="NpgsqlValueGenerationStrategy" /> to use for properties
        ///     of keys in the model that don't have a strategy explicitly set.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <param name="value"> The value to set. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        public static void SetNpgsqlValueGenerationStrategy(
            [NotNull] this IConventionModel model, NpgsqlValueGenerationStrategy? value, bool fromDataAnnotation = false)
            => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy, value, fromDataAnnotation);

        /// <summary>
        ///     Returns the <see cref="ConfigurationSource" /> for the default <see cref="NpgsqlValueGenerationStrategy" />.
        /// </summary>
        /// <param name="model"> The model. </param>
        /// <returns> The <see cref="ConfigurationSource" /> for the default <see cref="NpgsqlValueGenerationStrategy" />. </returns>
        public static ConfigurationSource? GetNpgsqlValueGenerationStrategyConfigurationSource([NotNull] this IConventionModel model)
            => model.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy)?.GetConfigurationSource();

        #endregion

        #region PostgreSQL Extensions

        [NotNull]
        public static PostgresExtension GetOrAddPostgresExtension(
            [NotNull] this IMutableModel model,
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
            => PostgresExtension.GetOrAddPostgresExtension(model, schema, name, version);

        public static IReadOnlyList<PostgresExtension> GetPostgresExtensions([NotNull] this IModel model)
            => PostgresExtension.GetPostgresExtensions(model).ToArray();

        #endregion

        #region Enum types

        public static PostgresEnum GetOrAddPostgresEnum(
            [NotNull] this IMutableModel model,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
            => PostgresEnum.GetOrAddPostgresEnum(model, schema, name, labels);

        public static IReadOnlyList<PostgresEnum> GetPostgresEnums([NotNull] this IModel model)
            => PostgresEnum.GetPostgresEnums(model).ToArray();

        #endregion Enum types

        #region Range types

        public static PostgresRange GetOrAddPostgresRange(
            [NotNull] this IMutableModel model,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            string canonicalFunction = null,
            string subtypeOpClass = null,
            string collation = null,
            string subtypeDiff = null)
            => PostgresRange.GetOrAddPostgresRange(
                model,
                schema,
                name,
                subtype,
                canonicalFunction,
                subtypeOpClass,
                collation,
                subtypeDiff);

        public static IReadOnlyList<PostgresRange> PostgresRanges([NotNull] this IModel model)
            => PostgresRange.GetPostgresRanges(model).ToArray();

        #endregion Range types

        #region Database Template

        public static string GetNpgsqlDatabaseTemplate([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.DatabaseTemplate];

        public static void SetNpgsqlDatabaseTemplate([NotNull] this IMutableModel model, [CanBeNull] string template)
            => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.DatabaseTemplate, template);
        #endregion

        #region Tablespace

        public static string GetNpgsqlTablespace([NotNull] this IModel model)
            => (string)model[NpgsqlAnnotationNames.Tablespace];

        public static void SetNpgsqlTablespace([NotNull] this IMutableModel model, [CanBeNull] string tablespace)
            => model.SetOrRemoveAnnotation(NpgsqlAnnotationNames.Tablespace, tablespace);

        #endregion

    }
}
