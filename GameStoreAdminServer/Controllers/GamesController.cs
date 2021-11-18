using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using GameStoreAdminServer.Models;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GameStoreAdminServer.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {

        private readonly Greeter.GreeterClient _client;

        public GamesController()
        {
            _client = GrpcClient.Instance;
        }
        
        // GET: api/games
        [HttpGet]
        public async Task<string> GetGames()
        {
            return "HolaMundo";
        }
        [HttpPost]
        public async Task<string> AddGame([FromBody]GameInModel game)
        {
            var reply = await _client.AddGameAsync(
                new AddRequest() { 
                    Name = game.Title, 
                    Genre = game.Genre, 
                    Sinopsis = game.Sinopsis});
            return reply.Message;
        }
        
        [HttpPut("{id}")]
        public async Task<string> ModifyGame([FromRoute]int id, [FromBody]GameInModel game)
        {
            var reply = await _client.ModifyGameAsync(
                new ModifyRequest() { 
                    Id = id,
                    Name = game.Title, 
                    Genre = game.Genre, 
                    Sinopsis = game.Sinopsis});
            return reply.Message;
        }
        
        [HttpDelete("{id}")]
        public async Task<string> DeleteGame([FromRoute]int id, [FromBody]GameInModel game)
        {
            var reply = await _client.DeleteGameAsync(
                new DeleteRequest() { 
                    Id = id });
            return reply.Message;
        }
    }
}