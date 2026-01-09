using ScottPlot.WinForms;

namespace HeatPumpCurve;

public partial class MainForm : Form
{
    private HeatingCurveSettings _settings;
    private TabControl tabControl = null!;
    private FormsPlot formsPlot = null!;
    private DataGridView dataGrid = null!;

    // Input controls for temperature values
    private NumericUpDown numMinus20 = null!;
    private NumericUpDown num9 = null!;
    private NumericUpDown num15 = null!;
    private NumericUpDown num20 = null!;
    private NumericUpDown num25 = null!;
    private ComboBox cmbInterpolation = null!;

    // Outside temperature points (fixed)
    private readonly double[] _outsideTemps = { -20, 9, 15, 20, 25 };

    public MainForm()
    {
        _settings = HeatingCurveSettings.Load();
        InitializeComponent();
        LoadSettingsToControls();
        UpdateChartAndTable();
    }

    private void InitializeComponent()
    {
        this.Text = "Heat Pump - Heating Curve";
        this.Size = new Size(1000, 700);
        this.MinimumSize = new Size(1000, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = System.Drawing.Color.FromArgb(30, 35, 40);
        this.ForeColor = System.Drawing.Color.White;

        // Main layout
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Input panel
        var inputPanel = CreateInputPanel();
        mainPanel.Controls.Add(inputPanel, 0, 0);

        // Tab control for Chart and Table views
        tabControl = new TabControl
        {
            Dock = DockStyle.Fill
        };

        // Chart tab
        var chartTab = new TabPage("Chart")
        {
            BackColor = System.Drawing.Color.FromArgb(30, 35, 40)
        };
        formsPlot = CreateChart();
        chartTab.Controls.Add(formsPlot);
        tabControl.TabPages.Add(chartTab);

        // Table tab
        var tableTab = new TabPage("Table")
        {
            BackColor = System.Drawing.Color.FromArgb(30, 35, 40)
        };
        dataGrid = CreateDataGrid();
        tableTab.Controls.Add(dataGrid);
        tabControl.TabPages.Add(tableTab);

        mainPanel.Controls.Add(tabControl, 0, 1);

        this.Controls.Add(mainPanel);
    }

    private Panel CreateInputPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = System.Drawing.Color.FromArgb(40, 45, 50),
            Padding = new Padding(10)
        };

        var titleLabel = new System.Windows.Forms.Label
        {
            Text = "Enter water temperature (°C) for each outside temperature:",
            ForeColor = System.Drawing.Color.White,
            Font = new Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
            AutoSize = true,
            Location = new Point(10, 10)
        };
        panel.Controls.Add(titleLabel);

        // Create input fields for each temperature point
        int startX = 15;
        int startY = 55;
        int spacing = 115;

        numMinus20 = CreateTempInput(panel, "-20°C:", startX, startY);
        num9 = CreateTempInput(panel, "9°C:", startX + spacing, startY);
        num15 = CreateTempInput(panel, "15°C:", startX + spacing * 2, startY);
        num20 = CreateTempInput(panel, "20°C:", startX + spacing * 3, startY);
        num25 = CreateTempInput(panel, "25°C:", startX + spacing * 4, startY);

        // Interpolation mode selector
        int curveTypeX = startX + spacing * 5 + 10;
        var interpolationLabel = new System.Windows.Forms.Label
        {
            Text = "Curve type:",
            ForeColor = System.Drawing.Color.LightGray,
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(curveTypeX, startY - 20)
        };
        panel.Controls.Add(interpolationLabel);

        cmbInterpolation = new ComboBox
        {
            Location = new Point(curveTypeX, startY + 5),
            Size = new Size(100, 25),
            DropDownStyle = ComboBoxStyle.DropDownList,
            BackColor = System.Drawing.Color.FromArgb(50, 55, 60),
            ForeColor = System.Drawing.Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9)
        };
        cmbInterpolation.Items.AddRange(new object[] { "Smooth", "Linear" });
        cmbInterpolation.SelectedIndex = _settings.CurveMode == "Smooth" ? 0 : 1;
        cmbInterpolation.SelectedIndexChanged += (s, e) => UpdateChartAndTable();
        panel.Controls.Add(cmbInterpolation);

        // Update button
        var updateButton = new Button
        {
            Text = "Update",
            Location = new Point(curveTypeX + 115, startY + 3),
            Size = new Size(90, 30),
            BackColor = System.Drawing.Color.FromArgb(255, 152, 0),
            ForeColor = System.Drawing.Color.Black,
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9, System.Drawing.FontStyle.Bold)
        };
        updateButton.FlatAppearance.BorderSize = 0;
        updateButton.Click += UpdateButton_Click;
        panel.Controls.Add(updateButton);

        return panel;
    }

    private NumericUpDown CreateTempInput(Panel parent, string labelText, int x, int y)
    {
        var label = new System.Windows.Forms.Label
        {
            Text = labelText,
            ForeColor = System.Drawing.Color.LightGray,
            Font = new Font("Segoe UI", 9),
            AutoSize = true,
            Location = new Point(x, y - 20)
        };
        parent.Controls.Add(label);

        var numUpDown = new NumericUpDown
        {
            Location = new Point(x, y + 5),
            Size = new Size(80, 28),
            Minimum = 10,
            Maximum = 80,
            DecimalPlaces = 1,
            BackColor = System.Drawing.Color.FromArgb(50, 55, 60),
            ForeColor = System.Drawing.Color.White,
            Font = new Font("Segoe UI", 10)
        };
        parent.Controls.Add(numUpDown);

        return numUpDown;
    }

    private FormsPlot CreateChart()
    {
        var plot = new FormsPlot
        {
            Dock = DockStyle.Fill
        };

        // Configure dark theme
        plot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1e2328");
        plot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#282d32");
        
        // Configure axes
        plot.Plot.Axes.Bottom.Label.Text = "Outside temperature (°C)";
        plot.Plot.Axes.Bottom.Label.ForeColor = ScottPlot.Colors.White;
        plot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Colors.White;
        plot.Plot.Axes.Bottom.MajorTickStyle.Color = ScottPlot.Colors.Gray;
        plot.Plot.Axes.Bottom.MinorTickStyle.Color = ScottPlot.Colors.Gray;
        plot.Plot.Axes.Bottom.FrameLineStyle.Color = ScottPlot.Colors.Gray;

        plot.Plot.Axes.Left.Label.Text = "Water temperature (°C)";
        plot.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.White;
        plot.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Colors.White;
        plot.Plot.Axes.Left.MajorTickStyle.Color = ScottPlot.Colors.Gray;
        plot.Plot.Axes.Left.MinorTickStyle.Color = ScottPlot.Colors.Gray;
        plot.Plot.Axes.Left.FrameLineStyle.Color = ScottPlot.Colors.Gray;

        // Set axis limits
        plot.Plot.Axes.SetLimits(-25, 30, 0, 80);

        // Configure grid
        plot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#464b50");

        // Title
        plot.Plot.Title("Heating Curve");
        plot.Plot.Axes.Title.Label.ForeColor = ScottPlot.Colors.White;
        plot.Plot.Axes.Title.Label.FontSize = 18;

        return plot;
    }

    private DataGridView CreateDataGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = System.Drawing.Color.FromArgb(40, 45, 50),
            GridColor = System.Drawing.Color.FromArgb(70, 75, 80),
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(50, 55, 60),
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 10, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            },
            DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(40, 45, 50),
                ForeColor = System.Drawing.Color.White,
                Font = new Font("Segoe UI", 9),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = System.Drawing.Color.FromArgb(255, 152, 0),
                SelectionForeColor = System.Drawing.Color.Black
            },
            AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(45, 50, 55),
                ForeColor = System.Drawing.Color.White,
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                SelectionBackColor = System.Drawing.Color.FromArgb(255, 152, 0),
                SelectionForeColor = System.Drawing.Color.Black
            },
            EnableHeadersVisualStyles = false
        };

        grid.Columns.Add("OutsideTemp", "Outside Temperature (°C)");
        grid.Columns.Add("WaterTemp", "Water Temperature (°C)");

        return grid;
    }

    private void LoadSettingsToControls()
    {
        numMinus20.Value = (decimal)_settings.TempAt_Minus20;
        num9.Value = (decimal)_settings.TempAt_9;
        num15.Value = (decimal)_settings.TempAt_15;
        num20.Value = (decimal)_settings.TempAt_20;
        num25.Value = (decimal)_settings.TempAt_25;
        cmbInterpolation.SelectedIndex = _settings.CurveMode == "Smooth" ? 0 : 1;
    }

    private void SaveSettingsFromControls()
    {
        _settings.TempAt_Minus20 = (double)numMinus20.Value;
        _settings.TempAt_9 = (double)num9.Value;
        _settings.TempAt_15 = (double)num15.Value;
        _settings.TempAt_20 = (double)num20.Value;
        _settings.TempAt_25 = (double)num25.Value;
        _settings.CurveMode = cmbInterpolation.SelectedIndex == 0 ? "Smooth" : "Linear";
        _settings.Save();
    }

    private void UpdateButton_Click(object? sender, EventArgs e)
    {
        SaveSettingsFromControls();
        UpdateChartAndTable();
    }

    private void UpdateChartAndTable()
    {
        double[] waterTemps = {
            (double)numMinus20.Value,
            (double)num9.Value,
            (double)num15.Value,
            (double)num20.Value,
            (double)num25.Value
        };

        // Determine interpolation mode
        bool useSmooth = cmbInterpolation.SelectedIndex == 0;

        // Generate curve points based on interpolation mode
        double[] curveX;
        double[] curveY;

        if (useSmooth)
        {
            // Smooth spline interpolation
            var spline = new SplineInterpolator(_outsideTemps, waterTemps);
            var smoothPoints = spline.GeneratePoints(-20, 25, 100);
            curveX = smoothPoints.Select(p => p.X).ToArray();
            curveY = smoothPoints.Select(p => p.Y).ToArray();
        }
        else
        {
            // Linear interpolation - just connect the data points
            curveX = _outsideTemps;
            curveY = waterTemps;
        }

        // Clear and update chart
        formsPlot.Plot.Clear();
        ApplyPlotStyle();

        // Add curve line
        var linePlot = formsPlot.Plot.Add.ScatterLine(curveX, curveY);
        linePlot.Color = ScottPlot.Color.FromHex("#ff9800");
        linePlot.LineWidth = 3;

        // Add data point markers
        var markers = formsPlot.Plot.Add.Scatter(_outsideTemps, waterTemps);
        markers.Color = ScottPlot.Color.FromHex("#ff9800");
        markers.MarkerSize = 12;
        markers.MarkerStyle.Shape = ScottPlot.MarkerShape.FilledCircle;
        markers.MarkerStyle.OutlineColor = ScottPlot.Colors.White;
        markers.MarkerStyle.OutlineWidth = 2;
        markers.LineWidth = 0;

        // Reset axis limits and refresh
        formsPlot.Plot.Axes.SetLimits(-25, 30, 0, 80);
        formsPlot.Refresh();

        // Update table with 1°C intervals
        dataGrid.Rows.Clear();
        SplineInterpolator? tableSpline = useSmooth ? new SplineInterpolator(_outsideTemps, waterTemps) : null;
        for (int temp = -20; temp <= 25; temp++)
        {
            double waterTemp = useSmooth
                ? tableSpline!.Interpolate(temp)
                : LinearInterpolate(_outsideTemps, waterTemps, temp);

            dataGrid.Rows.Add($"{temp}°C", $"{waterTemp:F1}°C");
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        SaveSettingsFromControls();
        base.OnFormClosing(e);
    }

    private void ApplyPlotStyle()
    {
        formsPlot.Plot.FigureBackground.Color = ScottPlot.Color.FromHex("#1e2328");
        formsPlot.Plot.DataBackground.Color = ScottPlot.Color.FromHex("#282d32");
        formsPlot.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#464b50");
        formsPlot.Plot.Axes.Bottom.Label.Text = "Outside temperature (°C)";
        formsPlot.Plot.Axes.Bottom.Label.ForeColor = ScottPlot.Colors.White;
        formsPlot.Plot.Axes.Bottom.TickLabelStyle.ForeColor = ScottPlot.Colors.White;
        formsPlot.Plot.Axes.Bottom.MajorTickStyle.Color = ScottPlot.Colors.Gray;
        formsPlot.Plot.Axes.Bottom.MinorTickStyle.Color = ScottPlot.Colors.Gray;
        formsPlot.Plot.Axes.Bottom.FrameLineStyle.Color = ScottPlot.Colors.Gray;

        formsPlot.Plot.Axes.Left.Label.Text = "Water temperature (°C)";
        formsPlot.Plot.Axes.Left.Label.ForeColor = ScottPlot.Colors.White;
        formsPlot.Plot.Axes.Left.TickLabelStyle.ForeColor = ScottPlot.Colors.White;
        formsPlot.Plot.Axes.Left.MajorTickStyle.Color = ScottPlot.Colors.Gray;
        formsPlot.Plot.Axes.Left.MinorTickStyle.Color = ScottPlot.Colors.Gray;
        formsPlot.Plot.Axes.Left.FrameLineStyle.Color = ScottPlot.Colors.Gray;

        formsPlot.Plot.Title("Heating Curve");
        formsPlot.Plot.Axes.Title.Label.ForeColor = ScottPlot.Colors.White;
        formsPlot.Plot.Axes.Title.Label.FontSize = 18;
    }

    private static double LinearInterpolate(double[] xValues, double[] yValues, double x)
    {
        // Handle edge cases
        if (x <= xValues[0]) return yValues[0];
        if (x >= xValues[xValues.Length - 1]) return yValues[yValues.Length - 1];

        // Find the interval
        for (int i = 0; i < xValues.Length - 1; i++)
        {
            if (x >= xValues[i] && x <= xValues[i + 1])
            {
                double t = (x - xValues[i]) / (xValues[i + 1] - xValues[i]);
                return yValues[i] + t * (yValues[i + 1] - yValues[i]);
            }
        }

        return yValues[yValues.Length - 1];
    }
}
