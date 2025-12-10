namespace PhotoCoop.Domain.Users;

public class AdminProfile
{
    public bool IsSuperAdmin { get; private set; }
    public List<string> ManagedPincodes { get; private set; } = new();

    private AdminProfile() { }

    public AdminProfile(bool isSuperAdmin)
    {
        IsSuperAdmin = isSuperAdmin;
    }

    public void AddManagedPincode(string pincode)
    {
        if (!ManagedPincodes.Contains(pincode))
            ManagedPincodes.Add(pincode);
    }
}