using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ShutTheBox.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    TotalWins = table.Column<int>(type: "int", nullable: false),
                    TotalLosses = table.Column<int>(type: "int", nullable: false),
                    TotalDraws = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    BestStreak = table.Column<int>(type: "int", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    WeeklyScore = table.Column<int>(type: "int", nullable: false),
                    MonthlyScore = table.Column<int>(type: "int", nullable: false),
                    LastWeeklyReset = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastMonthlyReset = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsOnline = table.Column<bool>(type: "bit", nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Rarity = table.Column<int>(type: "int", nullable: false),
                    PowerCost = table.Column<int>(type: "int", nullable: false),
                    UnlockLevel = table.Column<int>(type: "int", nullable: false),
                    IconUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerCards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Player1Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Player2Id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsAgainstBot = table.Column<bool>(type: "bit", nullable: false),
                    BotDifficulty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    CurrentTurn = table.Column<int>(type: "int", nullable: false),
                    Player1Board = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Player2Board = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentRound = table.Column<int>(type: "int", nullable: false),
                    Player1Score = table.Column<int>(type: "int", nullable: false),
                    Player2Score = table.Column<int>(type: "int", nullable: false),
                    LastDice1 = table.Column<int>(type: "int", nullable: false),
                    LastDice2 = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WinnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GameMode = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameSessions_Players_Player1Id",
                        column: x => x.Player1Id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GameSessions_Players_Player2Id",
                        column: x => x.Player2Id,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlayerCards",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PowerCardId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerCards", x => new { x.PlayerId, x.PowerCardId });
                    table.ForeignKey(
                        name: "FK_PlayerCards_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerCards_PowerCards_PowerCardId",
                        column: x => x.PowerCardId,
                        principalTable: "PowerCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GameMoves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MoveNumber = table.Column<int>(type: "int", nullable: false),
                    Dice1 = table.Column<int>(type: "int", nullable: false),
                    Dice2 = table.Column<int>(type: "int", nullable: false),
                    NumbersUsed = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BoardStateBefore = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BoardStateAfter = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameMoves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GameMoves_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GameMoves_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OpponentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OpponentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsWinner = table.Column<bool>(type: "bit", nullable: false),
                    ScoreGained = table.Column<int>(type: "int", nullable: false),
                    ExpGained = table.Column<int>(type: "int", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchHistories_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchHistories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PowerCardUsages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GameSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PowerCardId = table.Column<int>(type: "int", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    Effect = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UsedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerCardUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerCardUsages_GameSessions_GameSessionId",
                        column: x => x.GameSessionId,
                        principalTable: "GameSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PowerCardUsages_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PowerCardUsages_PowerCards_PowerCardId",
                        column: x => x.PowerCardId,
                        principalTable: "PowerCards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PowerCards",
                columns: new[] { "Id", "Description", "IconUrl", "IsActive", "Name", "PowerCost", "Rarity", "Type", "UnlockLevel" },
                values: new object[,]
                {
                    { 1, "Lock one die and reroll the other", "", true, "Lock & Reroll", 15, 0, 0, 1 },
                    { 2, "Force opponent's die to roll 1-3", "", true, "Sabotage", 20, 0, 1, 1 },
                    { 3, "Reroll both dice if no moves", "", true, "Second Chance", 25, 1, 2, 1 },
                    { 4, "Set one die to 5 or 6", "", true, "Wild Dice", 30, 1, 3, 1 },
                    { 5, "Steal opponent's dice roll", "", true, "Steal Turn", 35, 2, 4, 1 },
                    { 6, "Block opponent's attack card", "", true, "Shield", 25, 2, 5, 1 },
                    { 7, "Roll 3 times, keep best", "", true, "Lightning Roll", 40, 3, 6, 1 },
                    { 8, "Swap random numbers with opponent", "", true, "Swap Board", 30, 2, 7, 1 },
                    { 9, "Copy opponent's last card", "", true, "Mimic", 35, 3, 8, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameMoves_GameSessionId",
                table: "GameMoves",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GameMoves_PlayerId",
                table: "GameMoves",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_Player1Id",
                table: "GameSessions",
                column: "Player1Id");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_Player2Id",
                table: "GameSessions",
                column: "Player2Id");

            migrationBuilder.CreateIndex(
                name: "IX_MatchHistories_GameSessionId",
                table: "MatchHistories",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchHistories_PlayerId",
                table: "MatchHistories",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerCards_PowerCardId",
                table: "PlayerCards",
                column: "PowerCardId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                table: "Players",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Username",
                table: "Players",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PowerCardUsages_GameSessionId",
                table: "PowerCardUsages",
                column: "GameSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerCardUsages_PlayerId",
                table: "PowerCardUsages",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PowerCardUsages_PowerCardId",
                table: "PowerCardUsages",
                column: "PowerCardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameMoves");

            migrationBuilder.DropTable(
                name: "MatchHistories");

            migrationBuilder.DropTable(
                name: "PlayerCards");

            migrationBuilder.DropTable(
                name: "PowerCardUsages");

            migrationBuilder.DropTable(
                name: "GameSessions");

            migrationBuilder.DropTable(
                name: "PowerCards");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
