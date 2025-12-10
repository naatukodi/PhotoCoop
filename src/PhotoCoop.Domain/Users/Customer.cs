using PhotoCoop.Domain.Common;

namespace PhotoCoop.Domain.Users;

public class CustomerProfile
{
    public Address? DefaultAddress { get; private set; }
    public List<OccasionType> PreferredOccasions { get; private set; } = new();

    private CustomerProfile() { }

    public CustomerProfile(Address? defaultAddress = null)
    {
        DefaultAddress = defaultAddress;
    }

    public void AddPreferredOccasion(OccasionType occasion)
    {
        if (!PreferredOccasions.Contains(occasion))
            PreferredOccasions.Add(occasion);
    }
}
