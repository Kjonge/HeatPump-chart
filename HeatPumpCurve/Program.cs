namespace HeatPumpCurve;

/// <summary>
/// Application entry point for the Heat Pump Heating Curve Calculator.
/// </summary>
static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// Initializes Windows Forms and starts the main form.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
