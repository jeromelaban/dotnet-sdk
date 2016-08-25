﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.DotNet.Cli.Utils.CommandResolution;

namespace Microsoft.DotNet.Cli.Utils
{
    public class PublishedPathCommandResolver : ICommandResolver
    {
        private readonly IEnvironmentProvider _environment;
        private readonly IPublishedPathCommandSpecFactory _commandSpecFactory;

        public PublishedPathCommandResolver(
            IEnvironmentProvider environment,
            IPublishedPathCommandSpecFactory commandSpecFactory)
        {
            _environment = environment;
            _commandSpecFactory = commandSpecFactory;
        }

        public CommandSpec Resolve(CommandResolverArguments commandResolverArguments)
        {
            var publishDirectory = commandResolverArguments.OutputPath;
            var commandName = commandResolverArguments.CommandName;
            var applicationName = commandResolverArguments.ApplicationName;

            if (publishDirectory == null || commandName == null || applicationName == null)
            {
                return null;
            }

            var commandPath = ResolveCommandPath(publishDirectory, commandName);

            if (commandPath == null)
            {
                return null;
            }

            var depsFilePath = Path.Combine(publishDirectory, $"{applicationName}.deps.json");
            if (!File.Exists(depsFilePath))
            {
                Reporter.Verbose.WriteLine($"PublishedPathCommandResolver: {depsFilePath} does not exist");
                return null;
            }

            var runtimeConfigPath = Path.Combine(publishDirectory, $"{applicationName}.runtimeconfig.json");
            if (!File.Exists(runtimeConfigPath))
            {
                Reporter.Verbose.WriteLine($"projectdependenciescommandresolver: {runtimeConfigPath} does not exist");
                return null;
            }

            return _commandSpecFactory.CreateCommandSpecFromPublishFolder(
                    commandPath,
                    commandResolverArguments.CommandArguments.OrEmptyIfNull(),
                    CommandResolutionStrategy.OutputPath,
                    depsFilePath,
                    runtimeConfigPath);
        }

        private string ResolveCommandPath(string publishDirectory, string commandName)
        {
            if (!Directory.Exists(publishDirectory))
            {
                Reporter.Verbose.WriteLine($"publishedpathresolver: {publishDirectory} does not exist");
                return null;
            }

            return _environment.GetCommandPathFromRootPath(publishDirectory, commandName, ".dll");
        }      
    }
}
