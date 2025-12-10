using PhotoCoop.Domain.Common;

namespace PhotoCoop.Domain.Users;

public class User : Entity
{
    public UserType UserType { get; private set; }

    public string FullName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }

    public bool IsActive { get; private set; } = true;

    public string? ProfileImageUrl { get; private set; }

    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? LastLoginAtUtc { get; private set; }

    public List<string> Pincodes { get; private set; } = new(); // simple pincode array

    // type-specific sub-objects
    public PhotographerProfile? PhotographerProfile { get; private set; }
    public CustomerProfile? CustomerProfile { get; private set; }
    public AdminProfile? AdminProfile { get; private set; }

    private User() { } // for deserialization

    public User(UserType userType, string fullName, string email, string phoneNumber)
    {
        UserType = userType;
        FullName = fullName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public void AddPincode(string pincode)
    {
        if (!Pincodes.Contains(pincode))
            Pincodes.Add(pincode);
    }

    public void SetPhotographerProfile(PhotographerProfile profile)
    {
        if (UserType != UserType.Photographer)
            throw new InvalidOperationException("User is not a photographer.");

        PhotographerProfile = profile;
    }

    public void SetCustomerProfile(CustomerProfile profile)
    {
        if (UserType != UserType.Customer)
            throw new InvalidOperationException("User is not a customer.");

        CustomerProfile = profile;
    }

    public void SetAdminProfile(AdminProfile profile)
    {
        if (UserType != UserType.Admin && UserType != UserType.Manager)
            throw new InvalidOperationException("User is not admin/manager.");

        AdminProfile = profile;
    }
}
