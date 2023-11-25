namespace FantomMapLibrary;

public struct Move
{
    public int pos;
    public Transport tr;
    public Move(int p, Transport t) { pos = p; tr = t; }
    public Move(int p) { pos = p; tr = Transport.Nothing; }
    public Move(Transport tra) { pos = 0; tr = tra; }

    public static Move Invalid() => new() { pos = 0, tr = Transport.Nothing };
    public void Invalidate() { pos = 0; tr = Transport.Nothing; }

    public bool IsValid() => ContainsPosition() && ContainsTransport();
    public bool ContainsPosition() => pos != 0;
    public bool ContainsTransport() => tr != Transport.Nothing;
}
