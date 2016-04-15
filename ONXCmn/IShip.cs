namespace ONXCmn.Logic
{


    public interface IBattleble
    {
        int Length { get; }
        Battleground Parent { get; set; }
        Point Position { get; set; }
        ShipOrientation Orientation { get; set; }

        Rectangle GetOwnNeededSpace();
        Rectangle GetTotalNeededSpace();
    }
}