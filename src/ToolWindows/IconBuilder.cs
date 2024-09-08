using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MAUI_AI_Assistant
{
    public class IconBuilder : BaseToolWindow<IconBuilder>
    {
        public override string GetTitle(int toolWindowId) => "Icon Builder";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new IconBuilderControl { DataContext = new IconBuilderViewModel() });
        }

        [Guid("824e16f4-6bf0-40ed-b003-b39fcd859f5f")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}
