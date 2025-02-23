// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ao.SavableConfig
{
    /// <summary>
    /// Extensions method for <see cref="IConfigurationRoot"/>
    /// </summary>
    internal static class InternalConfigurationRootExtensions
    {
        /// <summary>
        /// Gets the immediate children sub-sections of configuration root based on key.
        /// </summary>
        /// <param name="root">Configuration from which to retrieve sub-sections.</param>
        /// <param name="path">Key of a section of which children to retrieve.</param>
        /// <returns>Immediate children sub-sections of section specified by key.</returns>
#if NETSTANDARD2_0
        internal static IEnumerable<IConfigurationSection> GetChildrenImplementation(this IConfigurationRoot root, string path)
#else
        internal static IEnumerable<IConfigurationSection> GetChildrenImplementation(this SavableConfigurationRoot root, string path)
#endif
        {
            return root.Providers
                .Aggregate(Enumerable.Empty<string>(),
                    (seed, source) => source.GetChildKeys(seed, path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(key => root.GetSection(path == null ? key : ConfigurationPath.Combine(path, key)));
        }
    }
}
