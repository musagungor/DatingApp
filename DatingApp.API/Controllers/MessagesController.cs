using System.Security.Claims;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using System;

namespace DatingApp.API.Controllers
{

    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]

    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repository;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repository, IMapper mapper)
        {
            this._mapper = mapper;
            this._repository = repository;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var messagesFromRepo = await _repository.GetMessage(id);

            if (messagesFromRepo == null)
            {
                return NotFound();
            }

            return Ok(messagesFromRepo);

        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {

            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            messageForCreationDto.SenderId=userId;
            var recipientId = await _repository.GetUserAsync(messageForCreationDto.RecipientId);
             
             if (recipientId == null)
             {
                 return BadRequest("Could not find user");
             }

             var message = _mapper.Map<Message>(messageForCreationDto);

             _repository.Add(message);

             var messageToReturn = _mapper.Map<MessageForCreationDto>(message);

             if (await _repository.SaveAllAsync())
             {
                 return CreatedAtRoute("GetMessage", new {id=message.Id}, messageToReturn);
             }

             throw new Exception("Creating the message failed on save");

        }

    }
}