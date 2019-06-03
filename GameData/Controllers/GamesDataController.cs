using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameData.Models;
using GameData.Models.TicTacToe;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using Newtonsoft.Json.Linq;
using GameApplication.Logging;

namespace GameData.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GamesDataController : Controller
    {
        private readonly IReliableStateManager _stateManager;
        private readonly GameApplication.Logging.ILogger _logger;
        public GamesDataController(IReliableStateManager stateManager, GameApplication.Logging.ILogger logger)
        {
            _stateManager = stateManager;
            _logger = logger;
        }


        [HttpGet()]
        public async Task<IActionResult> Get()
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<string, Game> gamesDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Game>>("games");

            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<string, Game>> list = await gamesDictionary.CreateEnumerableAsync(tx);

                Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<string, Game>> enumerator = list.GetAsyncEnumerator();

                List<string> result = new List<string>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current.Key);
                }

                return this.Json(result);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            IReliableDictionary<string, Game> gamesDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Game>>("games");

            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                ConditionalValue<Game> gameState = await gamesDictionary.TryGetValueAsync(tx, id);

                if (gameState.HasValue)
                {
                    if (gameState.Value.State is TicTacToeState state)
                    {
                        return Json(state.Board);
                    }
                    else
                    {
                        return BadRequest();
                    }                
                }
                else
                {
                    this.Response.Headers.Add("X-ServiceFabric", new Microsoft.Extensions.Primitives.StringValues("ResourceNotFound"));
                    return NotFound();
                }
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Post(string id)
        {
            IReliableDictionary<string, Game> gamesDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Game>>("games");

            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                bool gameExists = await gamesDictionary.ContainsKeyAsync(tx, id);

                if (gameExists)
                {
                    return BadRequest("Game already exists");
                }

                var game = new Game 
                {
                    State = new TicTacToeState() { Board = new string[9], IsXTurn = true, }, 
                    UserRoles = new Dictionary<string, string>() 
                };

                await gamesDictionary.AddAsync(tx, id, game);
                await tx.CommitAsync();

                return Ok();
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] JToken values)
        {
            try
            {

            var userId = values["player"].Value<string>();
            var idx = values["idx"].Value<int>();
               
               _logger.Info($"Got move from {userId} for {idx}");

            IReliableDictionary<string, Game> gamesDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Game>>("games");

            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                var game = await gamesDictionary.TryGetValueAsync(tx, id);

                if (!game.HasValue)
                {
                    return NotFound();
                }

                if (game.Value.State is TicTacToeState state)
                {
                    _logger.Info("It was a TicTacToeGame");

                    var playerMarker = game.Value.UserRoles[userId];

                    if (state.Board[idx] == null && 
                        ((state.IsXTurn && playerMarker == "X") || (!state.IsXTurn && playerMarker == "O")))
                    {
                        state.Board[idx] = playerMarker;
                        state.IsXTurn = !state.IsXTurn;
                    
                        await gamesDictionary.SetAsync(tx, id, game.Value);
                        await tx.CommitAsync();
                    }

                    return Json(state.Board);
                }
                else
                {
                    _logger.Info("It was not a TicTacToeGame");

                    return BadRequest();
                }
            }
            }
            catch (Exception e)
            {
                _logger.Info(e.ToString());
                throw;
            }
        }

        [HttpPut("AddPlayer/{id}")]
        public async Task<IActionResult> AddPlayer(string id, [FromBody] JToken values)
        {
            var userId = values["player"].Value<string>();

            IReliableDictionary<string, Game> gamesDictionary = await _stateManager.GetOrAddAsync<IReliableDictionary<string, Game>>("games");

            using (ITransaction tx = _stateManager.CreateTransaction())
            {
                var game = await gamesDictionary.TryGetValueAsync(tx, id);

                if (!game.HasValue)
                {
                    return NotFound();
                }

                if (userId != null && !game.Value.UserRoles.ContainsKey(userId))
                {
                    if (game.Value.UserRoles.Count == 0)
                    {
                        game.Value.UserRoles[userId] = "X";
                    }
                    else if (game.Value.UserRoles.Count == 1)
                    {
                        game.Value.UserRoles[userId] = game.Value.UserRoles.First().Value == "X" ? "O" : "X";
                    }
                    else
                    {
                        return BadRequest();
                    }

                    await gamesDictionary.SetAsync(tx, id, game.Value);
                    await tx.CommitAsync();
                }

                return Json(game.Value.UserRoles[userId]);
            }
        }
    }
}