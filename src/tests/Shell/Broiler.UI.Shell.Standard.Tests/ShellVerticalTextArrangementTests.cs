using Broiler.Graphics;
using Broiler.Graphics.Color;
using Broiler.Graphics.Geometry;
using Broiler.Graphics.RenderList;
using Broiler.Graphics.Rendering;
using Broiler.Graphics.Text;
using Broiler.UI.AboutDialog.Standard;
using Broiler.UI.FileDialog.Standard;
using Broiler.UI.FontDialog.Standard;
using Broiler.UI.PrintDialog.Standard;
using Broiler.UI.ScanDialog.Standard;
using Broiler.UI.Window.Standard;

namespace Broiler.UI.Standard.Tests;

public sealed class ShellVerticalTextArrangementTests
{
    [Theory]
    [InlineData("About", 30)]
    [InlineData("File", 34)]
    [InlineData("File", 50)]
    [InlineData("Font", 30)]
    [InlineData("Font", 50)]
    [InlineData("Print", 30)]
    [InlineData("Print", 50)]
    [InlineData("Scan", 30)]
    [InlineData("Scan", 50)]
    public void Every_Shell_Dialog_Title_Is_Centered_In_Its_Title_Bar(string kind, double height)
    {
        using var session = new StandardUiSessionBuilder().Build(new Host(new BSize(900, 700)));
        var owner = new StandardWindow();
        session.AddRoot(owner);
        const string title = "Title with gy";
        UiElement dialog;
        BRect titleBounds;
        var bounds = new BRect(20, 25, 800, 600);
        switch (kind)
        {
            case "About":
                var about = new StandardAboutDialog { Title = title };
                _ = about.ShowModal(owner, bounds);
                dialog = about;
                session.RenderFrame();
                titleBounds = about.ChromeLayout.Title;
                break;
            case "File":
                var file = new StandardFileDialog { Title = title, TitleBarHeight = height };
                _ = file.ShowModal(owner, bounds);
                dialog = file;
                titleBounds = new(bounds.Left, bounds.Top, bounds.Width, height);
                break;
            case "Font":
                var font = new StandardFontDialog { Title = title, TitleBarHeight = height };
                _ = font.ShowModal(owner, bounds);
                dialog = font;
                titleBounds = new(bounds.Left, bounds.Top, bounds.Width, height);
                break;
            case "Scan":
                var scan = new StandardScanDialog { Title = title, TitleBarHeight = height };
                _ = scan.ShowModal(owner, bounds);
                dialog = scan;
                titleBounds = new(bounds.Left, bounds.Top, bounds.Width, height);
                break;
            default:
                var print = new StandardPrintDialog { Title = title, TitleBarHeight = height };
                _ = print.ShowModal(owner, bounds);
                dialog = print;
                titleBounds = new(bounds.Left, bounds.Top, bounds.Width, height);
                break;
        }
        var command = Assert.Single(session.RenderFrame().Commands.OfType<BRenderCommand.DrawText>(), c => c.Text.Text == title);
        AssertCentered(command, titleBounds);
        dialog.Dispose();
    }

    private static void AssertCentered(BRenderCommand.DrawText command, BRect bounds)
    {
        Assert.NotNull(command.Text.Baseline);
        var list = new BRenderList();
        list.DrawText(new BTextRun(command.Text.Text, command.Text.Font, BColor.Black) { Baseline = command.Text.Baseline },
            new BPoint(8, command.Origin.Y - bounds.Top + 4));
        using var renderer = new BImageRenderer();
        using var bitmap = renderer.RenderToImage(list,
            BSurfaceDescriptor.Default(new BSize(Math.Ceiling(BTextMeasurer.MeasureAdvance(command.Text.Text, command.Text.Font)) + 16, Math.Ceiling(bounds.Height) + 8)),
            new BFrameContext(BColor.White));
        var rows = Enumerable.Range(0, bitmap.Height)
            .Where(y => Enumerable.Range(0, bitmap.Width).Any(x => bitmap.GetPixel(x, y).R < 128)).ToArray();
        Assert.NotEmpty(rows);
        double center = (rows[0] + rows[^1] + 1) / 2.0;
        Assert.InRange(Math.Abs(center - (4 + bounds.Height / 2)), 0, 1);
        Assert.True(rows[0] >= 4 && rows[^1] < 4 + bounds.Height);
    }

    private sealed class Host(BSize size, double scale = 1) : IUiHost
    {
        public BSize ViewportSize => size;
        public double Scale => scale;
        public BRenderList CreateRenderList(int capacity = 0) => new(capacity);
        public void Invalidate(UiInvalidation invalidation) { }
        public void Present(BRenderList renderList) { }
    }
}
