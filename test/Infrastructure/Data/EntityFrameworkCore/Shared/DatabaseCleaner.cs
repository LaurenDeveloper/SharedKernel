using Microsoft.EntityFrameworkCore;

namespace SharedKernel.Integration.Tests.Data.EntityFrameworkCore.Shared
{
    public static class DatabaseCleaner
    {
        public static void Invoke(DbContext context)
        {
            context?.Database.EnsureDeleted();
        }
    }
}