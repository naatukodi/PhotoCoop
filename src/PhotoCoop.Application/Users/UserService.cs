using PhotoCoop.Domain.Common;
using PhotoCoop.Domain.Users;

namespace PhotoCoop.Application.Users;

public interface IUserService
{
    Task<User> CreateCustomerAsync(CreateCustomerUserRequest request, CancellationToken cancellationToken = default);
    Task<User> CreatePhotographerAsync(CreatePhotographerUserRequest request, CancellationToken cancellationToken = default);
    Task<User> CreateAdminAsync(CreateAdminUserRequest request, CancellationToken cancellationToken = default);

    Task<User?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetPhotographersByPincodeAndOccasionAsync(string pincode, OccasionType occasion, CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    // You can inject password hashing / identity later

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> CreateCustomerAsync(CreateCustomerUserRequest request, CancellationToken cancellationToken = default)
    {
        // basic uniqueness check (can be improved later)
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("Email already exists.");

        var user = new User(UserType.Customer, request.FullName, request.Email, request.PhoneNumber);

        foreach (var pc in request.Pincodes.Distinct())
            user.AddPincode(pc);

        Domain.Common.Address? address = null;
        if (!string.IsNullOrWhiteSpace(request.AddressLine1) &&
            !string.IsNullOrWhiteSpace(request.City) &&
            !string.IsNullOrWhiteSpace(request.State) &&
            !string.IsNullOrWhiteSpace(request.Pincode))
        {
            address = new Domain.Common.Address(
                request.AddressLine1!,
                request.City!,
                request.State!,
                request.Pincode!);
        }

        var custProfile = new CustomerProfile(address);
        foreach (var occ in request.PreferredOccasions.Distinct())
            custProfile.AddPreferredOccasion(occ);

        user.SetCustomerProfile(custProfile);

        return await _userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<User> CreatePhotographerAsync(CreatePhotographerUserRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("Email already exists.");

        var user = new User(UserType.Photographer, request.FullName, request.Email, request.PhoneNumber);

        foreach (var pc in request.Pincodes.Distinct())
            user.AddPincode(pc);

        var profile = new PhotographerProfile(request.DisplayName, request.YearsOfExperience);

        if (!string.IsNullOrWhiteSpace(request.Bio))
            typeof(PhotographerProfile)
                .GetProperty("Bio")?
                .SetValue(profile, request.Bio);

        if (!string.IsNullOrWhiteSpace(request.PortfolioUrl))
            typeof(PhotographerProfile)
                .GetProperty("PortfolioUrl")?
                .SetValue(profile, request.PortfolioUrl);

        foreach (var occ in request.Occasions.Distinct())
            profile.AddOccasion(occ);

        foreach (var rc in request.RateCards)
        {
            var rateCard = new RateCard(rc.Occasion, rc.PackageName, rc.BasePrice, rc.PricePerHour, rc.PricePerDay);
            profile.AddRateCard(rateCard);
        }

        user.SetPhotographerProfile(profile);

        return await _userRepository.AddAsync(user, cancellationToken);
    }

    public async Task<User> CreateAdminAsync(CreateAdminUserRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existing != null)
            throw new InvalidOperationException("Email already exists.");

        var userType = request.IsSuperAdmin ? UserType.Admin : UserType.Manager;
        var user = new User(userType, request.FullName, request.Email, request.PhoneNumber);

        var adminProfile = new AdminProfile(request.IsSuperAdmin);
        foreach (var pc in request.ManagedPincodes.Distinct())
            adminProfile.AddManagedPincode(pc);

        user.SetAdminProfile(adminProfile);

        return await _userRepository.AddAsync(user, cancellationToken);
    }

    public Task<User?> GetUserByIdAsync(string id, CancellationToken cancellationToken = default)
        => _userRepository.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<User>> GetPhotographersByPincodeAndOccasionAsync(
        string pincode, OccasionType occasion, CancellationToken cancellationToken = default)
        => _userRepository.GetPhotographersByPincodeAndOccasionAsync(pincode, occasion, cancellationToken);
}
