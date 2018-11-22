﻿using System;
using System.Diagnostics;
using Rocket.API.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Rocket.API;
using Rocket.API.Commands;
using Rocket.API.Permissions;
using Rocket.API.Plugins;
using Rocket.Core.Plugins.NuGet;
using Rocket.Core.User;
using Rocket.API.User;

namespace Rocket.Core.Commands.RocketCommands
{
    public class CommandRocket : ICommand
    {
        public string Name => "Rocket";
        public string Syntax => "";
        public string Summary => "Manages RocketMod.";
        public string Description => null;
        public string[] Aliases => null;

        public IChildCommand[] ChildCommands => new IChildCommand[]
        {
            new CommandRocketInstall(), new CommandRocketUninstall(),
            new CommandRocketReload(), new CommandRocketUpdate(),
            new CommandRocketVersion()
        };

        public async Task ExecuteAsync(ICommandContext context)
        {
            throw new CommandWrongUsageException();
        }

        public bool  SupportsUser(IUser user) => true;
    }

    public class CommandRocketReload : IChildCommand
    {
        public string Name => "Reload";
        public string Summary => "Reloads RocketMod and all plugins.";
        public string Description => null;
        public string Syntax => "";
        public IChildCommand[] ChildCommands => null;
        public string[] Aliases => null;

        public bool  SupportsUser(IUser user) => true;

        public async Task ExecuteAsync(ICommandContext context)
        {
            IPermissionProvider permissions = context.Container.Resolve<IPermissionProvider>();
            await permissions.ReloadAsync();

            foreach (IPlugin plugin in context.Container.Resolve<IPluginLoader>()) plugin.Deactivate();

            foreach (IPlugin plugin in context.Container.Resolve<IPluginLoader>()) plugin.Activate(true);

            await context.User.SendMessageAsync("Reload completed.", Color.DarkGreen);
        }
    }

    public class CommandRocketInstall : IChildCommand
    {
        public string Name => "Install";
        public string[] Aliases => null;
        public string Summary => "Installs a plugin";
        public string Description => null;
        public string Syntax => "<repo> <plugin> [version] [-Pre]";
        public IChildCommand[] ChildCommands => null;

        public bool  SupportsUser(IUser user) => true;
        public async Task ExecuteAsync(ICommandContext context)
        {
            if (context.Parameters.Length < 2)
                throw new CommandWrongUsageException();

            NuGetPluginLoader pm = (NuGetPluginLoader)context.Container.Resolve<IPluginLoader>("nuget_plugins");

            var args = context.Parameters.ToList();

            string repoName = args[0];
            string pluginName = args[1];
            string version = null;
            bool isPre = false;

            if (args.Contains("-Pre"))
            {
                isPre = true;
                args.Remove("-Pre");
            }

            if (args.Count > 2)
                version = args[2];

            var repo = pm.Repositories.FirstOrDefault(c
                => c.Name.Equals(repoName, StringComparison.OrdinalIgnoreCase));

            if (repo == null)
            {
                await context.User.SendMessageAsync("Repository not found: " + repoName, Color.DarkRed);
                return;
            }

            var result = pm.Install(repoName, pluginName, version, isPre);
            if (result != NuGetInstallResult.Success)
            {
                await context.User.SendMessageAsync($"Failed to install \"{pluginName}\": " + result, Color.DarkRed);
                return;
            }

            if (!pm.LoadPlugin(repoName, pluginName))
            {
                await context.User.SendMessageAsync($"Failed to initialize \"{pluginName}\"", Color.DarkRed);
                pm.Uninstall(repoName, pluginName);
                return;
            }

            await context.User.SendMessageAsync($"Successfully installed \"{pluginName}\"", Color.DarkGreen);
        }
    }

    public class CommandRocketUninstall : IChildCommand
    {
        public string Name => "Uninstall";
        public string[] Aliases => null;
        public string Summary => "Uninstalls plugin";
        public string Description => null;
        public string Syntax => "<repo> <plugin>";
        public IChildCommand[] ChildCommands => null;

        public bool  SupportsUser(IUser user) => true;

        public async Task ExecuteAsync(ICommandContext context)
        {
            if (context.Parameters.Length != 2)
                throw new CommandWrongUsageException();

            NuGetPluginLoader pm = (NuGetPluginLoader)context.Container.Resolve<IPluginLoader>("nuget_plugins");

            string repoName = context.Parameters[0];
            string pluginName = context.Parameters[1];

            var repo = pm.Repositories.FirstOrDefault(c
                => c.Name.Equals(repoName, StringComparison.OrdinalIgnoreCase));

            if (repo == null)
            {
                await context.User.SendMessageAsync("Repository not found: " + repoName, Color.DarkRed);
                return;
            }

            if (!pm.Uninstall(repoName, pluginName))
            {
                await context.User.SendMessageAsync($"Failed to uninstall \"{pluginName}\"", Color.DarkRed);
                return;
            }

            await context.User.SendMessageAsync($"Successfully uninstalled  \"{pluginName}\"", Color.DarkGreen);
            await context.User.SendMessageAsync("Restart server to finish uninstall.", Color.Red);
        }
    }

    public class CommandRocketUpdate : IChildCommand
    {
        public string Name => "Update";
        public string[] Aliases => null;
        public string Summary => "Updates plugin";
        public string Description => null;
        public string Syntax => "<repo> <plugin> [version] [-Pre]";
        public IChildCommand[] ChildCommands => null;

        public bool  SupportsUser(IUser user) => true;

        public async Task ExecuteAsync(ICommandContext context)
        {
            if (context.Parameters.Length < 2)
                throw new CommandWrongUsageException();

            NuGetPluginLoader pm = (NuGetPluginLoader)context.Container.Resolve<IPluginLoader>("nuget_plugins");

            var args = context.Parameters.ToList();

            string repoName = args[0];
            string pluginName = args[1];
            string version = null;
            bool isPre = false;

            if (args.Contains("-Pre"))
            {
                isPre = true;
                args.Remove("-Pre");
            }

            if (args.Count > 2)
                version = args[2];

            var repo = pm.Repositories.FirstOrDefault(c
                => c.Name.Equals(repoName, StringComparison.OrdinalIgnoreCase));

            if (repo == null)
            {
                await context.User.SendMessageAsync("Repository not found: " + repoName, Color.DarkRed);
                return;
            }

            var result = pm.Update(repoName, pluginName, version, isPre);
            if (result != NuGetInstallResult.Success)
            {
                await context.User.SendMessageAsync($"Failed to update \"{pluginName}\": " + result, Color.DarkRed);
                return;
            }

            await context.User.SendMessageAsync($"Successfully updated \"{pluginName}\"", Color.DarkGreen);
            await context.User.SendMessageAsync("Restart server to finish update.", Color.Red);
        }
    }

    public class CommandRocketVersion : IChildCommand
    {
        public string Name => "Version";
        public string[] Aliases => new[] { "v" };
        public string Summary => "RocketMod version";
        public string Description => null;
        public string Syntax => "";
        public IChildCommand[] ChildCommands => null;
        public bool  SupportsUser(IUser user) => true;

        public async Task ExecuteAsync(ICommandContext context)
        {
            var runtime = context.Container.Resolve<IRuntime>();
            var host = context.Container.Resolve<IHost>();

            FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(typeof(IRuntime).Assembly.Location);

            await context.User.SendMessageAsync("Rocket Version: " + versionInfo.FileVersion, Color.Cyan);
            await context.User.SendMessageAsync(runtime.Name + " Version: " + runtime.Version, Color.Cyan);
            await context.User.SendMessageAsync(host.Name + " Version: " + host.HostVersion, Color.Blue);

            if (host.Name != host.GameName)
                await context.User.SendMessageAsync(host.GameName + " Version: " + host.GameVersion, Color.Green);
        }
    }
}