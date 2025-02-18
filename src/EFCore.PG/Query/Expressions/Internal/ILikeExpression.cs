﻿using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a PostgreSQL ILIKE expression.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class ILikeExpression : SqlExpression, IEquatable<ILikeExpression>
    {
        /// <summary>
        /// The match expression.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Match { get; }

        /// <summary>
        /// The pattern to match.
        /// </summary>
        [NotNull]
        public virtual SqlExpression Pattern { get; }

        /// <summary>
        /// The escape character to use in <see cref="Pattern"/>.
        /// </summary>
        [CanBeNull]
        public virtual SqlExpression EscapeChar { get; }

        /// <summary>
        /// Constructs a <see cref="ILikeExpression"/>.
        /// </summary>
        /// <param name="match">The expression to match.</param>
        /// <param name="pattern">The pattern to match.</param>
        /// <param name="escapeChar">The escape character to use in <paramref name="pattern"/>.</param>
        /// <exception cref="ArgumentNullException" />
        public ILikeExpression([NotNull] SqlExpression match, [NotNull] SqlExpression pattern, [CanBeNull] SqlExpression escapeChar, RelationalTypeMapping typeMapping)
            : base(typeof(bool), typeMapping)
        {
            Match = match;
            Pattern = pattern;
            EscapeChar = escapeChar;
        }

        /// <inheritdoc />
        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is NpgsqlQuerySqlGenerator npgsqlGenerator
                ? npgsqlGenerator.VisitILike(this)
                : base.Accept(visitor);

        /// <inheritdoc />
        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update(
                (SqlExpression)visitor.Visit(Match),
                (SqlExpression)visitor.Visit(Pattern),
                (SqlExpression)visitor.Visit(EscapeChar));

        public ILikeExpression Update(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar)
            => match == Match && pattern == Pattern && escapeChar == EscapeChar
                ? this
                : new ILikeExpression(match, pattern, escapeChar, TypeMapping);

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is ILikeExpression other && Equals(other);

        /// <inheritdoc />
        public bool Equals(ILikeExpression other)
            => ReferenceEquals(this, other) ||
               other is object &&
               base.Equals(other) &&
               Equals(Match, other.Match) &&
               Equals(Pattern, other.Pattern) &&
               Equals(EscapeChar, other.EscapeChar);

        /// <inheritdoc />
        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Match, Pattern, EscapeChar);

        public override void Print(ExpressionPrinter expressionPrinter)
        {
#pragma warning disable EF1001
            expressionPrinter.Visit(Match);
            expressionPrinter.StringBuilder.Append(" ILIKE ");
            expressionPrinter.Visit(Pattern);

            if (EscapeChar != null)
            {
                expressionPrinter.StringBuilder.Append(" ESCAPE ");
                expressionPrinter.Visit(EscapeChar);
            }
#pragma warning restore EF1001
        }

        /// <inheritdoc />
        public override string ToString() => $"{Match} ILIKE {Pattern}{(EscapeChar == null ? "" : $" ESCAPE {EscapeChar}")}";
    }
}
