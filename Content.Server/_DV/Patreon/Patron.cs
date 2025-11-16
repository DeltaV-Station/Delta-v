namespace Content.Server._DV.Patreon;
public sealed class Patron
{
    public string Name { get; set; }

    public string Tier { get; set; }

    public Patron(string name, string tier)
    {
        Name = name;
        Tier = tier;
    }
}
