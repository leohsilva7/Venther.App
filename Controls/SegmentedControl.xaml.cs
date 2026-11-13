using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Venther.App.Controls;

public partial class SegmentedControl : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable<SegmentItem>),
            typeof(SegmentedControl),
            default(IEnumerable<SegmentItem>),
            propertyChanged: (b, _, _) => ((SegmentedControl)b).RenderSegments());

    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(
            nameof(SelectedIndex),
            typeof(int),
            typeof(SegmentedControl),
            0,
            BindingMode.TwoWay,
            propertyChanged: (b, _, _) => ((SegmentedControl)b).UpdateSelection());

    public static readonly BindableProperty SelectionChangedCommandProperty =
        BindableProperty.Create(
            nameof(SelectionChangedCommand),
            typeof(ICommand),
            typeof(SegmentedControl),
            default(ICommand));

    public IEnumerable<SegmentItem>? ItemsSource
    {
        get => (IEnumerable<SegmentItem>?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public ICommand? SelectionChangedCommand
    {
        get => (ICommand?)GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public SegmentedControl()
    {
        InitializeComponent();
    }

    public void Refresh()
    {
        RenderSegments();
    }

    private void RenderSegments()
    {
        SegmentsGrid.Children.Clear();
        SegmentsGrid.ColumnDefinitions.Clear();

        if (ItemsSource == null)
            return;

        var items = ItemsSource.ToList();
        if (items.Count == 0)
            return;

        for (int i = 0; i < items.Count; i++)
        {
            SegmentsGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            var item = items[i];
            int index = i;
            bool isSelected = (index == SelectedIndex);

            var pill = CreateSegmentPill(item, index, isSelected);
            Grid.SetColumn(pill, index);
            SegmentsGrid.Children.Add(pill);
        }
    }

    private View CreateSegmentPill(SegmentItem item, int index, bool isSelected)
    {
        var border = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) },
            BackgroundColor = isSelected ? GetThemeColor("White", Colors.White) : Colors.Transparent,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };

        if (isSelected)
        {
            border.Shadow = new Shadow
            {
                Brush = new SolidColorBrush(Colors.Black),
                Opacity = 0.08f,
                Offset = new Point(0, 1),
                Radius = 2
            };
        }

        var stack = new HorizontalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var titleLabel = new Label
        {
            Text = item.Title,
            FontFamily = isSelected ? "InterSemiBold" : "InterRegular",
            FontSize = 13,
            TextColor = isSelected ? GetThemeColor("OnSurface", Color.FromArgb("#161C22")) : GetThemeColor("OnSurfaceVariant", Color.FromArgb("#424751")),
            VerticalOptions = LayoutOptions.Center
        };
        stack.Children.Add(titleLabel);

        // Counter badge
        if (item.Count >= 0)
        {
            var badgeBorder = new Border
            {
                StrokeThickness = 0,
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(9) },
                BackgroundColor = isSelected ? GetThemeColor("Surface", Color.FromArgb("#EEF4FC")) : Colors.Transparent,
                Padding = new Thickness(6, 1),
                VerticalOptions = LayoutOptions.Center
            };

            var countLabel = new Label
            {
                Text = item.Count.ToString(),
                FontFamily = "InterBold",
                FontSize = 11,
                TextColor = isSelected ? GetThemeColor("Primary", Color.FromArgb("#00326F")) : GetThemeColor("OnSurfaceVariant", Color.FromArgb("#424751")),
                VerticalOptions = LayoutOptions.Center
            };
            badgeBorder.Content = countLabel;
            stack.Children.Add(badgeBorder);
        }

        border.Content = stack;

        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            if (SelectedIndex != index)
            {
                SelectedIndex = index;
                SelectionChangedCommand?.Execute(item);
            }
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }

    private void UpdateSelection()
    {
        RenderSegments();
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
