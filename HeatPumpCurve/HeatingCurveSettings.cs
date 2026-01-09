using System.Text.Json;

namespace HeatPumpCurve;

/// <summary>
/// Manages persistent storage of user's heating curve configuration.
/// Settings are saved to AppData in JSON format.
/// </summary>
public class HeatingCurveSettings
{
    // Water temperature values for each outside temperature reference point
    // Default values represent a typical heating curve for well-insulated homes
    public double TempAt_Minus20 { get; set; } = 53;
    public double TempAt_9 { get; set; } = 33;
    public double TempAt_15 { get; set; } = 23;
    public double TempAt_20 { get; set; } = 20;
    public double TempAt_25 { get; set; } = 18;
    
    // User's preferred interpolation mode: "Smooth" (cubic spline) or "Linear"
    public string CurveMode { get; set; } = "Linear";

    private static string SettingsFilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "HeatPumpCurve",
        "settings.json");

    /// <summary>
    /// Loads settings from disk or returns defaults if file doesn't exist.
    /// </summary>
    public static HeatingCurveSettings Load()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                string json = File.ReadAllText(SettingsFilePath);
                return JsonSerializer.Deserialize<HeatingCurveSettings>(json) ?? new HeatingCurveSettings();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading settings: {ex.Message}");
        }
        return new HeatingCurveSettings();
    }

    /// <summary>
    /// Saves current settings to disk in JSON format.
    /// </summary>
    public void Save()
    {
        try
        {
            string? directory = Path.GetDirectoryName(SettingsFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }
}
