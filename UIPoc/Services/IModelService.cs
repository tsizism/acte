using UIPooc.Models;

namespace UIPooc.Services
{
    public interface IModelService
    {
        // User operations
        Task<User?> GetCurrentUserAsync();
        Task<User?> GetUserByIdAsync(int userId);
        Task<List<User>> GetAllUsersAsync();

        // Holding operations
        Task<Holding?> GetHoldingByIdAsync(int holdingId);
        Task<Holding?> GetHoldingByNameAsync(string name);
        Task<List<Holding>> GetHoldingsByUserIdAsync(int userId);
        Task<List<Holding>> GetAllHoldingsAsync();
        Task<Holding> CreateHoldingAsync(Holding holding);
        Task<Holding> UpdateHoldingAsync(Holding holding);
        Task DeleteHoldingAsync(int holdingId);

        // Equity operations
        Task<Equity?> GetEquityByIdAsync(int equityId);
        Task<List<Equity>> GetEquitiesByHoldingIdAsync(int holdingId);
        Task<List<Equity>> GetEquitiesBySymbolAsync(string symbol);
        Task<Equity> CreateEquityAsync(Equity equity);
        Task<Equity> UpdateEquityAsync(Equity equity);
        Task DeleteEquityAsync(int equityId);

        // Transaction operations
        Task<Transaction?> GetTransactionByIdAsync(int transactionId);
        Task<List<Transaction>> GetTransactionsByHoldingIdAsync(int holdingId);
        Task<List<Transaction>> GetTransactionsByUserIdAsync(int userId);
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<Transaction> UpdateTransactionAsync(Transaction transaction);
        Task DeleteTransactionAsync(int transactionId);

        // IndexHistory operations
        Task<IndexHistory?> GetIndexHistoryByIdAsync(int indexHistoryId);
        Task<List<IndexHistory>> GetIndexHistoriesByHoldingIdAsync(int holdingId);
        Task<IndexHistory> CreateIndexHistoryAsync(IndexHistory indexHistory);
        Task<List<IndexHistory>> GetIndexHistoriesForDateRangeAsync(int holdingId, DateTime startDate, DateTime endDate);

        // Aggregation operations
        Task<decimal> GetTotalPortfolioValueAsync(int holdingId);
        Task<int> GetEquityCountAsync(int holdingId);
        Task<Dictionary<string, int>> GetEquityCountByMarketAsync(int holdingId);
    }
}
