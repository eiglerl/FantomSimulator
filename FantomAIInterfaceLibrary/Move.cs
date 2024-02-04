namespace FantomMapLibrary;

public struct Move
{
    public int NewPosition;
    public Transport Tr;
    public Move(int p, Transport t) { NewPosition = p; Tr = t; }
    public Move(int p) { NewPosition = p; Tr = Transport.Nothing; }
    public Move(Transport tra) { NewPosition = 0; Tr = tra; }

    public static Move Invalid() => new() { NewPosition = 0, Tr = Transport.Nothing };
    public void Invalidate() { NewPosition = 0; Tr = Transport.Nothing; }

    public bool IsValid() => ContainsPosition() && ContainsTransport();
    public bool ContainsPosition() => NewPosition != 0;
    public bool ContainsTransport() => Tr != Transport.Nothing;
}
