namespace Venther.App.Controls;

public partial class StepProgress : ContentView
{
    public static readonly BindableProperty CurrentStepProperty =
        BindableProperty.Create(nameof(CurrentStep), typeof(int), typeof(StepProgress), 1,
            propertyChanged: OnStepChanged);

    public static readonly BindableProperty TotalStepsProperty =
        BindableProperty.Create(nameof(TotalSteps), typeof(int), typeof(StepProgress), 3,
            propertyChanged: OnStepChanged);

    public int CurrentStep
    {
        get => (int)GetValue(CurrentStepProperty);
        set => SetValue(CurrentStepProperty, value);
    }

    public int TotalSteps
    {
        get => (int)GetValue(TotalStepsProperty);
        set => SetValue(TotalStepsProperty, value);
    }

    public string StepText => $"PASSO {CurrentStep} DE {TotalSteps}";

    public double ProgressValue => TotalSteps > 0
        ? Math.Clamp((double)CurrentStep / TotalSteps, 0.0, 1.0)
        : 0.0;

    public string PercentageText => $"{(int)Math.Round(ProgressValue * 100)}%";

    public StepProgress()
    {
        InitializeComponent();
    }

    private static void OnStepChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StepProgress control)
        {
            control.OnPropertyChanged(nameof(StepText));
            control.OnPropertyChanged(nameof(ProgressValue));
            control.OnPropertyChanged(nameof(PercentageText));
        }
    }
}