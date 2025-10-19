public class ItemDto
{
    public string? Id { get; set; }
    public int Weight { get; set; }
    public int Value { get; set; }
}

public class KnapsackItem : ItemDto
{
    public string Name { get; set; } = "Item";
}

public class KsChallengeCreationReqDto
{
    public string Difficulty { get; set; }
    public List<ItemDto> Items { get; set; }
    public int MaxCapacity { get; set; }
    public int MaxDuration { get; set; }
    public int MissCount { get; set; }
    public string CreatedBy { get; set; }
}

public class KsChallengeUpdateReqDto
{
    public string? Difficulty { get; set; }
    public int? MaxCapacity { get; set; }
    public int? MaxDuration { get; set; }
    public string? UpdatedBy { get; set; }
}

public class MissCellDto
{
    public int X { get; set; }
    public int Y { get; set; }
}