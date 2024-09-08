using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MAUI_AI_Assistant
{
    public class ImageCreator : BaseToolWindow<ImageCreator>
    {
        public override string GetTitle(int toolWindowId) => "Image Creator";

        public override Type PaneType => typeof(Pane);

        public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
        {
            return Task.FromResult<FrameworkElement>(new ImageCreatorControl { DataContext = new ImageCreatorViewModel() });
        }

        [Guid("5aa00873-5c75-4af7-a2e6-d6f192ba708e")]
        internal class Pane : ToolkitToolWindowPane
        {
            public Pane()
            {
                BitmapImageMoniker = KnownMonikers.ToolWindow;
            }
        }
    }
}