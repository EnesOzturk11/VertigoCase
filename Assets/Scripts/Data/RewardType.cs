namespace VertigoCase.Data
{
    // Type of a wheel slice. Adding a new type is a single line here and does not
    // change existing code (SOLID "Open/Closed"). Bomb is just another reward type.
    public enum RewardType
    {
        Cash,
        Gold,
        Chest,
        Weapon,
        Bomb
    }
}
