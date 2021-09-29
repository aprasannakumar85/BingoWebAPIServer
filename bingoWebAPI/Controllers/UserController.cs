using System;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using bingoWebAPI.Connection;
using bingoWebAPI.Models;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Http;
using NSwag.Annotations;

namespace bingoWebAPI.Controllers
{
  [OpenApiTag("UserController", Description = "Endpoints to set/retrieve data from the UserController.")]
  [ApiController]
  public class UserController : Controller
  {
    private readonly IFirebaseClient _firebaseClient;
    private readonly ValidateConnection _validateConnection;

    public UserController(IFirebaseClient firebaseClient, ValidateConnection validateConnection)
    {
      _firebaseClient = firebaseClient;
      _validateConnection = validateConnection;
    }

    [Description("Insert Player.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpPost("insertPlayer/{secret}")]
    public  async Task<IActionResult> InsertPlayer(string secret, [FromQuery] string wohoo, [FromBody] PlayerModel player)
    {
      try
      {
        var decryptedSecret = EncryptDecrypt.DecryptStringAES(secret, string.Empty);

        if (!_validateConnection.ValidateToken(decryptedSecret))
        {
          return BadRequest("invalid token");
        }

        var adminData = await _firebaseClient.GetAsync("admin");
        var adminModels = adminData.ResultAs<Dictionary<string, AdminModel>>();

        if (adminModels == null)
        {
          return BadRequest("no admin account exists!");
        }

        foreach (var adminModel in adminModels)
        {
          var adminModelValue = adminModel.Value;
          if (adminModelValue.employerName.Equals(player.employerName) && adminModelValue.teamName.Equals(player.teamName))
          {
            await _firebaseClient.SetAsync($"players/{player.employerName}/{player.teamName}/{player.uniqueId}", player);
            return Ok();
          }
        }
        return BadRequest("no admin account exists!");
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }

    }

    [Description("Get Players.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpGet("getPlayers/{secret}")]
    public  async Task<IActionResult> GetPlayers(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName)
    {
      try
      {
        var decryptedSecret = EncryptDecrypt.DecryptStringAES(secret, string.Empty);

        if (!_validateConnection.ValidateToken(decryptedSecret))
        {
          return BadRequest("invalid token");
        }

        var decryptedEmployer = EncryptDecrypt.DecryptStringAES(employerName, wohoo);
        var decryptedTeam = EncryptDecrypt.DecryptStringAES(teamName, wohoo);

        var data = await _firebaseClient.GetAsync($"players/{decryptedEmployer}/{decryptedTeam}");

        var playerModels = data.ResultAs<Dictionary<string, PlayerModel>>();

        if (playerModels == null)
        {
          return BadRequest("no admin account exists!");
        }

        return Json(playerModels.Values);
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }
  }
}
