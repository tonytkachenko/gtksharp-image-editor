using Gdk;
using Gtk;
using System.Reflection;

class GraphicEditor : Gtk.Window
{
    private Image image;
    private double zoomFactor = 1.0;
    private Pixbuf originalPixbuf;
    private ScrolledWindow scrolledWindow;
    private List<ToggleButton> buttons = [];

    public GraphicEditor() : base("Простой графический редактор")
    {
        SetDefaultSize(800, 600);
        SetPosition(WindowPosition.Center);
        DeleteEvent += delegate { Application.Quit(); };

        originalPixbuf = new Pixbuf("main-edit-img.png");
        image = new Image(originalPixbuf);

        CssProvider cssProvider = new();
        cssProvider.LoadFromData(@"
            window {
                background-color: #232323;
            }

            #menuBar {
                background-color: #1B1B1B;
                color: #fff;
            }

            #toolbox {
                border-right: 1px solid #303030;
            }

            #bottomPanel {
                border-top: 1px solid #303030;
            }

            #rightPanel {
                border-left: 1px solid #303030;
            }

            .toggle-button {
                background-image: none;
                background-color: transparent;
                color: white;
                border: none;
                padding: 0px;
                margin: 0px;
                border-radius: 0px;
                box-shadow: none;
                text-shadow: none;
                transition-duration: 0.2s;
            }

            .toggle-button:hover {
                background-color: #1E1E1E;
            }

            .toggle-button:checked {
                background-color: #0085FF;
            }

        ");
        StyleContext.AddProviderForScreen(Gdk.Screen.Default, cssProvider, 800);

        MenuBar menuBar = [
            new MenuItem("File"),
            new MenuItem("Edit"),
            new MenuItem("About")
        ];

        menuBar.Name = "menuBar";

        Box toolBox = new(Orientation.Vertical, 0);
        toolBox.Name = "toolbox";
        AddToolButton(toolBox, "SimpleImageEditor.Icons.move.png");
        AddToolButton(toolBox, "SimpleImageEditor.Icons.crop.png");
        AddToolButton(toolBox, "SimpleImageEditor.Icons.pen.png");
        AddToolButton(toolBox, "SimpleImageEditor.Icons.line.png");
        AddToolButton(toolBox, "SimpleImageEditor.Icons.shape.png");

        Viewport viewport = [image];
        scrolledWindow = [viewport];

        viewport.ScrollEvent += OnScrollEvent;

        Box rightPanel = new(Orientation.Vertical, 2);
        rightPanel.SetSizeRequest(200, -1);
        rightPanel.Name = "rightPanel";

        Box bottomPanel = new(Orientation.Horizontal, 2);
        bottomPanel.SetSizeRequest(-1, 40);
        bottomPanel.Name = "bottomPanel";

        Box contentBox = new(Orientation.Horizontal, 2);
        contentBox.PackStart(toolBox, false, false, 0);
        contentBox.PackStart(scrolledWindow, true, true, 0);
        contentBox.PackStart(rightPanel, false, false, 0);

        Box mainBox = new(Orientation.Vertical, 2);
        mainBox.PackStart(menuBar, false, false, 0);
        mainBox.PackStart(contentBox, true, true, 0);
        mainBox.PackStart(bottomPanel, false, false, 0);

        Add(mainBox);
        ShowAll();
    }

    private void AddToolButton(Box toolBox, string resourcePath)
    {
        var iconPixbuf = LoadResourcePixbuf(resourcePath);
        ToggleButton button = [new Image(iconPixbuf)];
        button.StyleContext.AddClass("toggle-button");
        button.SetSizeRequest(40, 40);
        button.Toggled += ToggleButton_Toggled;
        buttons.Add(button);
        toolBox.PackStart(button, false, false, 0);
    }

    private static Pixbuf LoadResourcePixbuf(string resourcePath)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourcePath);
        return new Pixbuf(stream);
    }

    private void OnScrollEvent(object sender, ScrollEventArgs args)
    {
        if (args.Event.State.HasFlag(ModifierType.ControlMask))
        {
            HorizontalScroll(args.Event.DeltaY);
            args.Event.DeltaY = 0;
        }
        else if (args.Event.State.HasFlag(ModifierType.Mod1Mask)) // Alt key
        {
            ZoomImage(args.Event.DeltaY);
            args.Event.DeltaY = 0;
        }
    }

    private void HorizontalScroll(double deltaX)
    {
        var adjustment = scrolledWindow.Hadjustment;
        adjustment.Value += deltaX * adjustment.StepIncrement;
    }

    private void ZoomImage(double deltaY)
    {
        double zoomStep = deltaY > 0 ? 0.1 : -0.1;
        zoomFactor += zoomStep;
        if (zoomFactor < 0.1) zoomFactor = 0.1;

        int newWidth = (int)(originalPixbuf.Width * zoomFactor);
        int newHeight = (int)(originalPixbuf.Height * zoomFactor);
        Pixbuf scaledPixbuf = originalPixbuf.ScaleSimple(newWidth, newHeight, Gdk.InterpType.Bilinear);
        image.Pixbuf = scaledPixbuf;
    }

    private void ToggleButton_Toggled(object sender, EventArgs e)
    {
        if (sender is not ToggleButton activeButton || !activeButton.Active)
        {
            return;
        }

        foreach (ToggleButton button in buttons)
        {
            if (button != activeButton)
            {
                button.Active = false;
            }
        }
        
    }

    static void Main()
    {
        Application.Init();
        _ = new GraphicEditor();
        Application.Run();
    }
}
