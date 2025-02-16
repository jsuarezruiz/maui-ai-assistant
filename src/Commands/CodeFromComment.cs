namespace MAUI_AI_Assistant.Commands
{
    [Command(PackageIds.CodeFromComment)]
    internal sealed class CodeFromComment : MAUIAIBaseCommand<CodeFromComment>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            SystemMessage = @"
            Turn comments into C# code.
            - Create any helper classes that are necessary.
            - Follow the Common C# code conventions from https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions           
            - Write only the code, not the explanation.";
            CommandBehavior = CommandBehavior.Insert;

            await base.ExecuteAsync(e);
        }
    }
}