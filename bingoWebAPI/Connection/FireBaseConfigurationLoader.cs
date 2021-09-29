using FireSharp.Config;
using Microsoft.Extensions.Configuration;

namespace bingoWebAPI.Connection
{
  public class FireBaseConfigurationLoader
  {
    public static FirebaseConfig GetFireBaseConfiguration(IConfiguration config)
    {
      var authSecret = config.GetSection("AuthSecret").Value ?? string.Empty;
      var basePath = config.GetSection("FireBasePath").Value ?? string.Empty;

      var decryptAuthSecret = EncryptDecrypt.DecryptStringAES(authSecret, "6798890179734834");

      var decryptBasePath = EncryptDecrypt.DecryptStringAES(basePath, "6798890179734834");

      return new FirebaseConfig
      {
        AuthSecret = decryptAuthSecret,
        BasePath = decryptBasePath
      };
    }
  }
}
