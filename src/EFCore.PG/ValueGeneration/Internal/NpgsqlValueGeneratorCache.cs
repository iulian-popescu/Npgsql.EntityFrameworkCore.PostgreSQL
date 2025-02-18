﻿using System.Collections.Concurrent;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Npgsql.EntityFrameworkCore.PostgreSQL.Utilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.ValueGeneration.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NpgsqlValueGeneratorCache : ValueGeneratorCache, INpgsqlValueGeneratorCache
    {
        readonly ConcurrentDictionary<string, NpgsqlSequenceValueGeneratorState> _sequenceGeneratorCache
            = new ConcurrentDictionary<string, NpgsqlSequenceValueGeneratorState>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public NpgsqlValueGeneratorCache([NotNull] ValueGeneratorCacheDependencies dependencies)
            : base(dependencies)
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual NpgsqlSequenceValueGeneratorState GetOrAddSequenceState(
            IProperty property,
            IRelationalConnection connection)
        {
            //var sequence = property.GetNpgsql().FindHiLoSequence();
            var sequence = property.FindNpgsqlHiLoSequence();

            Debug.Assert(sequence != null);

            return _sequenceGeneratorCache.GetOrAdd(
                GetSequenceName(sequence),
                sequenceName => new NpgsqlSequenceValueGeneratorState(sequence));
        }

        static string GetSequenceName(ISequence sequence)
            => (sequence.Schema == null ? "" : sequence.Schema + ".") + sequence.Name;
    }
}
