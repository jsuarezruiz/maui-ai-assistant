using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MAUI_AI_Assistant
{
    public class DesignToCode : BaseToolWindow<DesignToCode>
    {
        public override string GetTitle(int toolWindowId) => "Design to Code";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new DesignToCodeControl { DataContext = new DesignToCodeViewModel() });
        }

        [Guid("2c862557-980f-493c-9851-cf7b96bb9f22")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}