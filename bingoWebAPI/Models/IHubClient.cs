using System.Threading.Tasks;

namespace bingoWebAPI.Models
{
  public interface IHubClient
  {
    Task BroadcastRequestMessage(RequestModel request);
    Task BroadcastRequestToken(RequestModel request);
    Task BroadcastMessage();
  }
}
