using Microsoft.EntityFrameworkCore;
using UIPooc.Data;
using UIPooc.Models;

namespace UIPooc.Services
{
    public class ModelService : IModelService
    {
        private readonly HoldingsDbContext _context;

        public ModelService(HoldingsDbContext context)
        {
            _context = context;
        }

        #region User Operations

        public async Task<User?> GetCurrentUserAsync()
        {
            return await _context.Users.FirstOrDefaultAsync();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Holdings)
                .Include(u => u.Transactions)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Holdings)
                .ToListAsync();
        }

        #endregion

        #region Holding Operations

        public async Task<Holding?> GetHoldingByIdAsync(int holdingId)
        {
            return await _context.Holdings
                .Include(h => h.Equities)
                //.Include(h => h.Transactions)
                .Include(h => h.IndexHistories)
                .FirstOrDefaultAsync(h => h.HoldingId == holdingId);
        }

        public async Task<Holding?> GetHoldingByNameAsync(string name)
        {
            if (_context.Holdings == null || !_context.Holdings.Any())
                return null;

            return await _context.Holdings
                .Include(h => h.Equities)
                .FirstOrDefaultAsync(h => h.Name == name);
        }

        public async Task<List<Holding>> GetHoldingsByUserIdAsync(int userId)
        {
            return await _context.Holdings
                .Include(h => h.Equities)
                .Where(h => h.UserId == userId)
                .OrderBy(h => h.Name)
                .ToListAsync();
        }

        public async Task<List<Holding>> GetAllHoldingsAsync()
        {
            if (_context.Holdings == null || !_context.Holdings.Any())
                return new List<Holding>();

            return await _context.Holdings
                .Include(h => h.Equities)
                .OrderBy(h => h.Name)
                .ToListAsync();
        }

        public async Task<Holding> CreateHoldingAsync(Holding holding)
        {
            _context.Holdings.Add(holding);
            await _context.SaveChangesAsync();
            return holding;
        }

        public async Task<Holding> UpdateHoldingAsync(Holding holding)
        {
            _context.Holdings.Update(holding);
            await _context.SaveChangesAsync();
            return holding;
        }

        public async Task DeleteHoldingAsync(int holdingId)
        {
            var holding = await _context.Holdings.FindAsync(holdingId);
            if (holding != null)
            {
                _context.Holdings.Remove(holding);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Equity Operations

        public async Task<Equity?> GetEquityByIdAsync(int equityId)
        {
            return await _context.Equities
                .Include(e => e.Holding)
                .FirstOrDefaultAsync(e => e.EquityId == equityId);
        }

        public async Task<List<Equity>> GetEquitiesByHoldingIdAsync(int holdingId)
        {
            return await _context.Equities
                .Where(e => e.HoldingId == holdingId)
                .OrderBy(e => e.Symbol)
                .ToListAsync();
        }

        public async Task<List<Equity>> GetEquitiesBySymbolAsync(string symbol)
        {
            return await _context.Equities
                .Include(e => e.Holding)
                .Where(e => e.Symbol == symbol)
                .ToListAsync();
        }

        public async Task<Equity> CreateEquityAsync(Equity equity)
        {
            _context.Equities.Add(equity);
            await _context.SaveChangesAsync();
            return equity;
        }

        public async Task<Equity> UpdateEquityAsync(Equity equity)
        {
            _context.Equities.Update(equity);
            await _context.SaveChangesAsync();
            return equity;
        }

        public async Task DeleteEquityAsync(int equityId)
        {
            var equity = await _context.Equities.FindAsync(equityId);
            if (equity != null)
            {
                _context.Equities.Remove(equity);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region Transaction Operations

        public async Task<Transaction?> GetTransactionByIdAsync(int transactionId)
        {
            return await _context.Transactions
                .Include(t => t.User)
                .Include(t => t.Holding)
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId);
        }

        public async Task<List<Transaction>> GetTransactionsByHoldingIdAsync(int holdingId)
        {
            return await _context.Transactions
                .Where(t => t.HoldingId == holdingId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<Transaction>> GetTransactionsByUserIdAsync(int userId)
        {
            return await _context.Transactions
                .Include(t => t.Holding)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction> UpdateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Update(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task DeleteTransactionAsync(int transactionId)
        {
            var transaction = await _context.Transactions.FindAsync(transactionId);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
            }
        }

        #endregion

        #region IndexHistory Operations

        public async Task<IndexHistory?> GetIndexHistoryByIdAsync(int indexHistoryId)
        {
            return await _context.IndexHistories
                .Include(i => i.Holding)
                .FirstOrDefaultAsync(i => i.IndexHistoryId == indexHistoryId);
        }

        public async Task<List<IndexHistory>> GetIndexHistoriesByHoldingIdAsync(int holdingId)
        {
            return await _context.IndexHistories
                .Where(i => i.HoldingId == holdingId)
                .OrderByDescending(i => i.RecordedAt)
                .ToListAsync();
        }

        public async Task<IndexHistory> CreateIndexHistoryAsync(IndexHistory indexHistory)
        {
            _context.IndexHistories.Add(indexHistory);
            await _context.SaveChangesAsync();
            return indexHistory;
        }

        public async Task<List<IndexHistory>> GetIndexHistoriesForDateRangeAsync(int holdingId, DateTime startDate, DateTime endDate)
        {
            return await _context.IndexHistories
                .Where(i => i.HoldingId == holdingId 
                    && i.RecordedAt >= startDate 
                    && i.RecordedAt <= endDate)
                .OrderBy(i => i.RecordedAt)
                .ToListAsync();
        }

        #endregion

        #region Aggregation Operations

        public async Task<decimal> GetTotalPortfolioValueAsync(int holdingId)
        {
            var equities = await _context.Equities
                .Where(e => e.HoldingId == holdingId)
                .ToListAsync();

            return equities.Sum(e => e.Quantity * e.CurrentPrice);
        }

        public async Task<int> GetEquityCountAsync(int holdingId)
        {
            return await _context.Equities
                .Where(e => e.HoldingId == holdingId)
                .CountAsync();
        }

        public async Task<Dictionary<string, int>> GetEquityCountByMarketAsync(int holdingId)
        {
            return await _context.Equities
                .Where(e => e.HoldingId == holdingId)
                .GroupBy(e => e.Market)
                .Select(g => new { Market = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Market, x => x.Count);
        }

        #endregion
    }
}
