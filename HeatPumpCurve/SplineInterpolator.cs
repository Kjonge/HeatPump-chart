namespace HeatPumpCurve;

/// <summary>
/// Cubic spline interpolation for smooth curves between data points
/// </summary>
public class SplineInterpolator
{
    private readonly double[] _x;
    private readonly double[] _y;
    private readonly double[] _a;
    private readonly double[] _b;
    private readonly double[] _c;
    private readonly double[] _d;

    public SplineInterpolator(double[] x, double[] y)
    {
        if (x.Length != y.Length)
            throw new ArgumentException("Arrays must have the same length");
        if (x.Length < 2)
            throw new ArgumentException("Need at least 2 points");

        _x = x;
        _y = y;
        int n = x.Length;

        _a = new double[n];
        _b = new double[n];
        _c = new double[n];
        _d = new double[n];

        // Copy y values to a
        for (int i = 0; i < n; i++)
            _a[i] = y[i];

        if (n == 2)
        {
            // Linear interpolation for 2 points
            _b[0] = (y[1] - y[0]) / (x[1] - x[0]);
            return;
        }

        // Calculate spline coefficients
        double[] h = new double[n - 1];
        for (int i = 0; i < n - 1; i++)
            h[i] = x[i + 1] - x[i];

        double[] alpha = new double[n - 1];
        for (int i = 1; i < n - 1; i++)
            alpha[i] = 3.0 / h[i] * (_a[i + 1] - _a[i]) - 3.0 / h[i - 1] * (_a[i] - _a[i - 1]);

        double[] l = new double[n];
        double[] mu = new double[n];
        double[] z = new double[n];

        l[0] = 1;
        mu[0] = 0;
        z[0] = 0;

        for (int i = 1; i < n - 1; i++)
        {
            l[i] = 2 * (x[i + 1] - x[i - 1]) - h[i - 1] * mu[i - 1];
            mu[i] = h[i] / l[i];
            z[i] = (alpha[i] - h[i - 1] * z[i - 1]) / l[i];
        }

        l[n - 1] = 1;
        z[n - 1] = 0;
        _c[n - 1] = 0;

        for (int j = n - 2; j >= 0; j--)
        {
            _c[j] = z[j] - mu[j] * _c[j + 1];
            _b[j] = (_a[j + 1] - _a[j]) / h[j] - h[j] * (_c[j + 1] + 2 * _c[j]) / 3;
            _d[j] = (_c[j + 1] - _c[j]) / (3 * h[j]);
        }
    }

    public double Interpolate(double xValue)
    {
        // Clamp to range
        if (xValue <= _x[0]) return _y[0];
        if (xValue >= _x[_x.Length - 1]) return _y[_y.Length - 1];

        // Find the right interval
        int i = 0;
        for (int j = 0; j < _x.Length - 1; j++)
        {
            if (xValue >= _x[j] && xValue < _x[j + 1])
            {
                i = j;
                break;
            }
        }

        double dx = xValue - _x[i];
        return _a[i] + _b[i] * dx + _c[i] * dx * dx + _d[i] * dx * dx * dx;
    }

    /// <summary>
    /// Generate interpolated values for a range of x values
    /// </summary>
    public List<(double X, double Y)> GeneratePoints(double startX, double endX, int count)
    {
        var points = new List<(double X, double Y)>();
        double step = (endX - startX) / (count - 1);

        for (int i = 0; i < count; i++)
        {
            double x = startX + i * step;
            double y = Interpolate(x);
            points.Add((x, y));
        }

        return points;
    }
}
