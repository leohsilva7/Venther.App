using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Maui.Behaviors;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Venther.App.Helpers;
using Venther.App.Models;

namespace Venther.App.Controls;

public partial class AlertCard : ContentView
{
    public static readonly BindableProperty AlertProperty =
        BindableProperty.Create(
            nameof(Alert),
            typeof(AlertItem),
            typeof(AlertCard),
            default(AlertItem),
            propertyChanged: (b, _, _) => ((AlertCard)b).UpdateVisuals());

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(
            nameof(ActionCommand),
            typeof(ICommand),
            typeof(AlertCard),
            default(ICommand));

    public AlertItem? Alert
    {
        get => (AlertItem?)GetValue(AlertProperty);
        set => SetValue(AlertProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public AlertCard()
    {
        InitializeComponent();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (Alert == null || IconCircle == null || AlertIconImage == null || StatusBadgeBorder == null || StatusBadgeLabel == null || TimeLabel == null)
            return;

        Color iconTint;

        if (Alert.IsResolved)
        {
            var greenBg = Color.FromArgb("#DCFCE7");
            var greenText = Color.FromArgb("#15803D");
            iconTint = Color.FromArgb("#16A34A");

            IconCircle.BackgroundColor = greenBg;
            AlertIconImage.Source = "check_circle.svg";

            StatusBadgeBorder.BackgroundColor = greenBg;
            StatusBadgeLabel.TextColor = greenText;
            StatusBadgeLabel.Text = "Resolvido";
        }
        else if (Alert.Severity == AlertSeverity.Critical)
        {
            var errorBg = GetThemeColor("ErrorBg", Color.FromArgb("#FEF2F2"));
            var errorText = GetThemeColor("Error", Color.FromArgb("#BA1A1A"));
            iconTint = errorText;

            IconCircle.BackgroundColor = errorBg;
            AlertIconImage.Source = "alert_triangle.svg";

            StatusBadgeBorder.BackgroundColor = errorBg;
            StatusBadgeLabel.TextColor = errorText;
            StatusBadgeLabel.Text = "Crítico";
        }
        else
        {
            var accentContainer = GetThemeColor("AccentContainer", Color.FromArgb("#FFDBCD"));
            var accentText = GetThemeColor("AccentText", Color.FromArgb("#A43D00"));
            iconTint = GetThemeColor("Accent", Color.FromArgb("#FC6100"));

            IconCircle.BackgroundColor = accentContainer;
            AlertIconImage.Source = "alert_circle.svg";

            StatusBadgeBorder.BackgroundColor = accentContainer;
            StatusBadgeLabel.TextColor = accentText;
            StatusBadgeLabel.Text = "Aviso";
        }

        // Apply IconTintColorBehavior
        AlertIconImage.Behaviors.Clear();
        AlertIconImage.Behaviors.Add(new IconTintColorBehavior { TintColor = iconTint });

        TimeLabel.Text = FormatRelativeTime(Alert.Date);
    }

    private string FormatRelativeTime(DateTime date)
    {
        var span = DateTime.Now - date;
        if (span.TotalMinutes < 1)
            return "Agora";
        if (span.TotalMinutes < 60)
            return $"Há {(int)span.TotalMinutes} min";
        if (span.TotalHours < 24)
            return $"Há {(int)span.TotalHours}h";
        if (span.TotalDays < 30)
            return $"Há {(int)span.TotalDays}d";

        return date.ToString("dd/MM/yyyy");
    }

    private async void OnCardTapped(object? sender, EventArgs e)
    {
        if (ActionCommand != null && ActionCommand.CanExecute(Alert))
        {
            ActionCommand.Execute(Alert);
            return;
        }

        if (Alert != null)
        {
            await Shell.Current.GoToAsync(Routes.DeviceDetail, new Dictionary<string, object>
            {
                ["Alert"] = Alert,
                ["AlertId"] = Alert.Id
            });
        }
    }

    private void OnActionClicked(object? sender, EventArgs e)
    {
        OnCardTapped(sender, e);
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
