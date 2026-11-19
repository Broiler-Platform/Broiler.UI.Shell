using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Broiler.Graphics;
using Broiler.Graphics.Color;
using Broiler.Graphics.Geometry;
using Broiler.Graphics.Imaging;
using Broiler.Graphics.Paging;
using Broiler.Graphics.RenderList;
using Broiler.Graphics.Rendering;
using Broiler.Graphics.Text;
using Broiler.Graphics.Windowing;
using Broiler.Graphics.Windows;
using Broiler.Input.Keyboard;
using Broiler.UI;
using Broiler.UI.AboutDialog.Standard;
using Broiler.UI.Button.Standard;
using Broiler.UI.ColorDialog.Standard;
using Broiler.UI.FileDialog;
using Broiler.UI.FileDialog.Standard;
using Broiler.UI.FontDialog.Standard;
using Broiler.UI.Label;
using Broiler.UI.Label.Standard;
using Broiler.UI.Panel;
using Broiler.UI.Panel.Standard;
using Broiler.UI.PrintDialog.Standard;
using Broiler.UI.ScanDialog.Standard;
using Broiler.UI.Standard;
using Broiler.UI.Window;
using Broiler.UI.Window.Standard;

namespace Broiler.UI.Shell.Win32.Demo;

[SupportedOSPlatform("windows7.0")]
internal sealed class ShellDemoWindow : Direct2DWindow
{
    private readonly DemoHost _host;
    private readonly StandardQueuedUiDispatcher _dispatcher;
    private readonly UiSession _session;
    private readonly StandardWindow _rootWindow;
    private readonly StandardLabel _status;

#pragma warning disable CS0618
    private readonly StandardLegacyGraphicsInputAdapter _legacyInput = new("broiler-ui-shell-win32-demo");
#pragma warning restore CS0618

    public ShellDemoWindow()
        : base(new BWindowOptions
        {
            Title = "Broiler.UI.Shell Win32 Demo",
            ClientWidth = 920,
            ClientHeight = 620,
            ClearColor = Palette.Canvas,
            RenderOptions = new BRenderOptions(Antialias: true, VSync: true, SubpixelText: true),
        })
    {
        _host = new DemoHost(this);
        _dispatcher = new StandardQueuedUiDispatcher(() => PostToUiThread(DrainDispatcher));
        _session = new StandardUiSessionBuilder()
            .WithDispatcher(_dispatcher)
            .Build(_host);

        _status = new StandardLabel
        {
            Text = "Ready. Launch any Broiler.UI.Shell dialog below.",
            Font = new BFontStyle("Segoe UI", 14),
            Foreground = Palette.StatusText,
            Wrapping = UiTextWrapping.Wrap,
        };

        _rootWindow = BuildTree();
        _session.AddRoot(_rootWindow);
    }

    protected override BRenderList? BuildRenderList(BSize clientSize)
    {
        _host.Update(clientSize, DpiScale);
        DrainDispatcher();
        return _session.RenderFrame();
    }

    private void DrainDispatcher()
    {
        try
        {
            _dispatcher.Drain();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    protected override void OnResized(BSize clientSize, double dpiScale)
    {
        _host.Update(clientSize, dpiScale);
        Invalidate();
    }

    protected override void OnPointerDown(BPointerEventArgs e) => Dispatch(_legacyInput.FromPointerButton(e));
    protected override void OnPointerMove(BPointerEventArgs e) => Dispatch(_legacyInput.FromPointerMove(e));
    protected override void OnPointerUp(BPointerEventArgs e) => Dispatch(_legacyInput.FromPointerButton(e));
    protected override void OnMouseWheel(BMouseWheelEventArgs e) => Dispatch(_legacyInput.FromMouseWheel(e));
    protected override void OnKeyDown(BKeyEventArgs e) => Dispatch(_legacyInput.FromKey(e, KeyboardKeyTransition.Down));
    protected override void OnKeyUp(BKeyEventArgs e) => Dispatch(_legacyInput.FromKey(e, KeyboardKeyTransition.Up));
    protected override void OnTextInput(BTextInputEventArgs e) => Dispatch(_legacyInput.FromText(e));

    private void Dispatch(UiInputEvent inputEvent)
    {
        if (_session.DispatchInput(inputEvent))
            Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _session.Dispose();

        base.Dispose(disposing);
    }

    private StandardWindow BuildTree()
    {
        var title = new StandardLabel
        {
            Text = "Broiler.UI.Shell Win32 Demo",
            Font = new BFontStyle("Segoe UI", 24, BFontWeight.Bold),
            Foreground = Palette.Title,
        };

        var subtitle = new StandardLabel
        {
            Text = "Combines Broiler.UI (Core) with Broiler.UI.Shell dialogs: About, Color, Open/Save File, Font, Print, and Scan.",
            Font = new BFontStyle("Segoe UI", 14),
            Foreground = Palette.Muted,
            Wrapping = UiTextWrapping.Wrap,
        };

        var row1 = Row(
            Button("About Dialog", () => ShowShellDialog(new StandardAboutDialog { ProductName = "Broiler.UI.Shell Demo" })),
            Button("Color Dialog", () => ShowShellDialog(new StandardColorDialog { Title = "Choose Color" })),
            Button("Open File", () => ShowFileDialog(UiFileDialogMode.Open)),
            Button("Save File", () => ShowFileDialog(UiFileDialogMode.Save)));

        var row2 = Row(
            Button("Font Dialog", () => ShowShellDialog(new StandardFontDialog { Title = "Choose Font" })),
            Button("Print Dialog", () => ShowShellDialog(new StandardPrintDialog { PrintHost = _host, Document = new SampleDocument() })),
            Button("Scan Dialog", () => ShowShellDialog(new StandardScanDialog { ScanHost = _host })));

        var body = new StandardPanel
        {
            LayoutMode = UiPanelLayoutMode.Stack,
            StackOrientation = UiStackOrientation.Vertical,
            Spacing = 16,
            Background = Palette.Canvas,
        };
        body.AddChild(title);
        body.AddChild(subtitle);
        body.AddChild(row1);
        body.AddChild(row2);
        body.AddChild(_status);

        var window = new StandardWindow
        {
            Title = "Broiler.UI.Shell Win32 Demo",
            Background = Palette.Canvas,
            BorderColor = Palette.Divider,
            BorderThickness = 1,
        };
        window.AddChild(body);
        return window;
    }

    private static StandardPanel Row(params StandardButton[] buttons)
    {
        var row = new StandardPanel
        {
            LayoutMode = UiPanelLayoutMode.Stack,
            StackOrientation = UiStackOrientation.Horizontal,
            Spacing = 10,
            Background = BColor.Transparent,
        };
        foreach (StandardButton button in buttons)
            row.AddChild(button);
        return row;
    }

    private static StandardButton Button(string text, Action onClick)
    {
        var button = new StandardButton
        {
            Text = text,
            PreferredSize = new BSize(150, 36),
            Font = new BFontStyle("Segoe UI", 14),
        };
        button.Clicked += (_, _) => onClick();
        return button;
    }

    private void ShowFileDialog(UiFileDialogMode mode)
    {
        var dialog = new StandardFileDialog
        {
            Title = mode == UiFileDialogMode.Open ? "Open File" : "Save File",
            BreakOutMode = UiWindowBreakOutMode.Manual,
        };
        dialog.ResultCompleted += (_, e) => SetStatus($"{dialog.Title}: {e.Result.Kind} ({e.Result.Value ?? "none"})");
        BSize viewport = _host.ViewportSize;
        dialog.Measure(viewport);
        double width = Math.Min(Math.Max(320, dialog.DesiredSize.Width), Math.Max(0, viewport.Width - 32));
        double height = Math.Min(Math.Max(220, dialog.DesiredSize.Height), Math.Max(0, viewport.Height - 32));
        var placement = new BRect(Math.Max(0, (viewport.Width - width) / 2), Math.Max(0, (viewport.Height - height) / 2), width, height);
        _ = mode == UiFileDialogMode.Open
            ? dialog.ShowOpenModal(_rootWindow, placement)
            : dialog.ShowSaveModal(_rootWindow, placement);
        Invalidate();
    }

    private void ShowShellDialog(Dialog.UiDialog dialog)
    {
        if (_rootWindow.IsDisposed || _rootWindow.IsClosed)
            return;

        dialog.BreakOutMode = UiWindowBreakOutMode.Manual;
        dialog.ResultCompleted += (_, e) => SetStatus($"{dialog.Title}: {e.Result.Kind} ({e.Result.Value ?? "none"})");

        BSize viewport = _host.ViewportSize;
        dialog.Measure(viewport);
        double width = Math.Min(Math.Max(320, dialog.DesiredSize.Width), Math.Max(0, viewport.Width - 32));
        double height = Math.Min(Math.Max(220, dialog.DesiredSize.Height), Math.Max(0, viewport.Height - 32));
        _ = dialog.ShowModal(_rootWindow, new BRect(
            Math.Max(0, (viewport.Width - width) / 2),
            Math.Max(0, (viewport.Height - height) / 2),
            width,
            height));
        Invalidate();
    }

    private void SetStatus(string text)
    {
        _status.Text = text;
        Invalidate();
    }

    private sealed class SampleDocument : IBPagedDocument
    {
        public string Title => "Shell Sample Document";
        public BPageSetup PreferredSetup => BPageSetup.Default;

        public IBPageSource Paginate(BPageSetup setup, CancellationToken cancellationToken = default)
        {
            var builder = new BPageBuilder(setup.PageSize);
            builder.Content.DrawText(new BTextRun("Broiler.UI.Shell Print Preview", new BFontStyle("Segoe UI", 16), BColor.Black),
                new BPoint(setup.Margins.Left, setup.Margins.Top));
            return new BPageCollection([builder.Build()]);
        }
    }

    private sealed class DemoHost(ShellDemoWindow window) : IUiHost, IUiPrintHost, IUiScanHost
    {
        public BSize ViewportSize { get; private set; } = new(920, 620);
        public double Scale { get; private set; } = 1;
        public bool ShowsSystemPrintDialog => false;
        public bool ShowsSystemScanDialog => false;

        public BRenderList CreateRenderList(int capacity = 0) => new(capacity);
        public void Invalidate(UiInvalidation invalidation) => window.Invalidate();
        public void Present(BRenderList renderList) { }

        public IReadOnlyList<UiPrinterInfo> GetPrinters() =>
            [new UiPrinterInfo("Sample PDF Printer", IsDefault: true)];

        UiPrinterCapabilities IUiPrintHost.GetCapabilities(string printerName) =>
            new() { SupportsColor = true, SupportsDuplex = true };

        public Task<UiPrintResult> PrintAsync(UiPrintJob job, CancellationToken cancellationToken = default) =>
            Task.FromResult(UiPrintResult.Printed(job.ResolvePages(cancellationToken).PageCount));

        public IReadOnlyList<UiScannerInfo> GetScanners() =>
            [new UiScannerInfo("Sample Flatbed Scanner", IsDefault: true)];

        UiScannerCapabilities IUiScanHost.GetCapabilities(string scannerName) =>
            UiScannerCapabilities.Unknown;

        public Task<UiScanResult> ScanAsync(UiScanJob job, CancellationToken cancellationToken = default)
        {
            var pixels = new BPixelBuffer(300, 400, new byte[300 * 400 * 4]);
            return Task.FromResult(UiScanResult.Scanned([pixels], job.Settings.Dpi, job.Settings.PageSetup.PageSize));
        }

        public void Update(BSize viewportSize, double scale)
        {
            ViewportSize = viewportSize;
            Scale = scale;
        }
    }

    private static class Palette
    {
        public static readonly BColor Canvas = BColor.FromArgb(0xFF, 0xF6, 0xF8, 0xFB);
        public static readonly BColor Title = BColor.FromArgb(0xFF, 0x14, 0x2A, 0x40);
        public static readonly BColor Muted = BColor.FromArgb(0xFF, 0x5B, 0x6B, 0x82);
        public static readonly BColor Divider = BColor.FromArgb(0xFF, 0xDD, 0xE4, 0xEE);
        public static readonly BColor StatusText = BColor.FromArgb(0xFF, 0x33, 0x42, 0x55);
    }
}
