using System.Collections.Generic;

namespace bingoWebAPI.Models
{
  public class TokenModel
  {
    public string employerName { get; set; }

    public string teamName { get; set; }

    public List<string> tokens { get; set; }

    public string currentToken { get; set; }
  }
}
