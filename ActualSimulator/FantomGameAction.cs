using FantomMapLibrary;
namespace ActualSimulator;

public struct FantomGameAction
{
    //public List<(INode? From, INode To)> Moves;
    //public List<Transport?> UsedTransports;
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

    public override readonly bool Equals(object obj)
    {
        if (obj is FantomGameAction otherAction)
        {
            return Moves.Count == otherAction.Moves.Count &&
                   Moves.Zip(otherAction.Moves, (move1, move2) =>
                       move1.NewPosition == move2.NewPosition && move1.Tr == move2.Tr).All(x => x);
        }
        return false;
    }

}
