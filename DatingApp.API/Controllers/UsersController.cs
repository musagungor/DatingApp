using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IDatingRepository repository, 
            IMapper mapper, 
            ILogger<UsersController> logger)
        {   
            this._repository = repository;
            this._mapper = mapper;
            this._logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync([FromQuery]UserParams userParams)
        {
             _logger.LogInformation("Get Users Async");
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var userFromRepo = await _repository.GetUserAsync(currentUserId);
            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender= userFromRepo.Gender =="male"?"female":"male";
            }

            var users = await _repository.GetUsersAsync(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(users);

            Response.AddPagination(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);

            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name ="GetUser")]
        public async Task<IActionResult> GetUserAsync(int id)
        {
             _logger.LogInformation("Get User Async");

            var user = await _repository.GetUserAsync(id);

            var userToReturn = _mapper.Map<UserForDetailedDto>(user);

            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userForUpdateDto)
        {
            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var userFromRepo = await _repository.GetUserAsync(id);
            _mapper.Map(userForUpdateDto,userFromRepo);

            if (await _repository.SaveAllAsync())
            {
                return NoContent();
            }

            throw new Exception($"Updating user {id} failed on save ");

        }

        [HttpPost("{id}/like/{recepientId}")]
        public async Task<IActionResult> LikeUser(int id, int recepientId) {

             if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var like = await _repository.GetLike(id,recepientId);

            if (like != null)
            {
                return BadRequest("You allready like this user");
            }

            if (await _repository.GetUserAsync(recepientId) == null)
            {
                return NotFound();
            }

            like = new Like{
                LikerId = id,
                LikeeId = recepientId
            };

            _repository.Add<Like>(like);

            if (await _repository.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("Failed to like user");

        }

    }
}