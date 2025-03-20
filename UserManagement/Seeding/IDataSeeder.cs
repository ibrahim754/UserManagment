namespace UserManagement.Seeding
{
    public interface IDataSeeder
    {
        Task SeedAsync();
        int OrderOfExecution { get; }
    }
}
