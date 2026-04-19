namespace Concert.Services // Заміни Concert на назву свого проєкту
{
    public interface IUserService
    {
        Task<string> GetAvatarUrlAsync(string userName);
    }
}