using Employee.Infrastructure.Data;
using Employee.Infrastructure.Messaging;
using Employee.Infrastructure.Messaging.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Employee.Infrastructure.Repositories
{
    /// <summary>
    /// Outbox repository implementation for storing and retrieving outbox messages
    /// </summary>
    public class OutboxRepository : IOutboxRepository
    {
        private readonly EmployeeDbContext _context;
        private readonly ILogger<OutboxRepository> _logger;

        public OutboxRepository(EmployeeDbContext context, ILogger<OutboxRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.OutboxMessages.AddAsync(message, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Outbox message added: {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding outbox message");
                throw;
            }
        }

        public async Task AddBatchAsync(IEnumerable<OutboxMessage> messages, CancellationToken cancellationToken = default)
        {
            try
            {
                await _context.OutboxMessages.AddRangeAsync(messages, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Batch of {Count} outbox messages added", messages.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding batch of outbox messages");
                throw;
            }
        }

        public async Task UpdateAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Outbox message updated: {MessageId}", message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating outbox message");
                throw;
            }
        }

        public async Task<IEnumerable<OutboxMessage>> GetUnprocessedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.OutboxMessages
                    .Where(m => !m.IsProcessed && m.RetryCount < 5)
                    .OrderBy(m => m.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving unprocessed outbox messages");
                throw;
            }
        }

        public async Task<OutboxMessage> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.OutboxMessages
                    .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving outbox message by ID: {MessageId}", id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var message = await GetByIdAsync(id, cancellationToken);
                if (message != null)
                {
                    _context.OutboxMessages.Remove(message);
                    await _context.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Outbox message deleted: {MessageId}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting outbox message");
                throw;
            }
        }
    }
}
