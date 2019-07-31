using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Conventions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata.Internal;
using Xunit;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Design.Internal
{
    public class NpgsqlAnnotationCodeGeneratorTest
    {
        [Fact]
        public void GenerateFluentApi_value_generation()
        {
            var defaultOptions = new DbContextOptionsBuilder().Options;
            var generator = new NpgsqlAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies(), defaultOptions);
            var modelBuilder = new ModelBuilder(NpgsqlConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id1").UseIdentityByDefaultColumn();
                    x.Property<int>("Id2").UseIdentityAlwaysColumn();
                    x.Property<int>("Id3").UseSerialColumn();
                });

            var property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Id1");
            var annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.True(generator.IsHandledByConvention(property, annotation));
            var result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal("UseIdentityByDefaultColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Id2");
            annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.False(generator.IsHandledByConvention(property, annotation));
            result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal("UseIdentityAlwaysColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Id3");
            annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.False(generator.IsHandledByConvention(property, annotation));
            result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal("UseSerialColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);
        }

        [Fact]
        public void GenerateFluentApi_value_generation_old_database()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            new NpgsqlDbContextOptionsBuilder(optionsBuilder).SetPostgresVersion(9, 6);
            var options = optionsBuilder.Options;
            var generator = new NpgsqlAnnotationCodeGenerator(new AnnotationCodeGeneratorDependencies(), options);
            var modelBuilder = new ModelBuilder(NpgsqlConventionSetBuilder.Build());
            modelBuilder.Entity(
                "Post",
                x =>
                {
                    x.Property<int>("Id1").UseIdentityByDefaultColumn();
                    x.Property<int>("Id2").UseIdentityAlwaysColumn();
                    x.Property<int>("Id3").UseSerialColumn();
                });

            var property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Id1");
            var annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.False(generator.IsHandledByConvention(property, annotation));
            var result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal("UseIdentityByDefaultColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Id2");
            annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.False(generator.IsHandledByConvention(property, annotation));
            result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal("UseIdentityAlwaysColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);

            property = modelBuilder.Model.FindEntityType("Post").GetProperties()
                .Single(p => p.Name == "Id3");
            annotation = property.FindAnnotation(NpgsqlAnnotationNames.ValueGenerationStrategy);
            Assert.True(generator.IsHandledByConvention(property, annotation));
            result = generator.GenerateFluentApi(property, annotation);
            Assert.Equal("UseSerialColumn", result.Method);
            Assert.Equal(0, result.Arguments.Count);
        }
    }
}
