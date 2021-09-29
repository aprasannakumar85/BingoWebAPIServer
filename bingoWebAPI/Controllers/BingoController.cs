using System;
using System.ComponentModel;
using System.Threading.Tasks;
using bingoWebAPI.Connection;
using bingoWebAPI.Models;
using FireSharp.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NSwag.Annotations;

namespace bingoWebAPI.Controllers
{
  [OpenApiTag("BingoController", Description = "Endpoints to set/retrieve data from the BingoController.")]
  [ApiController]
  public class BingoController : Controller
  {
    private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
    private readonly ValidateConnection _validateConnection;
    private readonly IFirebaseClient _firebaseClient;

    public BingoController(IFirebaseClient firebaseClient, IHubContext<BroadcastHub, IHubClient> hubContext, ValidateConnection validateConnection)
    {
      _firebaseClient = firebaseClient;
      _hubContext = hubContext;
      _validateConnection = validateConnection;
    }

    [Description("Send Token to all the listeners.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpGet("sendToken/{secret}")]
    public async Task<IActionResult> SendToken(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName, [FromQuery] string token)
    {
      var decryptedSecret = EncryptDecrypt.DecryptStringAES(secret, string.Empty);

      if (!_validateConnection.ValidateToken(decryptedSecret))
      {
        return BadRequest("invalid token");
      }

      try
      {
        var decryptedEmployer = EncryptDecrypt.DecryptStringAES(employerName, wohoo);
        var decryptedTeam = EncryptDecrypt.DecryptStringAES(teamName, wohoo);

        await _firebaseClient.SetAsync($"tokens/{decryptedEmployer}/{decryptedTeam}/{DateTime.Now:yyyy-MM-ddTHH:mm:ss}", token);
        
        await _hubContext.Clients.All.BroadcastMessage();
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }

      return Ok();
    }

    [Description("Send Token request to admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpGet("sendTokenRequest/{secret}")]
    public async Task<IActionResult> SendTokenRequest(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName, [FromQuery] string playerName, [FromQuery] string uniqueId)
    {
      var decryptedSecret = EncryptDecrypt.DecryptStringAES(secret, string.Empty);

      if (!_validateConnection.ValidateToken(decryptedSecret))
      {
        return BadRequest("invalid token");
      }

      try
      {
        var decryptedEmployer = EncryptDecrypt.DecryptStringAES(employerName, wohoo);
        var decryptedTeam = EncryptDecrypt.DecryptStringAES(teamName, wohoo);
        var decryptedPlayer = EncryptDecrypt.DecryptStringAES(playerName, wohoo);

        var request = new RequestModel
        {
          employerName = decryptedEmployer,
          teamName = decryptedTeam,
          playerName = decryptedPlayer,
          uniqueId = uniqueId,
          message = "requesting a token"
        };

        await _hubContext.Clients.All.BroadcastRequestToken(request);
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }

      return Ok();
    }

    [Description("Send message to admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpGet("notifyAdmin/{secret}")]
    public async Task<IActionResult> NotifyAdmin(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName, [FromQuery] string playerName, [FromQuery] string uniqueId, [FromQuery] string message)
    {
      var decryptedSecret = EncryptDecrypt.DecryptStringAES(secret, string.Empty);

      if (!_validateConnection.ValidateToken(decryptedSecret))
      {
        return BadRequest("invalid token");
      }

      var decryptedEmployer = EncryptDecrypt.DecryptStringAES(employerName, wohoo);
      var decryptedTeam = EncryptDecrypt.DecryptStringAES(teamName, wohoo);
      var decryptedPlayer = EncryptDecrypt.DecryptStringAES(playerName, wohoo);

      try
      {
        var request = new RequestModel
        {
          employerName = decryptedEmployer,
          teamName = decryptedTeam,
          message = message,
          playerName = decryptedPlayer,
          uniqueId = uniqueId
        };

        await _hubContext.Clients.All.BroadcastRequestMessage(request);
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }

      return Ok();
    }

  }
}
