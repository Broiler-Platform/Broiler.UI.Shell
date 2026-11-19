using System.Runtime.Versioning;
using Broiler.Graphics.Color;
using Broiler.Graphics.Geometry;
using Broiler.Graphics.RenderList;
using Broiler.Graphics.Rendering;
using Broiler.Graphics.Text;
using Broiler.Graphics.Windows;
using Broiler.UI.AboutDialog.Standard;
using Broiler.UI.Standard;
using Broiler.UI.Window.Standard;

namespace Broiler.UI.Standard.Windows.Tests;

[SupportedOSPlatform("windows")]
public sealed class ShellNativeCaptionAlignmentTests
{
    public static bool IsWindows => OperatingSystem.IsWindows();

    [Theory(Skip = "Requires DirectWrite on Windows.", SkipUnless = nameof(IsWindows))]
    [InlineData(1)]
    [InlineData(1.25)]
    [InlineData(1.5)]
    [InlineData(2)]
    public void About_Dialog_Captions_Are_Visually_Centered(double scale)
    {
        using var renderer = new Direct2DRenderer();
        using var session = new StandardUiSessionBuilder().Build(new Host(new BSize(620, 416), scale));
        using var owner = new StandardWindow();
        session.AddRoot(owner);
        using var dialog = new StandardAboutDialog { Title = "About", ProductName = "Broiler.UI Control Gallery" };
        _ = dialog.ShowModal(owner, new BRect(0, 0, dialog.PreferredSize.Width, dialog.PreferredSize.Height));
        var commands = session.RenderFrame().Commands.OfType<BRenderCommand.DrawText>().ToArray();
        AssertCentered(renderer, Assert.Single(commands, c => c.Text.Text == "About"), dialog.ChromeLayout.Title, scale);
        AssertCentered(renderer, Assert.Single(commands, c => c.Text.Text == "OK"), dialog.OkButton.Bounds, scale);
        var list = dialog.ComponentList;
        Assert.Equal(8 * list.EffectiveItemHeight, list.Bounds.Height, 8);
        var row = new BRect(list.ContentBounds.Left, list.ContentBounds.Top, list.ContentBounds.Width, list.EffectiveItemHeight);
        AssertCentered(renderer, commands.First(c => c.Origin.Y >= row.Top && c.Origin.Y < row.Bottom), row, scale, optical: true);
    }

    private static void AssertCentered(Direct2DRenderer renderer, BRenderCommand.DrawText command, BRect bounds, double scale, bool optical = false)
    {
        var list = new BRenderList();
        list.DrawText(new BTextRun(command.Text.Text, command.Text.Font, BColor.Black) { Baseline = command.Text.Baseline }, command.Origin);
        using var bitmap = renderer.RenderToImage(list,
            new BSurfaceDescriptor(new BSize(Math.Ceiling(Math.Max(bounds.Right, command.Origin.X + BTextMeasurer.MeasureAdvance(command.Text.Text, command.Text.Font))) + 16, Math.Ceiling(bounds.Bottom) + 8), scale),
            new BFrameContext(BColor.White));
        var rows = Enumerable.Range(0, bitmap.Height)
            .Where(y => Enumerable.Range(0, bitmap.Width).Any(x => bitmap.GetPixel(x, y).R < 128)).ToArray();
        Assert.NotEmpty(rows);
        double inkCenter = (rows[0] + rows[^1] + 1) / 2.0;
        if (optical)
        {
            double coverage = 0, moment = 0;
            for (int y = 0; y < bitmap.Height; y++)
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    double weight = 255 - (pixel.R + pixel.G + pixel.B) / 3.0;
                    coverage += weight;
                    moment += weight * (y + 0.5);
                }
            inkCenter = moment / coverage;
        }
        double target = (bounds.Top + bounds.Height / 2) * scale;
        Assert.True(Math.Abs(inkCenter - target) <= 1,
            $"'{command.Text.Text}' at {scale * 100}%: ink center {inkCenter}, row center {target}.");
    }

    private sealed class Host(BSize size, double scale) : IUiHost
    {
        public BSize ViewportSize => size;
        public double Scale => scale;
        public BRenderList CreateRenderList(int capacity = 0) => new(capacity);
        public void Invalidate(UiInvalidation invalidation) { }
        public void Present(BRenderList renderList) { }
    }
}
