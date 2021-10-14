﻿// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using TypeScript.TypeConverter.Extensions;
using static TypeScript.TypeConverter.Expressions.SharedRegex;

namespace TypeScript.TypeConverter.Readers;

internal class LibDomReader
{
    private static readonly HttpClient _httpClient = new();

    protected Lazy<IDictionary<string, string>> _lazyTypeDeclarationMap =
        new(() =>
        {
            ConcurrentDictionary<string, string> map = new();

            try
            {
                var rawUrl = "https://raw.githubusercontent.com/microsoft/TypeScript/main/lib/lib.dom.d.ts";
                var libDomDefinitionTypeScript = _httpClient.GetStringAsync(rawUrl).ConfigureAwait(false).GetAwaiter().GetResult();
                if (libDomDefinitionTypeScript is { Length: > 0 })
                {
                    var matchCollection = InterfaceRegex.Matches(libDomDefinitionTypeScript).Select(m => m.Value);
                    Parallel.ForEach(
                        matchCollection,
                        match =>
                        {
                            var typeName = InterfaceTypeNameRegex.GetMatchGroupValue(match, "TypeName");
                            if (typeName is not null)
                            {
                                map[typeName] = match;
                            }
                        });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error intializing lib dom parser. {ex}");
            }

            return map;
        });

    /// <summary>
    /// For testing purposes.
    /// </summary>
    internal bool IsInitialized => _lazyTypeDeclarationMap is { IsValueCreated: true };

    public bool TryGetDeclaration(
        string typeName, [NotNullWhen(true)] out string? declaration) =>
        _lazyTypeDeclarationMap.Value.TryGetValue(typeName, out declaration);
}