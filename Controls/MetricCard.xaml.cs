namespace Venther.App.Controls;

public partial class MetricCard : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(MetricCard), string.Empty);
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(string), typeof(MetricCard), string.Empty);
    public static readonly BindableProperty UnitProperty =
        BindableProperty.Create(nameof(Unit), typeof(string), typeof(MetricCard), string.Empty);
    public static readonly BindableProperty CaptionProperty =
        BindableProperty.Create(nameof(Caption), typeof(string), typeof(MetricCard), string.Empty);
    public static readonly BindableProperty IconSourceProperty =
        BindableProperty.Create(nameof(IconSource), typeof(ImageSource), typeof(MetricCard));
    public static readonly BindableProperty TrendIconSourceProperty =
        BindableProperty.Create(nameof(TrendIconSource), typeof(ImageSource), typeof(MetricCard));
    public static readonly BindableProperty IsAlertProperty =
        BindableProperty.Create(nameof(IsAlert), typeof(bool), typeof(MetricCard), false);

    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public string Unit { get => (string)GetValue(UnitProperty); set => SetValue(UnitProperty, value); }
    public string Caption { get => (string)GetValue(CaptionProperty); set => SetValue(CaptionProperty, value); }
    public ImageSource? IconSource { get => (ImageSource?)GetValue(IconSourceProperty); set => SetValue(IconSourceProperty, value); }
    public ImageSource? TrendIconSource { get => (ImageSource?)GetValue(TrendIconSourceProperty); set => SetValue(TrendIconSourceProperty, value); }
    public bool IsAlert { get => (bool)GetValue(IsAlertProperty); set => SetValue(IsAlertProperty, value); }

    // Propriedades calculadas (estado normal x alerta)
    public string UnitText => string.IsNullOrEmpty(Unit) ? string.Empty : $" {Unit}";
    public Color IconColor => IsAlert ? Res("Accent") : Res("Primary");
    public Color ValueColor => IsAlert ? Res("Accent") : Res("OnSurface");
    public Color TrendColor => IsAlert ? Res("AccentText") : Res("SuccessText");

    public MetricCard() => InitializeComponent();

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(Unit))
            OnPropertyChanged(nameof(UnitText));
        else if (propertyName == nameof(IsAlert))
        {
            OnPropertyChanged(nameof(IconColor));
            OnPropertyChanged(nameof(ValueColor));
            OnPropertyChanged(nameof(TrendColor));
        }
    }

    private static Color Res(string key) =>
        Application.Current?.Resources.TryGetValue(key, out var v) == true && v is Color c ? c : Colors.Black;
}