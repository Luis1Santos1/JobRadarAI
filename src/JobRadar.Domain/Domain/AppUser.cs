using JobRadar.SharedKernel;

namespace JobRadar.Domain.Users;

public sealed class AppUser : BaseEntity
{
    private AppUser()
    {
    }

    public AppUser(string name, string email)
    {
        Name = name;
        Email = email;
        IsActive = true;
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public void Update(string name, string email)
    {
        Name = name;
        Email = email;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}