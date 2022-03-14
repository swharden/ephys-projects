namespace Report2P.PvXml;

public struct Laser
{
    public readonly int Index;
    public readonly string Name;
    public double Power;
    public bool IsOn => Power > 0;
    public bool IsOff => Power == 0;
    public string PowerStatus => IsOn ? $"ON, Power={Power}" : "Off";

    public Laser(int index, string name, double power)
    {
        Index = index;
        Name = name;
        Power = power;
    }

    public override string ToString() =>
        $"Laser {Index}: {Name} ({PowerStatus})";
}