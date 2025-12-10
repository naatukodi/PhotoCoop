namespace PhotoCoop.Domain.Users;

public enum UserType
{
    Customer = 1,
    Photographer = 2,
    Admin = 3,
    Manager = 4
}

public enum OccasionType
{
    Wedding = 1,
    Engagement = 2,
    Birthday = 3,
    CorporateEvent = 4,
    BabyShower = 5,
    Other = 99
}

public enum BookingStatus
{
    New = 1,
    Shortlisted = 2,
    Assigned = 3,
    AcceptedByPhotographer = 4,
    RejectedByPhotographer = 5,
    InProgress = 6,
    Completed = 7,
    CancelledByCustomer = 8,
    CancelledByAdmin = 9
}
