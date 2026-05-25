using FantasyPitchXI.Data;
using FantasyPitchXI.DTO;
using FantasyPitchXI.DTO.Request_DTO;
using FantasyPitchXI.Models;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Intrinsics.X86;

namespace FantasyPitchXI.Services
{
    public class PickTeamService
    {
        private readonly AppDbContext _db;
        public readonly TeamValidationService _teamValidationService;

        public PickTeamService(AppDbContext db)
        {
            _db = db;
            _teamValidationService = new TeamValidationService(db);
        }

        public List<Player> GetTeamCurrentSquad(int teamId)
        {
            var team = _teamValidationService.GetTeamById(teamId);
            var sqaudPlayers = team.FantasyTeamPlayers.Select(p => p.Player).ToList();

            return sqaudPlayers;
        }
        
        public ApiResponse ValidateALLSelection(ValidateAllSelectionRequestDTO request)
        {
            var combined = request.SelectedXIPlayersIDs.Concat(request.SelectedBenchPlayersIDs).ToList();
            if (combined.Count != 15)
            {
                return ApiResponse.Fail("Starting / Bench incomplete");
            }
            if (combined.Distinct().Count() != 15)
            {
                return ApiResponse.Fail("Duplicate players selected");
            }

            return ApiResponse.Pass("passed");
        }

        public ApiResponse SelectStartingXIandBench(SelectStartingXIandBenchRequestDTO request)
        {
            var props = new ValidateAllSelectionRequestDTO
            {
                SelectedXIPlayersIDs = request.StartingIDs,
                SelectedBenchPlayersIDs = request.BenchIDs
            };
            var check = ValidateALLSelection(props);

            if (!check.Success) 
                return ApiResponse.Fail(check.Message);

            //Check the number of players chosen
            if (request.StartingIDs.Count != 11) 
                return ApiResponse.Fail("You must select 11 Players");

            var benchCheck = ValidateBenchPlayers(request.Team.Id, request.BenchIDs);
            if (!benchCheck.Success)
            {
                return ApiResponse.Fail(benchCheck.Message);
            }            

            //Check that IDs are valid in db and belong to Fantasy Team
            var areIDsValid = AreIDsValidAndOwned(request.Team.Id, request.StartingIDs);
            if (!areIDsValid.Success) 
                return ApiResponse.Fail(areIDsValid.Message);

            List<Player> chosenXIPlayers = _teamValidationService.GetPlayersByIds(request.StartingIDs);

            //Ensure players position count meets requirements
            var validatePlayersXI = ValidateNumberOfPlayersXI(chosenXIPlayers);
            if (!validatePlayersXI.Success) 
                return ApiResponse.Fail(validatePlayersXI.Message);

            //Validate the Captain and Vice Captain selections
            var response = new ValidateCapValidateViceRequestDTO
            {
                Team = request.Team,
                ChosenXIPlayers = chosenXIPlayers,
                CapID = request.CapID,
                ViceID = request.ViceID
            };
            bool checkCapVice = ValidateCapValidateVice(response);
            if (!checkCapVice)
            {
                return ApiResponse.Fail("Invalid Cap and Vice Selection");
            }

            //Set the lineup and bench in db
            var propsUpdate = new UpdateXIandBenchRequestDTO
            {
                TeamID = request.Team.Id,
                ChosenXIPlayers = chosenXIPlayers,
                SelectedBenchPlayers = benchCheck.Data,
                CapID = request.CapID,
                ViceID = request.ViceID,
            };


            if (!UpdateXIandBench(propsUpdate).Success)
            {
                return ApiResponse.Fail("Error saving team");
            }

            //Save changes to db
            _db.SaveChanges();

            return ApiResponse.Pass("XI Selected");

        }

        private ApiResponse UpdateXIandBench(UpdateXIandBenchRequestDTO request)
        {
            //Clear current XI and bench in db
            var existingLineup = _db.FantasyTeamLineups.Where(l => l.FantasyTeamId == request.TeamID && l.Gameweek == GameState.CurrentGameweek).ToList();
            _db.FantasyTeamLineups.RemoveRange(existingLineup);

            //Assign IsStarting bool to starters in db with 0 as BenchOrder
            foreach (var p in request.ChosenXIPlayers)
            {
                var addStart = new FantasyTeamLineup
                {
                    FantasyTeamId = request.TeamID,
                    PlayerId = p.Id,
                    Gameweek = GameState.CurrentGameweek,
                    IsStarting = true,
                    BenchOrder = 0
                };

                if (request.CapID == p.Id)
                {
                    addStart.IsCaptain = true;
                }

                if (request.ViceID == p.Id)
                {
                    addStart.IsViceCaptain = true;
                }

                _db.FantasyTeamLineups.Add(addStart);
            }

            //Assign remaining squad to bench via their position in List            
            for (int i = 0; i < request.SelectedBenchPlayers.Count; i++)
            {               
                var addBench = new FantasyTeamLineup
                {
                    FantasyTeamId = request.TeamID,
                    PlayerId = request.SelectedBenchPlayers[i].Id,
                    Gameweek = GameState.CurrentGameweek,
                    IsStarting = false,
                    BenchOrder = i + 1
                };
                _db.FantasyTeamLineups.Add(addBench);
            }

            return ApiResponse.Pass("passed");
        }

        private ApiResponse ValidateNumberOfPlayersXI(List<Player> chosenPlayers)
        {
            var playersGroup = _teamValidationService.groupPlayerByPosition(chosenPlayers);

            string? message =
                playersGroup.GK > GameRuleConstants.MaxGoalkeeperXI ? $"You Must Select {GameRuleConstants.MaxGoalkeeperXI} Goalkeeper" :
                playersGroup.DEF < GameRuleConstants.MinDefendersXI ? $"Must have Min {GameRuleConstants.MinDefendersXI} Defender" :
                playersGroup.MID < GameRuleConstants.MinMidfieldersXI ? $"Must have Min {GameRuleConstants.MinMidfieldersXI} Midfielders" :
                playersGroup.ST < GameRuleConstants.MinStrikersXI ? $"Must have Min {GameRuleConstants.MinStrikersXI} Strikers" :
                null;

            return message == null ? ApiResponse.Pass("passed") : ApiResponse.Fail(message);

        }

        private bool ValidateCapValidateVice(ValidateCapValidateViceRequestDTO request)
        {
            //cap and vice id cannot be the same
            if (request.CapID == request.ViceID)
            {
                return false;
            }

            //cap and vice id and team xcannot be null
            if (request.Team == null)
            {
                return false;
            }

            //cap and vice id must be players in team selected xi
            bool CapExist = false, ViceExist = false;

            foreach (Player player in request.ChosenXIPlayers)
            {
                if (player.Id == request.CapID)
                {
                    CapExist = true;
                }
                if (player.Id == request.ViceID)
                {
                    ViceExist = true;
                }
            }

            if (!(CapExist && ViceExist))
            {
                return false;
            }

            return true;

        }

        private ApiResponse AreIDsValidAndOwned (int teamID, List<int> IDs)
        {
            //Check that the IDs exist as players in db and check they belong to the team
            foreach (int i in IDs)
            {
                var exists = _db.Player.FirstOrDefault(p => p.Id == i);
                var owned = _db.FantasyTeamPlayer.Any(ftp => ftp.FantasyTeamId == teamID && ftp.PlayerId == i);

                if (exists == null)
                {
                    return ApiResponse.Fail("ID does not exist");
                }
                if (!owned)
                {
                    return ApiResponse.Fail("Chosen ID does not belong to Fantasy Team");
                }
            }

            return ApiResponse.Pass("proceed");
        }

        private ApiResponse<List<Player>> ValidateBenchPlayers(int teamID, List<int> SelectedBenchPlayersIDs)
        {
            //Validate exACTLY 4 PLAYERS CHOSEN
            if (SelectedBenchPlayersIDs.Count != 4)
            {
                return ApiResponse<List<Player>>.Fail("Bench must contain 4 players");
            }

            //Check that the IDs exist as players in db and belong to Fantasy Team
            var areValidandOwned = AreIDsValidAndOwned(teamID, SelectedBenchPlayersIDs);
            if (!areValidandOwned.Success)
            {
                return ApiResponse<List<Player>>.Fail(areValidandOwned.Message);
            }

            List<Player> SelectedBenchPlayers = _teamValidationService.GetPlayersByIds(SelectedBenchPlayersIDs);
            
            //Validate must be 1 GK and 3 outfield players
            var playersGroup = _teamValidationService.groupPlayerByPosition(SelectedBenchPlayers);
            var outfield = playersGroup.DEF + playersGroup.MID + playersGroup.ST;

            if (playersGroup.GK != 1)
            {
                return ApiResponse<List<Player>>.Fail("Must select 1 Goalkeeper");
            }
            if(outfield != 3)
            {
                return ApiResponse<List<Player>>.Fail("Must select 3 outfield players");
            }

            // Validate chosen IDs belong to Fantasy Team

            return ApiResponse<List<Player>>.Pass("passed", SelectedBenchPlayers);
        }


    }
}
