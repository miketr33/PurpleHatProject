using PurpleHatProject.Models;

namespace PurpleHatProject.Services;

public interface IUserSessionService
{
    User? CurrentUser { get; }
    void Login(User user);
    void Logout();
    event Action? OnChange;
}

public class UserSessionService : IUserSessionService
{
    public User? CurrentUser { get; private set; }

    public event Action? OnChange;

    public void Login(User user)
    {
        CurrentUser = user;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        CurrentUser = null;
        OnChange?.Invoke();
    }
}
