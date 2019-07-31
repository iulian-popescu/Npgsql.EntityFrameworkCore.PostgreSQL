﻿using System;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;
using Npgsql.NameTranslation;
using NpgsqlTypes;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// Npgsql specific extension methods for <see cref="ModelBuilder"/>.
    /// </summary>
    [PublicAPI]
    public static class NpgsqlModelBuilderExtensions
    {
        #region HiLo

        /// <summary>
        /// Configures the model to use a sequence-based hi-lo pattern to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="name">The name of the sequence.</param>
        /// <param name="schema">The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static ModelBuilder UseHiLo(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name = null,
            [CanBeNull] string schema = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            var model = modelBuilder.Model;

            name ??= NpgsqlModelExtensions.DefaultHiLoSequenceName;

            if (model.FindSequence(name, schema) == null)
            {
                modelBuilder.HasSequence(name, schema).IncrementsBy(10);
            }

            model.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SequenceHiLo);
            model.SetNpgsqlHiLoSequenceName(name);
            model.SetNpgsqlHiLoSequenceSchema(schema);

            return modelBuilder;
        }

        /// <summary>
        ///     Configures the database sequence used for the hi-lo pattern to generate values for key properties
        ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> A builder to further configure the sequence. </returns>
        public static IConventionSequenceBuilder HasHiLoSequence(
            [NotNull] this IConventionModelBuilder modelBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            if (!modelBuilder.CanSetHiLoSequence(name, schema))
            {
                return null;
            }

            modelBuilder.Metadata.SetNpgsqlHiLoSequenceName(name, fromDataAnnotation);
            modelBuilder.Metadata.SetNpgsqlHiLoSequenceSchema(schema, fromDataAnnotation);

            return name == null ? null : modelBuilder.HasSequence(name, schema, fromDataAnnotation);
        }

        /// <summary>
        ///     Returns a value indicating whether the given name and schema can be set for the hi-lo sequence.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the given name and schema can be set for the hi-lo sequence. </returns>
        public static bool CanSetHiLoSequence(
            [NotNull] this IConventionModelBuilder modelBuilder,
            [CanBeNull] string name,
            [CanBeNull] string schema,
            bool fromDataAnnotation = false)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));
            Check.NullButNotEmpty(schema, nameof(schema));

            return modelBuilder.CanSetAnnotation(NpgsqlAnnotationNames.HiLoSequenceName, name, fromDataAnnotation)
                   && modelBuilder.CanSetAnnotation(NpgsqlAnnotationNames.HiLoSequenceSchema, schema, fromDataAnnotation);
        }

        #endregion HiLo

        #region Serial

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL SERIAL feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL.
        /// </para>
        /// <para>
        /// This option should be considered deprecated starting with PostgreSQL 10, consider using <see cref="UseIdentityColumns"/> instead.
        /// </para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static ModelBuilder UseSerialColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.SerialColumn);
            property.SetNpgsqlHiLoSequenceName(null);
            property.SetNpgsqlHiLoSequenceSchema(null);

            return modelBuilder;
        }

        #endregion Serial

        #region Identity

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will always be generated as identity, and the application will not be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static ModelBuilder UseIdentityAlwaysColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityAlwaysColumn);
            property.SetNpgsqlHiLoSequenceName(null);
            property.SetNpgsqlHiLoSequenceSchema(null);

            return modelBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will be generated as identity by default, but the application will be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static ModelBuilder UseIdentityByDefaultColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.SetNpgsqlValueGenerationStrategy(NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            property.SetNpgsqlHiLoSequenceName(null);
            property.SetNpgsqlHiLoSequenceSchema(null);

            return modelBuilder;
        }

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will be generated as identity by default, but the application will be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>
        /// This is the default behavior when targeting PostgreSQL. Available only starting PostgreSQL 10.
        /// </para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static ModelBuilder UseIdentityColumns(
            [NotNull] this ModelBuilder modelBuilder)
            => modelBuilder.UseIdentityByDefaultColumns();

        /// <summary>
        /// Configures the value generation strategy for the key property, when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder">The builder for the property being configured.</param>
        /// <param name="valueGenerationStrategy">The value generation strategy.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        /// The same builder instance if the configuration was applied, <c>null</c> otherwise.
        /// </returns>
        public static IConventionModelBuilder HasValueGenerationStrategy(
            [NotNull] this IConventionModelBuilder modelBuilder,
            NpgsqlValueGenerationStrategy? valueGenerationStrategy,
            bool fromDataAnnotation = false)
        {
            if (modelBuilder.CanSetAnnotation(
                NpgsqlAnnotationNames.ValueGenerationStrategy, valueGenerationStrategy, fromDataAnnotation))
            {
                modelBuilder.Metadata.SetNpgsqlValueGenerationStrategy(valueGenerationStrategy, fromDataAnnotation);
                if (valueGenerationStrategy != NpgsqlValueGenerationStrategy.SequenceHiLo)
                {
                    modelBuilder.HasHiLoSequence(null, null, fromDataAnnotation);
                }

                return modelBuilder;
            }

            return null;
        }

        #endregion Identity

        #region Extensions

        /// <summary>
        /// Registers a PostgreSQL extension in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to define the extension.</param>
        /// <param name="schema">The schema in which to create the extension.</param>
        /// <param name="name">The name of the extension to create.</param>
        /// <param name="version">The version of the extension.</param>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/external-extensions.html
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="modelBuilder"/></exception>
        [NotNull]
        public static ModelBuilder HasPostgresExtension(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema,
            [NotNull] string name,
            [CanBeNull] string version)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));

            modelBuilder.Model.GetOrAddPostgresExtension(schema, name, version);

            return modelBuilder;
        }

        /// <summary>
        /// Registers a PostgreSQL extension in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to define the extension.</param>
        /// <param name="name">The name of the extension to create.</param>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/external-extensions.html
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="modelBuilder"/></exception>
        [NotNull]
        public static ModelBuilder HasPostgresExtension(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name)
            => modelBuilder.HasPostgresExtension(null, name, null);

        #endregion

        #region Enums

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="schema">The schema in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="labels">The enum label values.</param>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [NotNull]
        public static ModelBuilder HasPostgresEnum(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(labels, nameof(labels));

            modelBuilder.Model.GetOrAddPostgresEnum(schema, name, labels);
            return modelBuilder;
        }

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="labels">The enum label values.</param>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [NotNull]
        public static ModelBuilder HasPostgresEnum(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] string[] labels)
            => modelBuilder.HasPostgresEnum(null, name, labels);

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="schema">The schema in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="nameTranslator">
        /// The translator for name and label inference.
        /// Defaults to <see cref="NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [NotNull]
        public static ModelBuilder HasPostgresEnum<TEnum>(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema = null,
            [CanBeNull] string name = null,
            [CanBeNull] INpgsqlNameTranslator nameTranslator = null)
            where TEnum : struct, Enum
        {
            if (nameTranslator == null)
                nameTranslator = NpgsqlConnection.GlobalTypeMapper.DefaultNameTranslator;

            return modelBuilder.HasPostgresEnum(
                schema,
                name ?? GetTypePgName<TEnum>(nameTranslator),
                GetMemberPgNames<TEnum>(nameTranslator));
        }

        #endregion

        #region Templates

        public static ModelBuilder UseDatabaseTemplate(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string templateDatabaseName)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(templateDatabaseName, nameof(templateDatabaseName));

            modelBuilder.Model.SetNpgsqlDatabaseTemplate(templateDatabaseName);
            return modelBuilder;
        }

        #endregion

        #region Ranges

        /// <summary>
        /// Registers a user-defined range type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder on which to create the range type.</param>
        /// <param name="schema">The schema in which to create the range type.</param>
        /// <param name="name">The name of the range type to be created.</param>
        /// <param name="subtype">The subtype (or element type) of the range</param>
        /// <param name="canonicalFunction">
        /// An optional PostgreSQL function which converts range values to a canonical form.
        /// </param>
        /// <param name="subtypeOpClass">Used to specify a non-default operator class.</param>
        /// <param name="collation">Used to specify a non-default collation in the range's order.</param>
        /// <param name="subtypeDiff">
        /// An optional PostgreSQL function taking two values of the subtype type as argument, and return a double
        /// precision value representing the difference between the two given values.
        /// </param>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/rangetypes.html,
        /// https://www.postgresql.org/docs/current/static/sql-createtype.html,
        /// </remarks>
        [NotNull]
        public static ModelBuilder HasPostgresRange(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            string canonicalFunction = null,
            string subtypeOpClass = null,
            string collation = null,
            string subtypeDiff = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(name, nameof(name));
            Check.NotEmpty(subtype, nameof(subtype));

            modelBuilder.Model.GetOrAddPostgresRange(
                schema,
                name,
                subtype,
                canonicalFunction,
                subtypeOpClass,
                collation,
                subtypeDiff);
            return modelBuilder;
        }

        /// <summary>
        /// Registers a user-defined range type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder on which to create the range type.</param>
        /// <param name="name">The name of the range type to be created.</param>
        /// <param name="subtype">The subtype (or element type) of the range</param>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/rangetypes.html,
        /// https://www.postgresql.org/docs/current/static/sql-createtype.html,
        /// </remarks>
        public static ModelBuilder HasPostgresRange(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] string subtype)
            => HasPostgresRange(modelBuilder, null, name, subtype);

        #endregion

        #region Tablespaces

        public static ModelBuilder UseTablespace(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string tablespace)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NotEmpty(tablespace, nameof(tablespace));

            modelBuilder.Model.SetNpgsqlTablespace(tablespace);
            return modelBuilder;
        }

        #endregion

        #region Helpers

        // See: https://github.com/npgsql/npgsql/blob/dev/src/Npgsql/TypeMapping/TypeMapperBase.cs#L132-L138
        [NotNull]
        static string GetTypePgName<TEnum>([NotNull] INpgsqlNameTranslator nameTranslator) where TEnum : struct, Enum
            => typeof(TEnum).GetCustomAttribute<PgNameAttribute>()?.PgName ??
               nameTranslator.TranslateTypeName(typeof(TEnum).Name);

        // See: https://github.com/npgsql/npgsql/blob/dev/src/Npgsql/TypeHandlers/EnumHandler.cs#L118-L129
        [NotNull]
        [ItemNotNull]
        static string[] GetMemberPgNames<TEnum>([NotNull] INpgsqlNameTranslator nameTranslator) where TEnum : struct, Enum
            => typeof(TEnum)
               .GetFields(BindingFlags.Static | BindingFlags.Public)
               .Select(x => x.GetCustomAttribute<PgNameAttribute>()?.PgName ??
                            nameTranslator.TranslateMemberName(x.Name))
               .ToArray();

        #endregion

        #region Obsolete

        /// <summary>
        /// Configures the model to use a sequence-based hi-lo pattern to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <param name="name">The name of the sequence.</param>
        /// <param name="schema">The schema of the sequence.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseHiLo")]
        public static ModelBuilder ForNpgsqlUseSequenceHiLo([NotNull] this ModelBuilder modelBuilder, [CanBeNull] string name = null, [CanBeNull] string schema = null)
            => modelBuilder.UseHiLo(name, schema);

        /// <summary>
        /// Configures the model to use the PostgreSQL SERIAL feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. This is the default
        /// behavior when targeting PostgreSQL.
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseSerialColumns")]
        public static ModelBuilder ForNpgsqlUseSerialColumns([NotNull] this ModelBuilder modelBuilder)
            => modelBuilder.UseSerialColumns();

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will always be generated as identity, and the application will not be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseIdentityAlwaysColumns")]
        public static ModelBuilder ForNpgsqlUseIdentityAlwaysColumns([NotNull] this ModelBuilder modelBuilder)
            => modelBuilder.UseIdentityAlwaysColumns();

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will be generated as identity by default, but the application will be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseIdentityByDefaultColumns")]
        public static ModelBuilder ForNpgsqlUseIdentityByDefaultColumns([NotNull] this ModelBuilder modelBuilder)
            => modelBuilder.UseIdentityByDefaultColumns();

        /// <summary>
        /// <para>
        /// Configures the model to use the PostgreSQL IDENTITY feature to generate values for properties
        /// marked as <see cref="ValueGenerated.OnAdd" />, when targeting PostgreSQL. Values for these
        /// columns will be generated as identity by default, but the application will be able to override
        /// this behavior by providing a value.
        /// </para>
        /// <para>Available only starting PostgreSQL 10.</para>
        /// </summary>
        /// <param name="modelBuilder">The model builder.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        [Obsolete("Use UseIdentityColumns")]
        public static ModelBuilder ForNpgsqlUseIdentityColumns([NotNull] this ModelBuilder modelBuilder)
            => modelBuilder.UseIdentityColumns();

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="schema">The schema in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="labels">The enum label values.</param>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [Obsolete("Use HasPostgresEnum")]
        public static ModelBuilder ForNpgsqlHasEnum(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string[] labels)
            => modelBuilder.HasPostgresEnum(schema, name, labels);

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="labels">The enum label values.</param>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [Obsolete("Use HasPostgresEnum")]
        public static ModelBuilder ForNpgsqlHasEnum(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] string[] labels)
            => modelBuilder.HasPostgresEnum(name, labels);

        /// <summary>
        /// Registers a user-defined enum type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder in which to create the enum type.</param>
        /// <param name="schema">The schema in which to create the enum type.</param>
        /// <param name="name">The name of the enum type to create.</param>
        /// <param name="nameTranslator">
        /// The translator for name and label inference.
        /// Defaults to <see cref="NpgsqlSnakeCaseNameTranslator"/>.</param>
        /// <typeparam name="TEnum"></typeparam>
        /// <returns>
        /// The updated <see cref="ModelBuilder"/>.
        /// </returns>
        /// <remarks>
        /// See: https://www.postgresql.org/docs/current/static/datatype-enum.html
        /// </remarks>
        /// <exception cref="ArgumentNullException">builder</exception>
        [Obsolete("Use HasPostgresEnum")]
        public static ModelBuilder ForNpgsqlHasEnum<TEnum>(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema = null,
            [CanBeNull] string name = null,
            [CanBeNull] INpgsqlNameTranslator nameTranslator = null)
            where TEnum : struct, Enum
            => modelBuilder.HasPostgresEnum<TEnum>(schema, name, nameTranslator);

        [Obsolete("Use UseDatabaseTemplate")]
        public static ModelBuilder HasDatabaseTemplate([NotNull] this ModelBuilder modelBuilder, [NotNull] string templateDatabaseName)
            => modelBuilder.UseDatabaseTemplate(templateDatabaseName);

        /// <summary>
        /// Registers a user-defined range type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder on which to create the range type.</param>
        /// <param name="schema">The schema in which to create the range type.</param>
        /// <param name="name">The name of the range type to be created.</param>
        /// <param name="subtype">The subtype (or element type) of the range</param>
        /// <param name="canonicalFunction">
        /// An optional PostgreSQL function which converts range values to a canonical form.
        /// </param>
        /// <param name="subtypeOpClass">Used to specify a non-default operator class.</param>
        /// <param name="collation">Used to specify a non-default collation in the range's order.</param>
        /// <param name="subtypeDiff">
        /// An optional PostgreSQL function taking two values of the subtype type as argument, and return a double
        /// precision value representing the difference between the two given values.
        /// </param>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/rangetypes.html,
        /// https://www.postgresql.org/docs/current/static/sql-createtype.html,
        /// </remarks>
        [Obsolete("Use HasPostgresRange")]
        public static ModelBuilder ForNpgsqlHasRange(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] string subtype,
            string canonicalFunction = null,
            string subtypeOpClass = null,
            string collation = null,
            string subtypeDiff = null)
            => modelBuilder.HasPostgresRange(schema, name, subtype, canonicalFunction, subtype, collation, subtypeDiff);

        /// <summary>
        /// Registers a user-defined range type in the model.
        /// </summary>
        /// <param name="modelBuilder">The model builder on which to create the range type.</param>
        /// <param name="name">The name of the range type to be created.</param>
        /// <param name="subtype">The subtype (or element type) of the range</param>
        /// <remarks>
        /// See https://www.postgresql.org/docs/current/static/rangetypes.html,
        /// https://www.postgresql.org/docs/current/static/sql-createtype.html,
        /// </remarks>
        [Obsolete("Use HasPostgresRange")]
        public static ModelBuilder ForNpgsqlHasRange(
            [NotNull] this ModelBuilder modelBuilder,
            [NotNull] string name,
            [NotNull] string subtype)
            => modelBuilder.HasPostgresRange(name, subtype);

        [Obsolete("Use UseTablespace")]
        public static ModelBuilder ForNpgsqlUseTablespace([NotNull] this ModelBuilder modelBuilder, [NotNull] string tablespace)
            => modelBuilder.UseTablespace(tablespace);

        #endregion Obsolete
    }
}
