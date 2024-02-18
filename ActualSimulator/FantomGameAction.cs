using FantomMapLibrary;
namespace ActualSimulator;

public struct FantomGameAction
{
    public List<Move> Moves;

    public override readonly int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            foreach (var move in Moves)
            {
                hash = hash * 23 + move.NewPosition.GetHashCode();
                hash = hash * 23 + move.Tr.GetHashCode();
            }
            return hash;
        }
    }

    public readonly bool Equals(FantomGameAction other)
    {
        if (Moves.Count != other.Moves.Count)
            return false;

        for (int i = 0; i < Moves.Count; i++)
        {
            if (Moves[i].NewPosition != other.Moves[i].NewPosition || Moves[i].Tr != other.Moves[i].Tr)
                return false;
        }

        return true;
    }

    public override readonly bool Equals(object obj)
    {
        if (obj is FantomGameAction otherAction)
            return Equals(otherAction);
        return false;
    }

}
