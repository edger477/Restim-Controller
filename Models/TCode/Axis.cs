public class AxisData
{
    public AxisData()
    {
        NewFrequency = Frequency;
    }

    public string Name { get; set; } = "alpha";
    public string TCodeAxis { get; set; } = "L0";
    public double ValueMin { get; set; } = 0;
    public double ValueMax { get; set; } = 0.998;
    public double Step { get; set; }
    public double Frequency { get; set; } = 0; // Oscillations per second
    public double FrequencyMin { get; set; } = 0; // Oscillations per second
    public double FrequencyMax { get; set; } = 0; // Oscillations per second
    public double FrequencyStep { get; set; } = 0; // Oscillations per second
    public double NewFrequency { get; set; } = 0; // to update when value equal to Frequency
    public double Offset { get; set; } = 0; // offset from 0th millisecond
    public double Value { get; set; } = 0;
    public double NewValue { get; set; }
}
