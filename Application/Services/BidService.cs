using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Models.DataTransferObjects;
using Application.Models.Entities;
using Application.Repositories;
using Application.Utility.ClientLibrary;
using Application.Utility.ClientLibrary.Project;
using Application.Utility.Exception;
using Application.Utility.Models;
using Newtonsoft.Json;

namespace Application.Services
{
    public interface IBidService
    {
        Task<Bid> Open(Bid bid);
        Task<bool> Accept(Bid bid, string authorizationToken, string userId);
    }

    public class BidService : IBidService
    {
        private readonly IBidRepository _bidRepository;
        private readonly IClient _client;

        public BidService(IBidRepository bidRepository, IClient client)
        {
            _bidRepository = bidRepository;
            _client = client;
        }

        public async Task<Bid> Open(Bid bid)
        {
            var project = await _client.GetProjectAsync(bid.ProjectId);
            if (project == null) throw new InvalidBid();

            var existingBid = await _bidRepository.GetByFreelancerId(bid.FreelancerId);
            if (existingBid != null)
                throw new InvalidBid("User already has a bid pending");

            var createdBid = await _bidRepository.Create(bid);

            return createdBid ??
                throw new InvalidBid();
        }

        public async Task<bool> Accept(Bid bid, string authorizationToken, string userId)
        {
            var project = await _client.GetProjectAsync(bid.ProjectId);
            if (project == null)
                throw new InvalidBid("projectId invalid");

            if (project.OwnerId != userId)
                throw new InvalidBid("User doesn't have permission to accept this bid");

            if (!string.IsNullOrEmpty(project.FreelancerId))
                throw new InvalidBid("Project cannot be accepted as a freelancer has already been chosen");

            project.FreelancerId = bid.FreelancerId;

            return await _client.UpdateProjectAsync(authorizationToken, project);
        }
    }
}