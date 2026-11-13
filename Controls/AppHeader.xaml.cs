using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Venther.App.Controls;

public partial class AppHeader : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(AppHeader),
            default(string),
            propertyChanged: (b, _, n) => ((AppHeader)b).OnTitleChanged((string?)n));

    public static readonly BindableProperty ShowFilterButtonProperty =
        BindableProperty.Create(
            nameof(ShowFilterButton),
            typeof(bool),
            typeof(AppHeader),
            false,
            propertyChanged: (b, _, n) => ((AppHeader)b).FilterButton.IsVisible = (bool)n);

    public static readonly BindableProperty FilterCommandProperty =
        BindableProperty.Create(
            nameof(FilterCommand),
            typeof(ICommand),
            typeof(AppHeader),
            default(ICommand));

    public static readonly BindableProperty ShowSyncButtonProperty =
        BindableProperty.Create(
            nameof(ShowSyncButton),
            typeof(bool),
            typeof(AppHeader),
            false,
            propertyChanged: (b, _, n) => ((AppHeader)b).SyncButton.IsVisible = (bool)n);

    public static readonly BindableProperty ShowBellButtonProperty =
        BindableProperty.Create(
            nameof(ShowBellButton),
            typeof(bool),
            typeof(AppHeader),
            true,
            propertyChanged: (b, _, n) => ((AppHeader)b).BellButton.IsVisible = (bool)n);

    public static readonly BindableProperty SyncCommandProperty =
        BindableProperty.Create(
            nameof(SyncCommand),
            typeof(ICommand),
            typeof(AppHeader),
            default(ICommand));

    public static readonly BindableProperty BellCommandProperty =
        BindableProperty.Create(
            nameof(BellCommand),
            typeof(ICommand),
            typeof(AppHeader),
            default(ICommand));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool ShowFilterButton
    {
        get => (bool)GetValue(ShowFilterButtonProperty);
        set => SetValue(ShowFilterButtonProperty, value);
    }

    public ICommand? FilterCommand
    {
        get => (ICommand?)GetValue(FilterCommandProperty);
        set => SetValue(FilterCommandProperty, value);
    }

    public bool ShowSyncButton
    {
        get => (bool)GetValue(ShowSyncButtonProperty);
        set => SetValue(ShowSyncButtonProperty, value);
    }

    public bool ShowBellButton
    {
        get => (bool)GetValue(ShowBellButtonProperty);
        set => SetValue(ShowBellButtonProperty, value);
    }

    public ICommand? SyncCommand
    {
        get => (ICommand?)GetValue(SyncCommandProperty);
        set => SetValue(SyncCommandProperty, value);
    }

    public ICommand? BellCommand
    {
        get => (ICommand?)GetValue(BellCommandProperty);
        set => SetValue(BellCommandProperty, value);
    }

    public AppHeader()
    {
        InitializeComponent();
    }

    private void OnTitleChanged(string? newTitle)
    {
        bool hasTitle = !string.IsNullOrWhiteSpace(newTitle);
        TitleLabel.IsVisible = hasTitle;
        BrandingLayout.IsVisible = !hasTitle;
    }

    private void OnFilterTapped(object? sender, TappedEventArgs e)
    {
        if (FilterCommand?.CanExecute(null) == true)
            FilterCommand.Execute(null);
    }

    private void OnSyncTapped(object? sender, TappedEventArgs e)
    {
        if (SyncCommand?.CanExecute(null) == true)
        {
            SyncCommand.Execute(null);
        }
    }

    private async void OnBellTapped(object? sender, TappedEventArgs e)
    {
        if (BellCommand?.CanExecute(null) == true)
        {
            BellCommand.Execute(null);
            return;
        }

        // Navega diretamente para a aba de Alertas
        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("//main/alerts");
        }
    }
}
