using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Database;
using Application.Models.Entities;
using MongoDB.Driver;

namespace Application.Repositories
{
    public interface IBidRepository
    {
        Task<List<Bid>> Get();
        Task<Bid> GetById(string id);
        Task<List<Bid>> GetByProject(string projectId);
        Task<List<Bid>> GetByFreelancerId(string freelancerId);
        Task<Bid> GetByProjectAndFreelancer(string projectId, string freelancerId);
        Task<Bid> Create(Bid bid);
        Task Update(string id, Bid bidIn);
        Task Remove(Bid bidIn);
        Task Remove(string id);
        Task<List<Bid>> GetByProjectId(string projectId);
    }

    public class BidRepository : IBidRepository
    {
        private readonly IMongoCollection<Bid> _bids;

        public BidRepository(IDatabaseContext dbContext)
        {
            if (dbContext.IsConnectionOpen())
                _bids = dbContext.Bids;
        }

        public async Task<List<Bid>> Get() => await (await _bids.FindAsync(bid => true)).ToListAsync();
        public async Task<Bid> GetById(string id) => await (await _bids.FindAsync(bidding => bidding.Id == id)).FirstOrDefaultAsync();
        public async Task<List<Bid>> GetByProject(string projectId) => await (await _bids.FindAsync(bid => bid.ProjectId == projectId)).ToListAsync();
        public async Task<List<Bid>> GetByFreelancerId(string freelancerId) => await (await _bids.FindAsync(bid => bid.FreelancerId == freelancerId)).ToListAsync();

        public async Task<Bid> GetByProjectAndFreelancer(string projectId, string freelancerId) => await (
            await _bids.FindAsync(
                bid => bid.ProjectId == projectId && bid.FreelancerId == freelancerId)
        ).FirstOrDefaultAsync();

        public async Task<Bid> Create(Bid bid)
        {
            await _bids.InsertOneAsync(bid);
            return bid;
        }

        public async Task Update(string id, Bid bidIn) => await _bids.ReplaceOneAsync(bidding => bidding.Id == id, bidIn);
        public async Task Remove(Bid bidIn) => await _bids.DeleteOneAsync(bidding => bidding.Id == bidIn.Id);
        public async Task Remove(string id) => await _bids.DeleteOneAsync(bidding => bidding.Id == id);
        public async Task<List<Bid>> GetByProjectId(string projectId) => await (await _bids.FindAsync(bid => bid.ProjectId == projectId)).ToListAsync();
    }
}