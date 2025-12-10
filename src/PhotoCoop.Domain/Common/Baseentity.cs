namespace PhotoCoop.Domain.Common;

public abstract class Entity
{
    public string Id { get; protected set; } = Guid.NewGuid().ToString();
}

public class Address
{
    public string Line1 { get; private set; }
    public string? Line2 { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Pincode { get; private set; }
    public string Country { get; private set; } = "India";

    private Address() { } // for serialization

    public Address(string line1, string city, string state, string pincode, string? line2 = null, string? country = "India")
    {
        Line1 = line1;
        City = city;
        State = state;
        Pincode = pincode;
        Line2 = line2;
        Country = country ?? "India";
    }
}
