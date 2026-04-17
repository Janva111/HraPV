public class Room
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> Exits { get; set; } = new();
    public List<string> Items { get; set; } = new();
    public Dictionary<string, string> NPCs { get; set; } = new();

    public Room() { }

    public Room(string name, string description)
    {
        Name = name;
        Description = description;
    }
}