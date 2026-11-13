using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Venther.App.Controls;
using Venther.App.Helpers;
using Venther.App.Models;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public partial class AlertsViewModel : BaseViewModel
{
    private readonly IAlertService _alertService;
    private System.Collections.Generic.List<AlertItem> _allAlerts = new();

    [ObservableProperty]
    private ObservableCollection<AlertItem> _filteredAlerts = new();

    [ObservableProperty]
    private ObservableCollection<SegmentItem> _filterSegments = new();

    [ObservableProperty]
    private int _selectedSegmentIndex;

    [ObservableProperty]
    private int _activeAlertsCount;

    // TODO: No .NET MAUI Shell padrão não há propriedade nativa Tab.BadgeCount multiplataforma.
    // Para exibir o badge na TabBar no Android, implementar um ShellBottomNavViewAppearanceTracker
    // customizado utilizando bottomNavigationView.GetOrCreateBadge(tabIndex).Number = ActiveAlertsCount.

    public AlertsViewModel(IAlertService alertService)
    {
        _alertService = alertService;
        Title = "Alertas";
        InitSegments();
    }

    private void InitSegments()
    {
        FilterSegments = new ObservableCollection<SegmentItem>
        {
            new SegmentItem("Todos",    0, "all"),
            new SegmentItem("Críticos", 0, "critical"),
            new SegmentItem("Avisos",   0, "warning"),
            new SegmentItem("Resolvidos", 0, "resolved"),
        };
    }

    partial void OnSelectedSegmentIndexChanged(int value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            var alerts = await _alertService.GetAlertsAsync();
            _allAlerts = alerts.ToList();

            // Update segment counts
            int totalCount    = _allAlerts.Count;
            int criticalCount = _allAlerts.Count(a => !a.IsResolved && a.Severity == AlertSeverity.Critical);
            int warningCount  = _allAlerts.Count(a => !a.IsResolved && a.Severity == AlertSeverity.Warning);
            int resolvedCount = _allAlerts.Count(a => a.IsResolved);

            FilterSegments[0].Count = totalCount;
            FilterSegments[1].Count = criticalCount;
            FilterSegments[2].Count = warningCount;
            FilterSegments[3].Count = resolvedCount;

            ActiveAlertsCount = criticalCount + warningCount;

            ApplyFilter();
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var key = SelectedSegmentIndex >= 0 && SelectedSegmentIndex < FilterSegments.Count
            ? FilterSegments[SelectedSegmentIndex].Key
            : "all";

        var filtered = key switch
        {
            "critical" => _allAlerts.Where(a => !a.IsResolved && a.Severity == AlertSeverity.Critical),
            "warning"  => _allAlerts.Where(a => !a.IsResolved && a.Severity == AlertSeverity.Warning),
            "resolved" => _allAlerts.Where(a => a.IsResolved),
            _          => _allAlerts.AsEnumerable(),
        };

        FilteredAlerts = new ObservableCollection<AlertItem>(filtered);
    }

    [RelayCommand]
    private async Task ViewDetail(AlertItem? alert)
    {
        if (alert == null)
            return;

        await Shell.Current.GoToAsync(Routes.DeviceDetail, new System.Collections.Generic.Dictionary<string, object>
        {
            ["Alert"] = alert,
            ["AlertId"] = alert.Id
        });
    }
}
