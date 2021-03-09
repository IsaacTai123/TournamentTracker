using System;
using System.Collections.Generic;
using System.Text;
using TrackerLibrary.Models;

namespace TrackerLibrary.DataAccess
{
    public interface IDataConnection
    {
        void CreatePrize(PrizeModel model);
        void CreatePerson(PersonModel model);
        List<PersonModel> GetPerson_All();
        List<TeamModel> GetTeam_All();
        void createTeam(TeamModel model);
        void CreateTournament(TournamentModel model);
        void CompleteTournament(TournamentModel model);

        // why not return type, well because if we work on an object then anything we do gets passed back to the caller because again it's just that address 
        // but also we're just updating, we're just saying taking the information that i already have input the database so i can get back next time
        // i call the database
        void UpdateMatchup(MatchupModel model);
        List<TournamentModel> GetTournament_All();
    }
}
