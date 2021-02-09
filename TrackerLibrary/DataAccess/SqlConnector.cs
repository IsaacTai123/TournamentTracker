using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using TrackerLibrary.Models;



namespace TrackerLibrary.DataAccess
{
    public class SqlConnector : IDataConnection
    {
        private const string DB = "Tournaments";
        public PersonModel CreatePerson(PersonModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB)))
            {
                var p = new DynamicParameters();
                p.Add("@FirstName", model.FirstName);
                p.Add("@LastName", model.LastName);
                p.Add("@EmailAddress", model.EmailAddress);
                p.Add("@CellphoneNumber", model.CellphoneNumber);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPeople_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                return model;
            }
        }


        // TODO - Make the createPrize method actually save to the database
        /// <summary>
        /// Saves a new prize to the database
        /// </summary>
        /// <param name="model">The prize information</param>
        /// <returns>The prize information, the unique identifier</returns>
        public PrizeModel CreatePrize(PrizeModel model)
        {
            //@PlaceNumber int,
            //@PlaceName nvarchar(50),
            //@PrizeAmount money,
            //@PrizePercentage float,
            //@id int = 0 output
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB)))
           {
                var p = new DynamicParameters();
                p.Add("@PlaceNumber", model.PlaceNumber);
                p.Add("@PlaceName", model.PlaceName);
                p.Add("@PrizeAmount", model.PrizeAmount);
                p.Add("@PrizePercentage", model.PrizePercentage);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spPrizes_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                return model;
           }
        }

        public TeamModel createTeam(TeamModel model)
        {
            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB)))
            {
                var p = new DynamicParameters();
                p.Add("@TeamName", model.TeamName);
                p.Add("@id", 0, dbType: DbType.Int32, direction: ParameterDirection.Output);

                connection.Execute("dbo.spTeams_Insert", p, commandType: CommandType.StoredProcedure);

                model.Id = p.Get<int>("@id");

                //Insert everyone of those people into our table
                foreach (PersonModel tm in model.TeamMembers)
                {
                    p = new DynamicParameters(); //this is dynamicparameters which you want to use therefore we can change it
                    p.Add("@TeamId", model.Id);
                    p.Add("@PersonId", tm.Id);

                    connection.Execute("dbo.spTeamMembers_Insert", p, commandType: CommandType.StoredProcedure);
                }

                return model;
            }
        }

        public List<PersonModel> GetPerson_All()
        {
            List<PersonModel> output;

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB)))
            {
                output = connection.Query<PersonModel>("dbo.spPeople_GetAll").ToList();
            }

            return output;
        }

        public List<TeamModel> GetTeam_All()
        {
            List<TeamModel> output;

            using (IDbConnection connection = new System.Data.SqlClient.SqlConnection(GlobalConfig.CnnString(DB)))
            {
                output = connection.Query<TeamModel>("dbo.spTeam_GetAll").ToList();

                foreach (TeamModel team in output)
                {
                    var p = new DynamicParameters();
                    p.Add("@TeamId", team.Id);

                    team.TeamMembers = connection.Query<PersonModel>("dbo.spTeamMembers_GetByTeam", p, commandType: CommandType.StoredProcedure).ToList();
                }
            }

            return output;
        }
    }
}
