namespace DeaneryManagementSystem.Abstractions
{
    public interface IPasswordHasher
    {
        string Hash(string str);
        bool Verify(string hashToCheck, string input);
    }
}