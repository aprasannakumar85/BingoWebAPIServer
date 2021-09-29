using Microsoft.Extensions.Configuration;

namespace bingoWebAPI.Connection
{
  public class ValidateConnection
  {
    private readonly IConfiguration configuration;

    public ValidateConnection(IConfiguration Configuration)
    {
      configuration = Configuration;
    }

    public bool ValidateToken(string secret)
    {
      var applicationSecret = configuration.GetSection("ApplicationSecret").Value ?? string.Empty;

      var decryptStringAes = EncryptDecrypt.DecryptStringAES(applicationSecret, "6798890179734834");

      return decryptStringAes.Equals(secret);
    }
  }
}
