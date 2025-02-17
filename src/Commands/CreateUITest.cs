using EnvDTE;
using EnvDTE80;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.CreateUITest)]
    internal sealed class CreateUITest : MAUIAIBaseCommand<CreateUITest>
    {
        const string CSHARP = ".cs";
        const string XAML = ".xaml.cs";

        readonly HashSet<string> SupportedFiles = new HashSet<string> { CSHARP, XAML };

        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            SystemMessage = @$"
            Create a class with UI Test methods for {PromptConstants.Framework}." +
            $"- Use the extension methods from https://github.com/dotnet/maui/blob/d6cc8154b47e53290571fe465de56ce9632e5f4c/src/TestUtils/src/UITest.Appium/HelperExtensions.cs#L11" +
            $"- Use the tests from https://github.com/dotnet/maui/tree/d6cc8154b47e53290571fe465de56ce9632e5f4c/src/Controls/tests/TestCases.Shared.Tests/Tests as reference." +
            $"- Use UITest class as base class to inherit from." +
            $"- Write only the code, not the explanation.";

            ChatMessage = await GetCodeAsync();
            CommandBehavior = CommandBehavior.Insert;

            await base.ExecuteAsync(e);
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            try
            {
                Command.Visible = CanShow();
            }
            catch (Exception ex)
            {
                Command.Visible = false;
                ex.Log();
            }
        }

        [SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread")]
        bool CanShow()
        {
            if (!ThreadHelper.CheckAccess()) 
                return false;

            var dte = (DTE2)(ServiceProvider.GlobalProvider.GetService(typeof(DTE)));
            var item = dte?.SelectedItems?.Item(1)?.ProjectItem;

            if (item == null)
                return false;

            var fileExtension = Path.GetExtension(item.Name);

            // Show the button only if a supported file is selected
            return SupportedFiles.Contains(fileExtension);
        }

        async Task<string> GetCodeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            var dte = (DTE2)(ServiceProvider.GlobalProvider.GetService(typeof(DTE)));
            var itemProjectItem = dte?.SelectedItems?.Item(1)?.ProjectItem;

            string result = string.Empty;

            if (itemProjectItem.Name.Contains(XAML))
            {
                ProjectItem parentProjectItem;

                if (itemProjectItem.ProjectItems.Count == 0)
                    parentProjectItem = itemProjectItem.Collection.Parent as ProjectItem;
                else
                    parentProjectItem = dte.ActiveDocument.ProjectItem;

                var parentPath = parentProjectItem.Properties.Item("FullPath").Value as string;
                result = File.ReadAllText(parentPath);
            }
            else
            {
                var itemPath = itemProjectItem.Properties.Item("FullPath").Value as string;
                result = File.ReadAllText(itemPath);
            }

            return result;
        }
    }
}
