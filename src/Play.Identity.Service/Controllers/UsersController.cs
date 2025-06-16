using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duende.IdentityServer;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Play.Identity.Contracts;
using Play.Identity.Service.Entities;

namespace Play.Identity.Service.Controllers;

[ApiController]
[Route("users")]
[Authorize( Policy = IdentityServerConstants.LocalApi.PolicyName, Roles = Roles.Admin)]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPublishEndpoint  _publishEndpoint;
    public UsersController(UserManager<ApplicationUser> userManager, IPublishEndpoint publishEndpoint)
    {
        _userManager = userManager;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public ActionResult<IEnumerable<UserDto>> Get()
    {
        var users = _userManager.Users
            .ToList()
            .Select(user => user.AsDto());
        return Ok(users); 
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetbyIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }
        return user.AsDto();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateUserDto updateUserDto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }
        user.Email = updateUserDto.Email;
        // by convention of asp net core username is email 
        user.UserName = updateUserDto.Email;
        user.Gil = updateUserDto.Gil;
        await _userManager.UpdateAsync(user);
        // sending as an event
        await _publishEndpoint.Publish( new UserUpdated(
            user.Id, user.Email, user.Gil));
        return NoContent();
    }
        
    // user/{123}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return NotFound();
        }
        await _userManager.DeleteAsync(user);
        await _publishEndpoint.Publish( new UserUpdated(
            user.Id, user.Email, 0));
        return NoContent();
    }
}