using System;
using System.Collections.Generic;
using System.Text;
using TrackerLibrary.Models;
using TrackerLibrary.DataAccess.TextHelpers;
using System.Linq;

namespace TrackerLibrary.DataAccess
{
    // TODO - Wire up the CreatePrize for text files
    public class TextConnector : IDataConnection
    {
        public void CreatePerson(PersonModel model)
        {
            List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

            // Find the max id
            int currentId = 1;

            if (people.Count > 0)
            {
                currentId = people.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;

            people.Add(model);

            people.SaveToPeopleFile();
        }

        public void CreatePrize(PrizeModel model)
        { 
            // * Load the text file
            // * Convert the text to List<PrizeModel>
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();

            // Find the max ID
            int currentId = 1;
            
            if (prizes.Count > 0)
            {
                currentId = prizes.OrderByDescending(x => x.Id).First().Id + 1;
            }
            model.Id = currentId;

            // Add the new record with the new ID (max + 1)
            prizes.Add(model);

            // Convert the prizes to List<string>
            // save the List<string> to the text file
            prizes.SaveToPrizeFile();
        }

        public void createTeam(TeamModel model)
        {
            // Load the text file
            // convert text file to List<TeamModel>
            List<TeamModel> teams = GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();

            int currentId = 1;
            if (teams.Count > 0)
            {
                currentId = teams.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;
            teams.Add(model);

            teams.SaveToTeamFile();
        }

        public List<PersonModel> GetPerson_All()
        {
            return GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();
        }

        public List<TeamModel> GetTeam_All()
        {
            return GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();
        }

        // 這邊我用 void 因為instances 不需要來回傳送新資料, 如果有兩個地方有用到這個 只要改動一邊 那麼另一邊也會自動更新
        // instances passed don't have to pass back and forth, once they have been two different locations you can modify either location and they both are updated.
        public void CreateTournament(TournamentModel model)
        {
            List<TournamentModel> tournament = GlobalConfig.TournamentFile
                .FullFilePath()
                .LoadFile()
                .ConvertToTournamentModels();

            int currentId = 1;
            if (tournament.Count > 0)
            {
                currentId = tournament.OrderByDescending(x => x.Id).First().Id + 1;
            }

            model.Id = currentId;

            // 在存入tournament 之前 我們要像SQL 一樣把裡面Id對應的model 都存入個別的file裡
            model.SaveRoundsToFile();

            tournament.Add(model);

            tournament.SaveToTournamentFile();

            // 在我們Create tournament 上傳到sql/Textfile之後 我們要把每個bye moved into the next round
            TournamentLogic.UpdateTournamentResults(model);
        }

        public List<TournamentModel> GetTournament_All()
        {
            return GlobalConfig.TournamentFile
                .FullFilePath()
                .LoadFile()
                .ConvertToTournamentModels();
        }

        public void UpdateMatchup(MatchupModel model)
        {
            model.UpdateMatchupToFile();
        }

        public void CompleteTournament(TournamentModel model)
        {
            List<TournamentModel> tournament = GlobalConfig.TournamentFile
                .FullFilePath()
                .LoadFile()
                .ConvertToTournamentModels();

            //tournament.Remove(model); //this doesn't remove the model from the list.

            tournament.RemoveAt(tournament.Count - 1); // 上面的不知為什麼無法remove the model, 所以用這個刪掉對應的位置 也就是最後一個

            tournament.SaveToTournamentFile();

            //TournamentLogic.UpdateTournamentResults(model); // 這行不需要 因為已經結束了 不需要更新 而且加上這行會造成infinite loop
        }
    }
}
