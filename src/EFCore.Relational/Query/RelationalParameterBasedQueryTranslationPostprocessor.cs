﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A class that postprocesses the <see cref="SelectExpression"/> in the translated query after parementer values are known.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalParameterBasedQueryTranslationPostprocessor
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="QueryTranslationPostprocessor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="useRelationalNulls"> A bool value indicating if relational nulls should be used. </param>
        public RelationalParameterBasedQueryTranslationPostprocessor(
            [NotNull] RelationalParameterBasedQueryTranslationPostprocessorDependencies dependencies,
            bool useRelationalNulls)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
            UseRelationalNulls = useRelationalNulls;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual RelationalParameterBasedQueryTranslationPostprocessorDependencies Dependencies { get; }

        /// <summary>
        ///     A bool value indicating if relational nulls should be used.
        /// </summary>
        protected virtual bool UseRelationalNulls { get; }

        /// <summary>
        ///     Optimizes a <see cref="SelectExpression"/> for given parameter values.
        /// </summary>
        /// <param name="selectExpression"> A select expression to optimize. </param>
        /// <param name="parametersValues"> A dictionary of parameter values to use. </param>
        /// <returns> A tuple of optimized select expression and a bool value if it can be cached. </returns>
        public virtual (SelectExpression, bool) Optimize(
            [NotNull] SelectExpression selectExpression,
            [NotNull] IReadOnlyDictionary<string, object> parametersValues)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            var canCache = true;
            var (sqlExpressionOptimized, optimizerCanCache) = new NullabilityBasedSqlProcessingExpressionVisitor(
                Dependencies.SqlExpressionFactory,
                parametersValues,
                UseRelationalNulls).Process(selectExpression);

            canCache &= optimizerCanCache;

            var fromSqlParameterOptimized = new FromSqlParameterApplyingExpressionVisitor(
                Dependencies.SqlExpressionFactory,
                Dependencies.TypeMappingSource,
                Dependencies.ParameterNameGeneratorFactory.Create(),
                parametersValues).Visit(sqlExpressionOptimized);

            if (!ReferenceEquals(sqlExpressionOptimized, fromSqlParameterOptimized))
            {
                canCache = false;
            }

            return ((SelectExpression)fromSqlParameterOptimized, canCache);
        }
    }
}
