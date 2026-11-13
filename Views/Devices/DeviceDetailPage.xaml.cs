using System.ComponentModel;
using Venther.App.Helpers;
using Venther.App.ViewModels;

namespace Venther.App.Views.Devices;

public partial class DeviceDetailPage : ContentPage
{
    private readonly DeviceDetailViewModel _vm;
    private readonly TrendChartDrawable _drawable = new();

    public DeviceDetailPage(DeviceDetailViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
        TrendChart.Drawable = _drawable;
        _vm.PropertyChanged += OnVmPropertyChanged;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }

    private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DeviceDetailViewModel.TrendValues))
        {
            _drawable.Values = _vm.TrendValues;
            TrendChart.Invalidate();
        }
    }
}