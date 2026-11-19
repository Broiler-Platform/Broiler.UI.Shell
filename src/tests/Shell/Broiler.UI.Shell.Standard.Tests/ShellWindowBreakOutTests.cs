using System;
using System.Collections.Generic;
using Broiler.Graphics.Geometry;
using Broiler.Graphics.Imaging;
using Broiler.Graphics.RenderList;
using Broiler.Input;
using Broiler.Input.Mouse;
using Broiler.UI.FileDialog.Standard;
using Broiler.UI.Window;
using Broiler.UI.Window.Standard;

namespace Broiler.UI.Standard.Tests;

public sealed class ShellWindowBreakOutTests
{
    [Fact]
    public void A_Broken_Out_Dialog_Moves_Through_The_Window_Manager()
    {
        var host = new FakeWindowHost();
        using UiSession session = new StandardUiSessionBuilder().Build(host);
        var owner = new StandardWindow();
        session.AddRoot(owner);

        var dialog = new StandardFileDialog { Title = "Open" };
        _ = dialog.ShowOpenModal(owner, new BRect(20, 20, 300, 200));
        Assert.True(dialog.IsBrokenOut);

        dialog.Measure(new BSize(300, 200));
        dialog.Arrange(new BRect(0, 0, 300, 200));
        BRect before = dialog.Placement;

        Assert.True(dialog.DispatchInput(CreateMouseDown(150, 17)));
        Assert.Equal(1, host.Created[0].MoveDrags);

        dialog.DispatchInput(CreateMouseMove(210, 92));
        Assert.Equal(before, dialog.Placement);
    }

    [Fact]
    public void A_Broken_Out_File_Dialog_Asks_For_A_Resizable_Host_Window()
    {
        var host = new FakeWindowHost();
        using UiSession session = new StandardUiSessionBuilder().Build(host);
        var owner = new StandardWindow();
        session.AddRoot(owner);

        var dialog = new StandardFileDialog { Title = "Open" };
        _ = dialog.ShowOpenModal(owner, new BRect(20, 20, 300, 200));

        Assert.True(host.Created[0].Request.Resizable);
        Assert.True(dialog.BeginResizeDrag(UiWindowEdge.BottomRight));
        Assert.Equal([UiWindowEdge.BottomRight], host.Created[0].ResizeDrags);
        Assert.False(dialog.CanMaximize);
    }

    [Fact]
    public void A_Logical_Dialog_Still_Moves_Itself_By_Placement()
    {
        var host = new FakeWindowHost();
        using UiSession session = new StandardUiSessionBuilder().Build(host);
        var owner = new StandardWindow();
        session.AddRoot(owner);

        var dialog = new StandardFileDialog { Title = "Open", BreakOutMode = UiWindowBreakOutMode.Manual };
        _ = dialog.ShowOpenModal(owner, new BRect(20, 20, 300, 200));
        Assert.False(dialog.IsBrokenOut);

        owner.Measure(new BSize(640, 480));
        owner.Arrange(new BRect(0, 0, 640, 480));

        dialog.DispatchInput(CreateMouseDown(150, 37));
        dialog.DispatchInput(CreateMouseMove(190, 77));

        Assert.Empty(host.Created);
        Assert.Equal(new BRect(60, 60, 300, 200), dialog.Placement);
    }

    private static UiInputEvent CreateMouseMove(double x, double y)
    {
        var header = new InputEventHeader(
            InputDeviceId.FromOpaqueValue("mouse:test"),
            new InputTimestamp(1, TimeSpan.TicksPerSecond, "test"),
            1);

        return UiInputEvent.FromMouseMove(new MouseMoveEvent(
            header,
            InputPoint.ClientDeviceIndependentPixels(x, y),
            MouseButtons.Left,
            InputEventSource.Synthetic));
    }

    private static UiInputEvent CreateMouseDown(double x, double y)
    {
        var header = new InputEventHeader(
            InputDeviceId.FromOpaqueValue("mouse:test"),
            new InputTimestamp(1, TimeSpan.TicksPerSecond, "test"),
            1);

        return UiInputEvent.FromMouseButton(new MouseButtonEvent(
            header,
            InputPoint.ClientDeviceIndependentPixels(x, y),
            MouseButtons.Left,
            MouseButton.Left,
            MouseButtonTransition.Down,
            InputEventSource.Synthetic));
    }

    internal sealed class FakeWindowHost : IUiHost, IUiWindowHost
    {
        public List<FakeHostWindow> Created { get; } = [];
        public UiHostWindowChrome? ChromeOverride { get; set; }
        public BSize ViewportSize => new(800, 600);
        public double Scale => 1.0;
        public BRenderList CreateRenderList(int capacity = 0) => new(capacity);
        public void Invalidate(UiInvalidation invalidation) { }
        public void Present(BRenderList renderList) { }

        public IUiHostWindow CreateHostWindow(UiHostWindowRequest request)
        {
            var window = new FakeHostWindow(request) { Chrome = ChromeOverride ?? request.Chrome };
            Created.Add(window);
            return window;
        }
    }

    internal sealed class FakeHostWindow(UiHostWindowRequest request) : IUiHostWindow, IUiWindowChromeHost
    {
        public UiHostWindowRequest Request { get; } = request;
        public string Title { get; private set; } = request.Title;
        public BRect Placement { get; } = request.Placement;
        public bool IsModal { get; } = request.IsModal;
        public UiSession? BoundSession { get; private set; }
        public bool IsDisposed { get; private set; }
        public BPixelBuffer? Icon { get; private set; }
        public List<UiWindowEdge> ResizeDrags { get; } = [];
        public int MoveDrags { get; private set; }
        public event EventHandler? CloseRequested;
        public event EventHandler? WindowStateChanged;
        public BSize ViewportSize => new(400, 300);
        public double Scale => 1.0;
        public UiHostWindowChrome Chrome { get; set; } = request.Chrome;
        public bool IsResizable { get; set; } = true;
        public UiHostWindowState WindowState { get; private set; }
        public BRenderList CreateRenderList(int capacity = 0) => new(capacity);
        public void Invalidate(UiInvalidation invalidation) { }
        public void Present(BRenderList renderList) { }
        public void Bind(UiSession session) => BoundSession = session;
        public void SetTitle(string title) => Title = title;
        public void SetIcon(BPixelBuffer? icon) => Icon = icon;
        public void Activate() { }
        public void SetWindowState(UiHostWindowState state)
        {
            if (WindowState == state)
                return;
            WindowState = state;
            WindowStateChanged?.Invoke(this, EventArgs.Empty);
        }
        public void RequestClose() => CloseRequested?.Invoke(this, EventArgs.Empty);
        public void BeginMoveDrag() => MoveDrags++;
        public void BeginResizeDrag(UiWindowEdge edge) => ResizeDrags.Add(edge);
        public void Dispose() => IsDisposed = true;
    }
}
