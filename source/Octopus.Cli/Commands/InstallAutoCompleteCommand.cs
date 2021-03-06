using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Octopus.Cli.Commands.ShellCompletion;
using Octopus.Cli.Infrastructure;
using Octopus.Cli.Util;

namespace Octopus.Cli.Commands
{
    public enum SupportedShell
    {
        Unspecified,
        Pwsh,
        Zsh,
        Bash,
        Powershell
    }
    
    [Command(name: "install-autocomplete", Description = "Install a shell auto-complete script into your shell profile, if they aren't already there. Supports pwsh, zsh, bash & powershell.")]
    public class InstallAutoCompleteCommand : CommandBase
    {
        private readonly IEnumerable<ShellCompletionInstaller> installers;

        private readonly string supportedShells = 
            Enum.GetNames(typeof(SupportedShell))
                .Except(new [] {SupportedShell.Unspecified.ToString()})
                .ReadableJoin();
        
        public InstallAutoCompleteCommand(ICommandOutputProvider commandOutputProvider, IEnumerable<ShellCompletionInstaller> installers) : base(commandOutputProvider)
        {
            this.installers = installers;

            var options = Options.For("Install AutoComplete");
            options.Add<SupportedShell>("shell=",
                $"The type of shell to install auto-complete scripts for. This will alter your shell configuration files. Supported shells are {supportedShells}.",
                v => { ShellSelection = v; });
            options.Add<bool>("dryRun",
                "[Optional] Dry run will output the proposed changes to console, instead of writing to disk.",
                v => DryRun = true);
        }

        public bool DryRun { get; set; }

        public SupportedShell ShellSelection { get; set; }

        public override Task Execute(string[] commandLineArguments)
        {
            Options.Parse(commandLineArguments);
            if (printHelp)
            {
                GetHelp(Console.Out, commandLineArguments);
                return Task.FromResult(0);
            }
            
            var invalidShellSelectionMessage = $"Please specify the type of shell to install auto-completion for: --shell=XYZ. Valid values are {supportedShells}.";
            if (ShellSelection == SupportedShell.Unspecified) throw new CommandException(invalidShellSelectionMessage);

            commandOutputProvider.PrintHeader();
            if (DryRun) commandOutputProvider.Warning("DRY RUN");
            commandOutputProvider.Information($"Installing auto-complete scripts for {ShellSelection}");
            return Task.Run(() =>
            {
                switch (ShellSelection)
                {
                    case SupportedShell.Pwsh:
                        InstallForPwsh();
                        break;
                    case SupportedShell.Zsh:
                        InstallForZsh();
                        break;
                    case SupportedShell.Bash:
                        InstallForBash();
                        break;
                    case SupportedShell.Powershell:
                        InstallForPowershell();
                        break;
                    default:
                        throw new CommandException(invalidShellSelectionMessage);
                }
            });
        }

        private void InstallForBash()
        {
            var installer = installers.Single(i => i.GetType() == typeof(BashCompletionInstaller));
            installer.Install(DryRun);
        }

        private void InstallForZsh()
        {
            var installer = installers.Single(i => i.GetType() == typeof(ZshCompletionInstaller));
            installer.Install(DryRun);
        }

        private void InstallForPwsh()
        {
            var installer = installers.Single(i => i.GetType() == typeof(PwshCompletionInstaller));
            installer.Install(DryRun);
        }

        private void InstallForPowershell()
        {
            
            var installer = installers.Single(i => i.GetType() == typeof(PowershellCompletionInstaller));
            installer.Install(DryRun);
            
            
        }
    }
}
