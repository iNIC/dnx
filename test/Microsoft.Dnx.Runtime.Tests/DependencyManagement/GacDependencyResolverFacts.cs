﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.Testing.xunit;
using Microsoft.Dnx.Runtime.Helpers;
using NuGet;
using Xunit;

namespace Microsoft.Dnx.Runtime.Tests
{
    public class GacDependencyResolverFacts
    {
        // NOTE(anurse): Disabling tests for frameworks < .NET 4.0 because the CI doesn't have them installed :(
        [ConditionalTheory]
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
        [InlineData("mscorlib", "4.0.0.0", "dnx451", @"%Windir%\Microsoft.NET\assembly\GAC_32\mscorlib\v4.0_4.0.0.0__b77a5c561934e089\mscorlib.dll", true)]
        //[InlineData("mscorlib", "2.0.0.0", "net20", @"%Windir%\assembly\GAC_32\mscorlib\2.0.0.0__b77a5c561934e089\mscorlib.dll", true)]
        [InlineData("mscorlib", "4.0.0.0", "net20", "", false)]
        [InlineData("mscorlib", "2.0.0.0", "dnx451", "", false)]
        [InlineData("mscorlib", "1.0.0.0", "dnx451", "", false)]
        [InlineData("mscorlib", "4.0.0.0", "dnxcore50", "", false)]
        public void GetDescriptionReturnsCorrectResults(string name, string version, string framework, string path, bool found)
        {
            var libraryRange = new LibraryRange(name, frameworkReference: true)
            {
                VersionRange = VersionUtility.ParseVersionRange(version)
            };

            var frameworkName = FrameworkNameHelper.ParseFrameworkName(framework);
            var resolver = new GacDependencyResolver();
            var library = resolver.GetDescription(libraryRange, frameworkName);

            if (found)
            {
                Assert.NotNull(library);
                Assert.Equal(SemanticVersion.Create(version), library.Identity.Version);
                Assert.Equal(Environment.ExpandEnvironmentVariables(path), library.Path);
            }
            else
            {
                Assert.Null(library);
            }
        }

        [ConditionalTheory]
        [FrameworkSkipCondition(RuntimeFrameworks.Mono)]
        [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
        [InlineData("net20", @"%WinDir%\assembly\GAC_32\{name}\{version}\{name}.dll,%WinDir%\assembly\GAC_64\{name}\{version}\{name}.dll,%WinDir%\assembly\GAC_MSIL\{name}\{version}\{name}.dll")]
        [InlineData("net30", @"%WinDir%\assembly\GAC_32\{name}\{version}\{name}.dll,%WinDir%\assembly\GAC_64\{name}\{version}\{name}.dll,%WinDir%\assembly\GAC_MSIL\{name}\{version}\{name}.dll")]
        [InlineData("net35", @"%WinDir%\assembly\GAC_32\{name}\{version}\{name}.dll,%WinDir%\assembly\GAC_64\{name}\{version}\{name}.dll,%WinDir%\assembly\GAC_MSIL\{name}\{version}\{name}.dll")]
        [InlineData("net40", @"%WinDir%\Microsoft.NET\assembly\GAC_32\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_64\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_MSIL\{name}\{version}\{name}.dll")]
        [InlineData("net45", @"%WinDir%\Microsoft.NET\assembly\GAC_32\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_64\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_MSIL\{name}\{version}\{name}.dll")]
        [InlineData("net451", @"%WinDir%\Microsoft.NET\assembly\GAC_32\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_64\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_MSIL\{name}\{version}\{name}.dll")]
        [InlineData("net452", @"%WinDir%\Microsoft.NET\assembly\GAC_32\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_64\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_MSIL\{name}\{version}\{name}.dll")]
        [InlineData("net46", @"%WinDir%\Microsoft.NET\assembly\GAC_32\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_64\{name}\{version}\{name}.dll,%WinDir%\Microsoft.NET\assembly\GAC_MSIL\{name}\{version}\{name}.dll")]
        public void GetAttemptedPathsReportsExpectedPaths(string framework, string expectedPaths)
        {
            var resolver = new GacDependencyResolver();
            var paths = resolver.GetAttemptedPaths(VersionUtility.ParseFrameworkName(framework));
            Assert.Equal(
                expectedPaths.Split(',').Select(s => Environment.ExpandEnvironmentVariables(s)).ToArray(),
                paths.ToArray());
        }
    }
}
