using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using DiddyFlow.Helpers;
using DiddyFlow.Models;
using DiddyFlow.Services;

namespace DiddyFlow;

public partial class FloatingOrbWindow : Window
{
    private readonly SettingsService _settings;
    private readonly DispatcherTimer _pulseTimer;
    private readonly ScaleTransform _orbScale = new(1, 1, 43, 43);
    private DictationState _state = DictationState.Idle;
    private double _phase;
    private bool _pressed;
    private bool _dragging;
    private Point _dragOrigin;
    private double _originLeft;
    private double _originTop;
    private double _level;

    public FloatingOrbWindow(SettingsService settings)
    {
        _settings = settings;
        InitializeComponent();
        OrbBorder.RenderTransform = _orbScale;
        SourceInitialized += OnSourceInitialized;
        Loaded += OnLoaded;
        MouseLeftButtonDown += OnMouseLeftButtonDown;
        MouseMove += OnMouseMove;
        MouseLeftButtonUp += OnMouseLeftButtonUp;
        MouseRightButtonUp += (_, _) => OpenRequested?.Invoke(this, EventArgs.Empty);

        _pulseTimer = new DispatcherTimer(DispatcherPriority.Render)
        {
            Interval = TimeSpan.FromMilliseconds(33)
        };
        _pulseTimer.Tick += OnPulseTick;
    }

    public event EventHandler? ToggleRequested;
    public event EventHandler? OpenRequested;

    public void ApplyBranding(AppSettings settings)
    {
        var logo = ImageLoader.Load(settings.LogoPath, 180);
        if (logo is not null)
        {
            OrbLogo.Source = logo;
            OrbLogo.Visibility = Visibility.Visible;
            MicGlyph.Visibility = Visibility.Collapsed;
        }
        else
        {
            OrbLogo.Source = null;
            OrbLogo.Visibility = Visibility.Collapsed;
            MicGlyph.Visibility = Visibility.Visible;
        }
    }

    public void SetLevel(float level)
    {
        _level = Math.Clamp(level, 0, 1);
    }

    public void SetState(DictationState state)
    {
        _state = state;
        var accent = Brush("AccentBrush");
        var danger = Brush("DangerBrush");
        var success = Brush("SuccessBrush");

        StatusDot.Fill = state switch
        {
            DictationState.Recording => danger,
            DictationState.Transcribing => accent,
            _ => success
        };
        StatusText.Text = state switch
        {
            DictationState.Recording => "LISTENING",
            DictationState.Transcribing => "DIDDY IS TYPING",
            _ => "DIDDY FLOW"
        };
        RingOne.Stroke = state == DictationState.Recording ? danger : accent;
        RingTwo.Stroke = state == DictationState.Recording ? danger : accent;
        OrbGlow.Color = state == DictationState.Recording ? Color.FromRgb(255, 77, 77) : Color.FromRgb(242, 193, 78);

        if (state == DictationState.Transcribing)
        {
            var spin = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1.1))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            MicGlyph.RenderTransformOrigin = new Point(0.5, 0.5);
            MicGlyph.RenderTransform = new RotateTransform();
            MicGlyph.RenderTransform.BeginAnimation(RotateTransform.AngleProperty, spin);
        }
        else
        {
            MicGlyph.RenderTransform = Transform.Identity;
        }
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        NativeMethods.MakeNoActivate(handle);
        var source = HwndSource.FromHwnd(handle);
        source?.AddHook((IntPtr _, int msg, IntPtr _, IntPtr _, ref bool handled) =>
        {
            if (msg == NativeMethods.WmMouseActivate)
            {
                handled = true;
                return new IntPtr(NativeMethods.MaNoActivate);
            }

            return IntPtr.Zero;
        });
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var settings = _settings.Settings;
        var workArea = SystemParameters.WorkArea;
        if (settings.OrbLeft > -90000 && settings.OrbTop > -90000
            && settings.OrbLeft < workArea.Right && settings.OrbTop < workArea.Bottom)
        {
            Left = settings.OrbLeft;
            Top = settings.OrbTop;
        }
        else
        {
            Left = workArea.Right - Width - 26;
            Top = workArea.Bottom - Height - 26;
        }

        ApplyBranding(settings);
        _pulseTimer.Start();
    }

    private void OnPulseTick(object? sender, EventArgs e)
    {
        _phase += _state == DictationState.Recording ? 0.24 : 0.08;
        var pulse = (Math.Sin(_phase) + 1) / 2;
        RingOne.Opacity = 0.22 + (pulse * 0.55);
        RingTwo.Opacity = 0.5 - (pulse * 0.4);

        var targetScale = 1 + (_state == DictationState.Recording ? pulse * 0.07 : pulse * 0.025) + (_level * 0.05);
        _orbScale.ScaleX += (targetScale - _orbScale.ScaleX) * 0.25;
        _orbScale.ScaleY += (targetScale - _orbScale.ScaleY) * 0.25;
        OrbGlow.BlurRadius = 22 + (pulse * 10) + (_level * 14);
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _pressed = true;
        _dragging = false;
        _dragOrigin = e.GetPosition(this);
        _originLeft = Left;
        _originTop = Top;
        CaptureMouse();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_pressed || !IsMouseCaptured)
        {
            return;
        }

        var position = e.GetPosition(this);
        var dx = position.X - _dragOrigin.X;
        var dy = position.Y - _dragOrigin.Y;
        if (!_dragging && (Math.Abs(dx) > 5 || Math.Abs(dy) > 5))
        {
            _dragging = true;
        }

        if (_dragging)
        {
            Left = _originLeft + dx;
            Top = _originTop + dy;
        }
    }

    private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();
        }

        var wasDragging = _dragging;
        _pressed = false;
        _dragging = false;

        if (wasDragging)
        {
            _settings.Settings.OrbLeft = Left;
            _settings.Settings.OrbTop = Top;
            _settings.Save();
            return;
        }

        ToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private Brush Brush(string key)
    {
        return TryFindResource(key) as Brush ?? Brushes.Goldenrod;
    }
}
