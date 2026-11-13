using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Venther.App.Controls;

public partial class AppEntry : ContentView
{
    private bool _isFocused;
    private bool _isPasswordRevealed;

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(
            nameof(Text),
            typeof(string),
            typeof(AppEntry),
            default(string),
            BindingMode.TwoWay);

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(
            nameof(Placeholder),
            typeof(string),
            typeof(AppEntry),
            string.Empty);

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(
            nameof(Label),
            typeof(string),
            typeof(AppEntry),
            string.Empty,
            propertyChanged: (b, _, n) => ((AppEntry)b).OnLabelChanged((string)n));

    public static readonly BindableProperty IsPasswordProperty =
        BindableProperty.Create(
            nameof(IsPassword),
            typeof(bool),
            typeof(AppEntry),
            false,
            propertyChanged: (b, _, n) => ((AppEntry)b).OnIsPasswordChanged((bool)n));

    public static readonly BindableProperty KeyboardProperty =
        BindableProperty.Create(
            nameof(Keyboard),
            typeof(Keyboard),
            typeof(AppEntry),
            Keyboard.Default,
            propertyChanged: (b, _, n) => ((AppEntry)b).InnerEntry.Keyboard = (Keyboard)n);

    public static readonly BindableProperty ReturnTypeProperty =
        BindableProperty.Create(
            nameof(ReturnType),
            typeof(ReturnType),
            typeof(AppEntry),
            ReturnType.Default,
            propertyChanged: (b, _, n) => ((AppEntry)b).InnerEntry.ReturnType = (ReturnType)n);

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(
            nameof(HasError),
            typeof(bool),
            typeof(AppEntry),
            false,
            propertyChanged: (b, _, _) => ((AppEntry)b).UpdateVisualState());

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(
            nameof(ErrorMessage),
            typeof(string),
            typeof(AppEntry),
            string.Empty,
            propertyChanged: (b, _, _) => ((AppEntry)b).UpdateVisualState());

    public string? Text
    {
        get => (string?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public ReturnType ReturnType
    {
        get => (ReturnType)GetValue(ReturnTypeProperty);
        set => SetValue(ReturnTypeProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public AppEntry()
    {
        InitializeComponent();

        InnerEntry.Focused += OnEntryFocused;
        InnerEntry.Unfocused += OnEntryUnfocused;

        UpdateVisualState();
    }

    private void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        _isFocused = true;
        UpdateVisualState();
    }

    private void OnEntryUnfocused(object? sender, FocusEventArgs e)
    {
        _isFocused = false;
        UpdateVisualState();
    }

    private void OnBorderTapped(object? sender, TappedEventArgs e)
    {
        InnerEntry.Focus();
    }

    private void OnLabelChanged(string newLabel)
    {
        FieldLabel.IsVisible = !string.IsNullOrWhiteSpace(newLabel);
    }

    private void OnIsPasswordChanged(bool isPassword)
    {
        _isPasswordRevealed = false;
        InnerEntry.IsPassword = isPassword;
        PasswordToggleBorder.IsVisible = isPassword;
        EyeOpenPath.IsVisible = true;
        EyeClosedPath.IsVisible = false;
    }

    private void OnTogglePasswordTapped(object? sender, TappedEventArgs e)
    {
        _isPasswordRevealed = !_isPasswordRevealed;
        InnerEntry.IsPassword = !_isPasswordRevealed;
        EyeOpenPath.IsVisible = !_isPasswordRevealed;
        EyeClosedPath.IsVisible = _isPasswordRevealed;
    }

    private void UpdateVisualState()
    {
        if (HasError)
        {
            InputBorder.BackgroundColor = GetThemeColor("ErrorBg", Color.FromArgb("#FEF2F2"));
            InputBorder.Stroke = GetThemeBrush("ErrorBorder", Color.FromArgb("#FECACA"));
            InputBorder.StrokeThickness = 1;
            ErrorLabel.IsVisible = !string.IsNullOrWhiteSpace(ErrorMessage);
        }
        else if (_isFocused)
        {
            InputBorder.BackgroundColor = GetThemeColor("White", Colors.White);
            InputBorder.Stroke = GetThemeBrush("Primary", Color.FromArgb("#00326F"));
            InputBorder.StrokeThickness = 1.5;
            ErrorLabel.IsVisible = false;
        }
        else
        {
            InputBorder.BackgroundColor = GetThemeColor("White", Colors.White);
            InputBorder.Stroke = GetThemeBrush("AuthBorder", Color.FromArgb("#E2E8F0"));
            InputBorder.StrokeThickness = 1;
            ErrorLabel.IsVisible = false;
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

    private Brush GetThemeBrush(string key, Color fallback)
    {
        if (Application.Current?.Resources.TryGetValue(key, out var val) == true)
        {
            if (val is Brush brush) return brush;
            if (val is Color color) return new SolidColorBrush(color);
        }
        return new SolidColorBrush(fallback);
    }
}
