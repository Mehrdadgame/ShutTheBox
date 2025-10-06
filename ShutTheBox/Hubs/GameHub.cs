using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ShutTheTwelve.Backend.Enums;
using ShutTheTwelve.Backend.Interfaces;
using System.Security.Claims;

namespace ShutTheTwelve.Backend.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly IGameService _gameService;
        private readonly IBotService _botService;
        private static readonly Dictionary<int, string> _playerConnections = new();

        public GameHub(IGameService gameService, IBotService botService)
        {
            _gameService = gameService;
            _botService = botService;
        }

        private int GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }

        public override async Task OnConnectedAsync()
        {
            int userId = GetUserId();
            if (userId > 0)
            {
                _playerConnections[userId] = Context.ConnectionId;
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            int userId = GetUserId();
            _playerConnections.Remove(userId);
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinGame(int gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Game_{gameId}");

            var gameState = await _gameService.GetGameState(gameId);
            await Clients.Caller.SendAsync("GameJoined", gameState);
            await Clients.OthersInGroup($"Game_{gameId}").SendAsync("OpponentJoined", GetUserId());
        }

        public async Task LeaveGame(int gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Game_{gameId}");
            await Clients.OthersInGroup($"Game_{gameId}").SendAsync("OpponentLeft", GetUserId());
        }

        public async Task RollDice(int gameId)
        {
            try
            {
                var result = await _gameService.RollDice(gameId);

                // Send dice result to all players in the game
                await Clients.Group($"Game_{gameId}").SendAsync("DiceRolled", new
                {
                    die1 = result.Die1,
                    die2 = result.Die2,
                    playerTurn = result.PlayerTurn
                });

                // Check if bot game and if it's bot's turn
                var gameState = await _gameService.GetGameState(gameId);
                if (gameState.IsAIGame && gameState.ActiveTurn == "Player2")
                {
                    await Task.Delay(1000); // Bot thinking time
                    await ProcessBotTurn(gameId, result.Die1, result.Die2);
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task MakeMove(int gameId, List<int> selectedNumbers)
        {
            try
            {
                int playerId = GetUserId();
                bool success = await _gameService.MakeMove(gameId, playerId, selectedNumbers);

                if (success)
                {
                    var gameState = await _gameService.GetGameState(gameId);

                    // Notify all players about the move
                    await Clients.Group($"Game_{gameId}").SendAsync("MoveMade", new
                    {
                        playerId,
                        selectedNumbers,
                        gameState
                    });

                    // Check if game ended
                    if (gameState.State == "Finished")
                    {
                        await Clients.Group($"Game_{gameId}").SendAsync("GameEnded", new
                        {
                            winnerId = gameState.WinnerId,
                            gameState
                        });
                        return;
                    }

                    // End turn and switch to next player
                    await _gameService.EndTurn(gameId);
                    var updatedState = await _gameService.GetGameState(gameId);

                    await Clients.Group($"Game_{gameId}").SendAsync("TurnChanged", new
                    {
                        activeTurn = updatedState.ActiveTurn,
                        gameState = updatedState
                    });

                    // If bot game and now bot's turn, process bot move
                    if (updatedState.IsAIGame && updatedState.ActiveTurn == "Player2")
                    {
                        await Task.Delay(1500); // Bot thinking
                        var botRoll = await _gameService.RollDice(gameId);

                        await Clients.Group($"Game_{gameId}").SendAsync("DiceRolled", new
                        {
                            die1 = botRoll.Die1,
                            die2 = botRoll.Die2,
                            playerTurn = "Player2"
                        });

                        await Task.Delay(1000);
                        await ProcessBotTurn(gameId, botRoll.Die1, botRoll.Die2);
                    }
                }
                else
                {
                    await Clients.Caller.SendAsync("InvalidMove", "Invalid move selection");
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        private async Task ProcessBotTurn(int gameId, int die1, int die2)
        {
            try
            {
                var gameState = await _gameService.GetGameState(gameId);
                var botMove = await _botService.GetBotMove(gameState.Player2Board, die1, die2);

                if (botMove.Count > 0)
                {
                    // Bot makes move (simulate as Player2)
                    await Clients.Group($"Game_{gameId}").SendAsync("BotThinking", new
                    {
                        message = "AI is making a move..."
                    });

                    await Task.Delay(800);

                    // We need to manually update Player2's board since bot is not a real player
                    var board = gameState.Player2Board.ToList();
                    foreach (var num in botMove)
                    {
                        board.Remove(num);
                    }

                    await Clients.Group($"Game_{gameId}").SendAsync("MoveMade", new
                    {
                        playerId = 0, // Bot ID
                        selectedNumbers = botMove,
                        gameState
                    });

                    // Check for bot win
                    if (board.Count == 0)
                    {
                        await Clients.Group($"Game_{gameId}").SendAsync("GameEnded", new
                        {
                            winnerId = (int?)null,
                            message = "AI Won!",
                            gameState
                        });
                        return;
                    }

                    // Switch back to player
                    await _gameService.EndTurn(gameId);
                    var updatedState = await _gameService.GetGameState(gameId);

                    await Clients.Group($"Game_{gameId}").SendAsync("TurnChanged", new
                    {
                        activeTurn = "Player1",
                        gameState = updatedState
                    });
                }
                else
                {
                    // Bot has no moves
                    await Clients.Group($"Game_{gameId}").SendAsync("BotNoMoves", new
                    {
                        message = "AI has no valid moves"
                    });

                    await _gameService.EndTurn(gameId);
                    var updatedState = await _gameService.GetGameState(gameId);

                    await Clients.Group($"Game_{gameId}").SendAsync("TurnChanged", new
                    {
                        activeTurn = "Player1",
                        gameState = updatedState
                    });
                }
            }
            catch (Exception ex)
            {
                await Clients.Group($"Game_{gameId}").SendAsync("Error", ex.Message);
            }
        }

        public async Task UseCard(int gameId, string cardType, int? targetDie)
        {
            try
            {
                int playerId = GetUserId();
                bool success = await _gameService.UseCard(gameId, playerId, cardType, targetDie);

                if (success)
                {
                    await Clients.Group($"Game_{gameId}").SendAsync("CardUsed", new
                    {
                        playerId,
                        cardType,
                        targetDie
                    });
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Failed to use card");
                }
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task SendMessage(int gameId, string message)
        {
            try
            {
                int senderId = GetUserId();
                var username = Context.User?.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown";

                await Clients.OthersInGroup($"Game_{gameId}").SendAsync("ReceiveMessage", new
                {
                    senderId,
                    username,
                    message,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("Error", ex.Message);
            }
        }

        public async Task RequestRematch(int gameId)
        {
            await Clients.OthersInGroup($"Game_{gameId}").SendAsync("RematchRequested", GetUserId());
        }

        public async Task AcceptRematch(int gameId)
        {
            await Clients.Group($"Game_{gameId}").SendAsync("RematchAccepted");
        }

        public async Task Surrender(int gameId)
        {
            int playerId = GetUserId();
            await Clients.Group($"Game_{gameId}").SendAsync("PlayerSurrendered", playerId);
        }
    }
}