using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Venther.App.Services.Api;
using Venther.App.Services.Interfaces;

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

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(
            nameof(Subtitle),
            typeof(string),
            typeof(AppHeader),
            default(string),
            propertyChanged: (b, _, n) => ((AppHeader)b).OnSubtitleChanged((string?)n));

    public static readonly BindableProperty ShowBackButtonProperty =
        BindableProperty.Create(
            nameof(ShowBackButton),
            typeof(bool),
            typeof(AppHeader),
            false,
            propertyChanged: (b, _, n) => ((AppHeader)b).BackButton.IsVisible = (bool)n);

    public static readonly BindableProperty BackCommandProperty =
        BindableProperty.Create(
            nameof(BackCommand),
            typeof(ICommand),
            typeof(AppHeader),
            default(ICommand));

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

    public static readonly BindableProperty RightActionContentProperty =
        BindableProperty.Create(
            nameof(RightActionContent),
            typeof(View),
            typeof(AppHeader),
            default(View),
            propertyChanged: (b, _, n) => ((AppHeader)b).OnRightActionContentChanged((View?)n));

    public static readonly BindableProperty BottomContentProperty =
        BindableProperty.Create(
            nameof(BottomContent),
            typeof(View),
            typeof(AppHeader),
            default(View),
            propertyChanged: (b, _, n) => ((AppHeader)b).OnBottomContentChanged((View?)n));

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool ShowBackButton
    {
        get => (bool)GetValue(ShowBackButtonProperty);
        set => SetValue(ShowBackButtonProperty, value);
    }

    public ICommand? BackCommand
    {
        get => (ICommand?)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
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

    public View? RightActionContent
    {
        get => (View?)GetValue(RightActionContentProperty);
        set => SetValue(RightActionContentProperty, value);
    }

    public View? BottomContent
    {
        get => (View?)GetValue(BottomContentProperty);
        set => SetValue(BottomContentProperty, value);
    }

    public AppHeader()
    {
        InitializeComponent();
    }

    private void OnTitleChanged(string? newTitle)
    {
        bool hasTitle = !string.IsNullOrWhiteSpace(newTitle);
        TitleLayout.IsVisible = hasTitle;
        BrandingLayout.IsVisible = !hasTitle;
    }

    private void OnSubtitleChanged(string? newSubtitle)
    {
        bool hasSubtitle = !string.IsNullOrWhiteSpace(newSubtitle);
        SubtitleLabel.IsVisible = hasSubtitle;
    }

    private void OnRightActionContentChanged(View? newView)
    {
        RightActionContainer.Content = newView;
        RightActionContainer.IsVisible = newView != null;
    }

    private void OnBottomContentChanged(View? newView)
    {
        BottomContainer.Content = newView;
        BottomContainer.IsVisible = newView != null;
    }

    private async void OnBackTapped(object? sender, TappedEventArgs e)
    {
        if (BackCommand?.CanExecute(null) == true)
        {
            BackCommand.Execute(null);
            return;
        }

        if (Shell.Current != null)
        {
            await Shell.Current.GoToAsync("..");
        }
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

    private void OnProfileTapped(object? sender, TappedEventArgs e)
    {
        var popup = new ProfileMenuPopup();
        var currentPage = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0].Page : null;
        if (currentPage != null)
        {
            currentPage.ShowPopup(popup);
        }
    }
}
