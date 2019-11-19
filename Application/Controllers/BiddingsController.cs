using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Helpers;
using Application.Models.DataTransferObjects;
using Application.Models.Entities;
using Application.Repositories;
using Application.Services;
using Application.Utility;
using Application.Utility.Exception;
using Application.Utility.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;

namespace Application.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class BiddingsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IBidRepository _bidRepository;
        private readonly IBidService _bidService;

        public BiddingsController(IBidService bidService, IBidRepository bidRepository, IMapper mapper)
        {
            _bidService = bidService;
            _bidRepository = bidRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Opens a bid on a given project
        /// </summary>
        /// <param name="bidDto">BidDto contains information about the project, how much the freelancer wants and so on</param>
        /// <returns>A ok confirmation with the created bid object</returns>
        /// <response code="200">Returns the newly created bid</response>
        /// <response code="400">Badrequest for a wrong bidDto or invalid fields. The return message should show what went wrong</response>
        /// <response code="404">The User was not found</response>
        [HttpPost]
        public async Task<IActionResult> OpenBid([FromBody] BidCreationDto bidDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var createBid = _mapper.Map<Bid>(bidDto);
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Name);
                if (bidDto.FreelancerId != userId)
                    throw new InvalidBid("User doesn't have access to this bid");

                var createdBid = await _bidService.Open(createBid);
                return Ok(createdBid);
            }
            catch (UserNotFound)
            {
                return NotFound(new MessageObj("User not found"));
            }
            catch (EnvironmentNotSet)
            {
                throw;
            }
            catch (Exception e)
            {
                return BadRequest(new MessageObj(e.Message));
            }
        }

        /// <summary>
        /// An employer can accept a bid
        /// </summary>
        /// <param name="bidId">The id of the bid wanted to accept</param>
        /// <param name="bidDto">The object itself</param>
        /// <returns>An empty OK if successful</returns>
        /// <response code="200">Empty ok if successfully accepted</response>
        /// <response code="400">Badrequest for a wrong bidDto or invalid fields. The return message should show what went wrong</response>
        [HttpPut("{bidId}")]
        public async Task<IActionResult> AcceptBid([FromHeader] string authorization, [FromRoute] string bidId, [FromBody] BidUpdateDto bidDto)
        {
            if (bidId != bidDto.Id)
                return BadRequest(new MessageObj("Invalid id(s)"));

            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var updateBid = _mapper.Map<Bid>(bidDto);
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.Name);

                if (await _bidService.Accept(updateBid, authorization.Split(' ') [1], userId))
                    return Ok();
                throw new InvalidBid();
            }
            catch (Exception e)
            {
                return BadRequest(new MessageObj(e.Message));
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var bids = await _bidRepository.Get();
            var bidDtos = _mapper.Map<IList<BidDto>>(bids);
            return Ok(bidDtos);
        }

        [HttpGet("freelancer/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByFreelancerId(string id)
        {
            var bids = await _bidRepository.GetByFreelancerId(id);
            var bidsDto = _mapper.Map<IList<BidDto>>(bids);
            return Ok(bidsDto);
        }

        [HttpGet("project/{projectId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProjectId([FromRoute] string projectId)
        {
            var bids = await _bidRepository.GetByProjectId(projectId);
            var bidDtos = _mapper.Map<IList<BidDto>>(bids);
            return Ok(bidDtos);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var bid = await _bidRepository.GetById(id);
            var bidDto = _mapper.Map<BidDto>(bid);
            return Ok(bidDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                var bid = await _bidRepository.GetById(id);

                if (bid.FreelancerId != User.FindFirstValue(ClaimTypes.Name))
                    throw new ArgumentException("User doesn't have access to this resource");

                await _bidRepository.Remove(id);
            }
            catch (Exception e)
            {
                return BadRequest(new MessageObj(e.Message));
            }

            return Ok();
        }
    }
}