using System;
using System.Collections.Generic;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;

namespace Venther.App.Controls.Dialogs;

public partial class ActionSheetPopup : Popup
{
    public ActionSheetPopup(string title, string cancelText, IEnumerable<string> options)
    {
        InitializeComponent();

        TitleLabel.Text = title;
        CancelLabel.Text = cancelText;

        foreach (var option in options)
        {
            var border = new Border
            {
                StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
                StrokeThickness = 1,
                Stroke = (Color)Application.Current!.Resources["AuthBorder"],
                BackgroundColor = (Color)Application.Current!.Resources["White"],
                HeightRequest = 48,
                Padding = new Thickness(16, 0)
            };

            var label = new Label
            {
                Text = option,
                FontFamily = "InterMedium",
                FontSize = 14,
                TextColor = (Color)Application.Current!.Resources["OnSurface"],
                VerticalOptions = LayoutOptions.Center
            };

            border.Content = label;

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) => Close(option);
            border.GestureRecognizers.Add(tapGesture);

            OptionsLayout.Children.Add(border);
        }
    }

    private void OnCancelTapped(object? sender, EventArgs e)
    {
        Close(null);
    }
}
