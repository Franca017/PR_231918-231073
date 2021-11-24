using System.Threading.Tasks;
using GameStoreAdminServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameStoreAdminServer.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly GameComs.GameComsClient _client;

        public GamesController()
        {
            var channel = GrpcChannelAccess.Instance;
            _client = new GameComs.GameComsClient(channel);
        } 
        
        [HttpGet]
        public async Task<string> GetGames()
        {
            var reply = await _client.GetGamesAsync(
                new RequestGames());
            return reply.GamesList.ToString();
        }
        [HttpPost]
        public async Task<string> AddGame([FromBody]GameInModel game)
        {
            var reply = await _client.AddGameAsync(
                new AddGameRequest() { 
                    Name = game.Title, 
                    Genre = game.Genre, 
                    Sinopsis = game.Sinopsis});
            return reply.Message;
        }
        
        [HttpPut("{id}")]
        public async Task<string> ModifyGame([FromRoute]int id, [FromBody]GameInModel game)
        {
            var reply = await _client.ModifyGameAsync(
                new ModifyGameRequest() { 
                    Id = id,
                    Name = game.Title, 
                    Genre = game.Genre, 
                    Sinopsis = game.Sinopsis});
            return reply.Message;
        }
        
        [HttpDelete("{id}")]
        public async Task<string> DeleteGame([FromRoute]int id)
        {
            var reply = await _client.DeleteGameAsync(
                new DeleteGameRequest() { 
                    Id = id });
            return reply.Message;
        }
    }
}