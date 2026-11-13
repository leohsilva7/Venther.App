using System;
using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Venther.App.Helpers;
using Venther.App.Models;

namespace Venther.App.Controls;

public partial class WorkOrderCard : ContentView
{
    // ── Bindable properties ──────────────────────────────────────────────────

    public static readonly BindableProperty WorkOrderProperty =
        BindableProperty.Create(
            nameof(WorkOrder),
            typeof(WorkOrder),
            typeof(WorkOrderCard),
            default(WorkOrder),
            propertyChanged: (b, _, _) => ((WorkOrderCard)b).UpdateVisuals());

    public static readonly BindableProperty ActionCommandProperty =
        BindableProperty.Create(
            nameof(ActionCommand),
            typeof(ICommand),
            typeof(WorkOrderCard),
            default(ICommand));

    public WorkOrder? WorkOrder
    {
        get => (WorkOrder?)GetValue(WorkOrderProperty);
        set => SetValue(WorkOrderProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    // ── Constructor & visuals ────────────────────────────────────────────────

    public WorkOrderCard()
    {
        InitializeComponent();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (WorkOrder == null) return;

        switch (WorkOrder.Status)
        {
            case WorkOrderStatus.InProgress:
                ShowInProgress();
                break;

            case WorkOrderStatus.Done:
                ShowScheduled(isDone: true);
                break;

            default:
                ShowScheduled(isDone: false);
                break;
        }
    }

    // ── InProgress variant ───────────────────────────────────────────────────

    private void ShowInProgress()
    {
        InProgressLayout.IsVisible = true;
        ScheduledLayout.IsVisible = false;

        var wo = WorkOrder!;
        StatusBar.Color = GetThemeColor("Accent", Color.FromArgb("#FC6100"));

        InProgressIdLabel.Text = wo.Id;
        InProgressTitleLabel.Text = wo.Title;
        InProgressDeviceLabel.Text = wo.DeviceName;
        InProgressLocationLabel.Text = wo.Location;
        TelemetryLabel.Text = wo.TelemetrySnippet ?? string.Empty;
        TechnicianLabel.Text = wo.TechnicianName ?? string.Empty;

        TelemetryBorder.IsVisible = !string.IsNullOrWhiteSpace(wo.TelemetrySnippet);

        StartPulseAnimation();
    }

    private async void StartPulseAnimation()
    {
        // Simple scale pulse on the dot to suggest "live" state
        while (InProgressLayout.IsVisible && WorkOrder?.Status == WorkOrderStatus.InProgress)
        {
            await PulseDot.ScaleTo(1.5, 600, Easing.SinInOut);
            await PulseDot.ScaleTo(1.0, 600, Easing.SinInOut);
        }
    }

    // ── Scheduled / Done variant ─────────────────────────────────────────────

    private void ShowScheduled(bool isDone)
    {
        InProgressLayout.IsVisible = false;
        ScheduledLayout.IsVisible = true;

        var wo = WorkOrder!;

        if (isDone)
        {
            // Green bar + "Concluída" badge
            StatusBar.Color = Color.FromArgb("#16A34A");

            ScheduledStatusBadge.BackgroundColor = Color.FromArgb("#DCFCE7");
            ScheduledStatusLabel.Text = "Concluída";
            ScheduledStatusLabel.TextColor = Color.FromArgb("#15803D");

            ScheduledActionButton.Text = "Ver detalhes";
            ScheduledActionButton.BackgroundColor = GetThemeColor("Surface", Color.FromArgb("#EEF4FC"));
            ScheduledActionButton.TextColor = GetThemeColor("OnSurface", Color.FromArgb("#161C22"));
        }
        else
        {
            // Outline / neutral bar + "Agendada" badge
            StatusBar.Color = GetThemeColor("Outline", Color.FromArgb("#737783"));

            ScheduledStatusBadge.BackgroundColor = GetThemeColor("SurfaceVariant", Color.FromArgb("#E8EEF6"));
            ScheduledStatusLabel.Text = "Agendada";
            ScheduledStatusLabel.TextColor = GetThemeColor("OnSurfaceVariant", Color.FromArgb("#424751"));

            ScheduledActionButton.Text = "Iniciar OS";
            ScheduledActionButton.BackgroundColor = GetThemeColor("Primary", Color.FromArgb("#00326F"));
            ScheduledActionButton.TextColor = GetThemeColor("White", Colors.White);
        }

        ScheduledIdLabel.Text = wo.Id;
        ScheduledTitleLabel.Text = wo.Title;
        ScheduledDeviceLabel.Text = wo.DeviceName;
        ScheduledLocationLabel.Text = wo.Location;
        ScheduledDateLabel.Text = wo.ScheduledDateLabel ?? wo.Date.ToString("dd/MM/yyyy");
    }

    // ── Event handlers ───────────────────────────────────────────────────────

    private async void OnCardTapped(object? sender, TappedEventArgs e)
        => await NavigateToDetail();

    private async void OnActionClicked(object? sender, EventArgs e)
        => await NavigateToDetail();

    private async System.Threading.Tasks.Task NavigateToDetail()
    {
        if (ActionCommand != null && ActionCommand.CanExecute(WorkOrder))
        {
            ActionCommand.Execute(WorkOrder);
            return;
        }

        if (WorkOrder != null)
        {
            await Shell.Current.GoToAsync(Routes.WorkOrderDetail, new Dictionary<string, object>
            {
                ["WorkOrderId"] = WorkOrder.Id,
                ["WorkOrder"] = WorkOrder
            });
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private Color GetThemeColor(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var val) == true && val is Color color)
            return color;
        return fallback;
    }
}
