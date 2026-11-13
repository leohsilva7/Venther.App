using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Venther.App.Models;

namespace Venther.App.Controls;

public partial class StatusBadge : ContentView
{
    public static readonly BindableProperty StatusProperty =
        BindableProperty.Create(
            nameof(Status),
            typeof(DeviceStatus),
            typeof(StatusBadge),
            DeviceStatus.Normal,
            propertyChanged: (b, _, _) => ((StatusBadge)b).UpdateBadge());

    public DeviceStatus Status
    {
        get => (DeviceStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public StatusBadge()
    {
        InitializeComponent();
        UpdateBadge();
    }

    private void UpdateBadge()
    {
        switch (Status)
        {
            case DeviceStatus.Attention:
                BadgeBorder.BackgroundColor = GetThemeColor("AccentContainer", Color.FromArgb("#FFDBCD"));
                BadgeDot.Color = GetThemeColor("Accent", Color.FromArgb("#FC6100"));
                BadgeLabel.TextColor = GetThemeColor("AccentText", Color.FromArgb("#A43D00"));
                BadgeLabel.Text = "Atenção";
                break;

            case DeviceStatus.Critical:
                BadgeBorder.BackgroundColor = GetThemeColor("ErrorBg", Color.FromArgb("#FEF2F2"));
                BadgeDot.Color = GetThemeColor("Error", Color.FromArgb("#BA1A1A"));
                BadgeLabel.TextColor = GetThemeColor("Error", Color.FromArgb("#BA1A1A"));
                BadgeLabel.Text = "Crítico";
                break;

            case DeviceStatus.Normal:
            default:
                BadgeBorder.BackgroundColor = Color.FromArgb("#DCFCE7");
                BadgeDot.Color = Color.FromArgb("#16A34A");
                BadgeLabel.TextColor = Color.FromArgb("#15803D");
                BadgeLabel.Text = "Normal";
                break;
        }
    }

    private Color GetThemeColor(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var val) == true && val is Color color)
        {
            return color;
        }
        return fallback;
    }
}
