using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Venther.App.Controls;
using Venther.App.Helpers;
using Venther.App.Models;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

namespace Venther.App.ViewModels;

public partial class AlertsViewModel : BaseViewModel
{
    private const int PageSize = 20;
    private readonly IAlertService _alertService;
    private readonly ITelemetryHubService _telemetryHub;
    private readonly List<AlertItem> _allAlerts = new();
    private int _currentPage = 1;
    private int _totalPages = 1;
    private bool _isLoadingMore;

    [ObservableProperty]
    private ObservableCollection<AlertItem> _filteredAlerts = new();

    [ObservableProperty]
    private ObservableCollection<SegmentItem> _filterSegments = new();

    [ObservableProperty]
    private int _selectedSegmentIndex;

    [ObservableProperty]
    private int _activeAlertsCount;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _emptyMessage = "Não há alertas para o filtro selecionado.";

    // TODO: No .NET MAUI Shell padrão não há propriedade nativa Tab.BadgeCount multiplataforma.
    // Para exibir o badge na TabBar no Android, implementar um ShellBottomNavViewAppearanceTracker
    // customizado utilizando bottomNavigationView.GetOrCreateBadge(tabIndex).Number = ActiveAlertsCount.

    public AlertsViewModel(IAlertService alertService, ITelemetryHubService telemetryHub)
    {
        _alertService = alertService;
        _telemetryHub = telemetryHub;
        Title = "Alertas";
        InitSegments();
        _telemetryHub.FaultRaised += OnFaultRaised;
    }

    private void InitSegments()
    {
        FilterSegments = new ObservableCollection<SegmentItem>
        {
            new SegmentItem("Todos", 0, "all"),
            new SegmentItem("Críticos", 0, "critical"),
            new SegmentItem("Avisos", 0, "warning"),
            new SegmentItem("Resolvidos", 0, "resolved"),
        };
    }

    partial void OnSelectedSegmentIndexChanged(int value) => ApplyFilter();

    [RelayCommand]
    private async Task Refresh()
    {
        if (IsBusy)
        {
            IsRefreshing = false;
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = string.Empty;
        EmptyMessage = "Não há alertas para o filtro selecionado.";
        _currentPage = 1;

        try
        {
            var page = await _alertService.GetAlertsAsync(_currentPage, PageSize);
            _allAlerts.Clear();
            _allAlerts.AddRange(page.Items);
            _totalPages = Math.Max(1, page.TotalPages);

            UpdateCounts();
            ApplyFilter();
        }
        catch (ApiException ex) when (ex.StatusCode is 401 or 403)
        {
            EmptyMessage = "Sem permissão para listar alertas";
            UpdateCounts();
            ApplyFilter();
        }
        catch (ApiException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "Não foi possível carregar os alertas.";
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsBusy || _isLoadingMore || _currentPage >= _totalPages)
            return;

        try
        {
            _isLoadingMore = true;
            var nextPage = _currentPage + 1;
            var page = await _alertService.GetAlertsAsync(nextPage, PageSize);

            if (page.Items.Count > 0)
            {
                _currentPage = nextPage;
                _totalPages = Math.Max(1, page.TotalPages);
                _allAlerts.AddRange(page.Items);
                UpdateCounts();
                ApplyFilter();
            }
        }
        catch
        {
            // Mantém os alertas já carregados
        }
        finally
        {
            _isLoadingMore = false;
        }
    }

    private void OnFaultRaised(FaultRaisedDto fault)
    {
        var date = DateTime.TryParse(fault.Date, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed.ToLocalTime()
            : DateTime.Now;

        bool exists = _allAlerts.Any(a =>
            a.BoardId == fault.BoardId &&
            Math.Abs((a.Date - date).TotalSeconds) < 2);

        if (exists)
            return;

        _allAlerts.Insert(0, new AlertItem
        {
            Id = $"live-{fault.BoardId}-{date.Ticks}",
            BoardId = fault.BoardId,
            Title = fault.FailureCause,
            DeviceInfo = fault.BoardId.ToString(),
            Severity = AlertSeverity.Critical,
            Date = date,
            IsResolved = false
        });

        UpdateCounts();
        ApplyFilter();
    }

    private void UpdateCounts()
    {
        int totalCount = _allAlerts.Count;
        int criticalCount = _allAlerts.Count(a => !a.IsResolved && a.Severity == AlertSeverity.Critical);
        int warningCount = _allAlerts.Count(a => !a.IsResolved && a.Severity == AlertSeverity.Warning);
        int resolvedCount = _allAlerts.Count(a => a.IsResolved);

        FilterSegments[0].Count = totalCount;
        FilterSegments[1].Count = criticalCount;
        FilterSegments[2].Count = warningCount;
        FilterSegments[3].Count = resolvedCount;

        ActiveAlertsCount = criticalCount + warningCount;
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
        if (alert == null || alert.BoardId == Guid.Empty)
            return;

        await Shell.Current.GoToAsync(Routes.DeviceDetail, new Dictionary<string, object>
        {
            ["DeviceId"] = alert.BoardId.ToString()
        });
    }
}
