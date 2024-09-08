global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;

using System.Runtime.InteropServices;
using System.Threading;
using MAUI_AI_Assistant.Options;

namespace MAUI_AI_Assistant
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(PackageGuids.MAUI_AI_AssistantString)]
    [ProvideOptionPage(typeof(OptionsProvider.GeneralOptions), ".NET MAUI AI Assistant", "General", 0, 0, true, SupportsProfiles = true)]
    [ProvideOptionPage(typeof(OptionsProvider.CustomCommandOptions), ".NET MAUI AI Assistant", "Custom Command", 1, 1, true, SupportsProfiles = true)]
    [ProvideToolWindow(typeof(DesignToCode.Pane))]
    [ProvideToolWindow(typeof(IconBuilder.Pane))]
    [ProvideToolWindow(typeof(ImageCreator.Pane))]
    public sealed class MAUIAIAssistantPackage : ToolkitPackage
    {
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {          
            // Tool windows
            this.RegisterToolWindows();

            // Commands
            await this.RegisterCommandsAsync();
        }
    }
}