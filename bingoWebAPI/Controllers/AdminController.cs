using System;
using System.Collections.Generic;
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
  [OpenApiTag("AdminController", Description = "Endpoints to set/retrieve data from the AdminController.")]
  [ApiController]
  public class AdminController : Controller
  {
    private readonly IHubContext<BroadcastHub, IHubClient> _hubContext;
    private readonly IFirebaseClient _firebaseClient;
    private readonly ValidateConnection _validateConnection;

    public AdminController(IFirebaseClient firebaseClient, ValidateConnection validateConnection, IHubContext<BroadcastHub, IHubClient> hubContext)
    {
      _firebaseClient = firebaseClient;
      _validateConnection = validateConnection;
      _hubContext = hubContext;
    }

    [Description("Insert Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpPost("insertAdmin/{secret}")]
    public async Task<IActionResult> InsertAdmin(string secret, [FromQuery] string wohoo, [FromBody] AdminModel adminRequest)
    {
      try
      {
        var decryptedSecret = EncryptDecrypt.DecryptStringAES(secret, string.Empty);

        if (!_validateConnection.ValidateToken(decryptedSecret))
        {
          return BadRequest("invalid token");
        }

        var adminData = _firebaseClient.GetAsync("admin");
        var adminModels = adminData.Result.ResultAs<Dictionary<string, AdminModel>>();

        if (adminModels != null)
        {
          foreach (var adminModel in adminModels)
          {
            var adminModelValue = adminModel.Value;
            if (adminModelValue.userName.Equals(adminRequest.userName) || adminModelValue.displayName.Equals(adminRequest.displayName))
            {
              return BadRequest($"admin account already exists for {adminRequest.employerName} - {adminRequest.teamName}!");
            }

            if (adminModelValue.teamName.Equals(adminRequest.teamName) && adminModelValue.employerName.Equals(adminRequest.employerName))
            {
              return BadRequest($"admin account already exists for {adminRequest.employerName} - {adminRequest.teamName}!");
            }
          }
        }

        var data = await _firebaseClient.SetAsync($"admin/{adminRequest.userName}", adminRequest);
        return Json(data);
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }

    [Description("Login Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpGet("adminLogin/{secret}")]
    public async Task<IActionResult> AdminLogin(string secret, [FromQuery] string wohoo, [FromQuery] string userName, [FromQuery] string password)
    {
      try
      {
        var decryptedSecret = EncryptDecrypt.DecryptStringAES(secret, string.Empty);

        if (!_validateConnection.ValidateToken(decryptedSecret))
        {
          return BadRequest("invalid token");
        }

        var decryptedUserName = EncryptDecrypt.DecryptStringAES(userName, wohoo);
        var decryptedPassword = EncryptDecrypt.DecryptStringAES(password, wohoo);

        var adminData = await _firebaseClient.GetAsync("admin");
        var adminModels = adminData.ResultAs<Dictionary<string, AdminModel>>();

        if (adminModels == null)
        {
          return BadRequest("no admin account exists!");
        }

        foreach (var adminModel in adminModels)
        {
          var adminModelValue = adminModel.Value;
          if (adminModelValue.userName.Equals(decryptedUserName) && adminModelValue.password.Equals(decryptedPassword))
          {
            adminModelValue.password = "";
            return Json(adminModelValue);
          }
        }

        return BadRequest("no admin account exists!");
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }

    [Description("Get Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpGet("getAdmin/{secret}")]
    public async Task<IActionResult> GetAdmin(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName)
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

        var adminData = await _firebaseClient.GetAsync("admin");
        var adminModels = adminData.ResultAs<Dictionary<string, AdminModel>>();

        if (adminModels != null)
        {
          foreach (var adminModel in adminModels)
          {
            var adminModelValue = adminModel.Value;
            if (adminModelValue.employerName.Equals(decryptedEmployer) && adminModelValue.teamName.Equals(decryptedTeam) && adminModelValue.isActive)
            {
              return Json($"{adminModelValue.displayName} is active for {decryptedEmployer} in {decryptedTeam} at the moment");
            }
          }
        }
        else
        {
          return BadRequest("no admin account exists!");
        }
        return Json($"no admin is active for {decryptedEmployer} in {decryptedTeam} at the moment");
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }

    [Description("Update Admin.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpPut("updateAdmin/{secret}/{reset}")]
    public async Task<IActionResult> UpdateAdmin(string secret, string reset, [FromQuery] string wohoo, [FromBody] AdminModel admin)
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
          if (!adminModelValue.userName.Equals(admin.userName) ||
              !adminModelValue.employerName.Equals(admin.employerName) ||
              !adminModelValue.teamName.Equals(admin.teamName))
          {
            continue;
          }
          if (reset.Equals("true"))
          {
            admin.displayName = adminModelValue.displayName;
            admin.isActive = adminModelValue.isActive;
          }
          else
          {
            admin.password = adminModelValue.password;
          }

          var data = await _firebaseClient.UpdateAsync($"admin/{admin.userName}", admin);
          return Ok(new { Admin = data.ResultAs<AdminModel>(), Status = 200 });
        }

        return BadRequest("no admin account exists!");
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }

    [Description("Delete Player.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpDelete("deletePlayer/{secret}")]
    public async Task<IActionResult> DeletePlayer(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName, [FromQuery] string uniqueId)
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

        var data = await _firebaseClient.DeleteAsync($"players/{decryptedEmployer}/{decryptedTeam}/{uniqueId}");
        return Json(data);
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }

    [Description("Delete Players.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpDelete("deletePlayers/{secret}")]
    public async Task<IActionResult> DeletePlayers(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName)
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

        var data = await _firebaseClient.DeleteAsync($"players/{decryptedEmployer}/{decryptedTeam}");
        return Json(data);
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }

    [Description("Get Tokens.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpGet("getTokens/{secret}")]
    public async Task<IActionResult> GetTokens(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName)
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

        var tokensResponse = await _firebaseClient.GetAsync($"tokens/{decryptedEmployer}/{decryptedTeam}");
        var tokensResponseModel = tokensResponse.ResultAs<Dictionary<DateTime, string>>();

        var tokenModel = new TokenModel
        {
          employerName = decryptedEmployer,
          teamName = decryptedTeam,
          tokens = new List<string>()
        };

        if (tokensResponseModel == null) return NotFound("no admin account exists!");

        tokenModel.tokens.AddRange(tokensResponseModel.Values);
        tokenModel.currentToken = tokenModel.tokens[^1];
        return Json(tokenModel);

      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }

    [Description("Delete Tokens.")]
    [SwaggerResponse(StatusCodes.Status200OK, typeof(void))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, typeof(ProblemDetails))]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, typeof(ProblemDetails))]
    [HttpDelete("deleteTokens/{secret}")]
    public async Task<IActionResult> DeleteTokens(string secret, [FromQuery] string wohoo, [FromQuery] string employerName, [FromQuery] string teamName)
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

        var data = await _firebaseClient.DeleteAsync($"tokens/{decryptedEmployer}/{decryptedTeam}");
        return Json(data);
      }
      catch (Exception exception)
      {
        return Conflict(exception);
      }
    }

  }
}
