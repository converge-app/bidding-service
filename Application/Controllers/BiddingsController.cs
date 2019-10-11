using System;
using System.Collections.Generic;
using System.Linq;
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

        [HttpPost]
        public async Task<IActionResult> OpenBid([FromBody] BidCreationDto bidDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });

            var createBid = _mapper.Map<Bid>(bidDto);
            try
            {
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
                if (await _bidService.Accept(updateBid, authorization.Split(' ')[1]))
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
            var bidsDto = _mapper.Map<BidDto>(bids);
            return Ok(bidsDto);
        }

        [HttpGet("project/{projectId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByProjectId([FromRoute] string projectId)
        {
            var bids = await _bidRepository.GetByProjectId(projectId);
            var bidDtos = _mapper.Map<BidDto>(bids);
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