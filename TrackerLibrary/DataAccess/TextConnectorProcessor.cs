using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using TrackerLibrary.Models;

// * Load the text file
// * Convert the text to List<****Model>
// Find the max ID
// Add the new record with the new ID (max + 1)
// Convert the prizes to List<string>
// save the List<string> to the text file

namespace TrackerLibrary.DataAccess.TextHelpers
{
    public static class TextConnectorProcessor
    {
        public static string FullFilePath(this string fileName)
        {
            // C:\Users\user\source\repos\TournamentTracker\Data\Tournaments\PrizeModels.csv
            return $"{ ConfigurationManager.AppSettings["filePath"]}\\{ fileName }";
        }

        public static List<string> LoadFile(this string file)
        {
            //check if the file is exist
            if (!File.Exists(file))
            {
                //file not exists
                return new List<string>();
            }

            return File.ReadAllLines(file).ToList();
        }

        public static List<PrizeModel> ConvertToPrizeModels(this List<string> lines)
        {
            List<PrizeModel> output = new List<PrizeModel>();

            foreach(string line in lines)
            {
                string[] cols = line.Split(',');

                PrizeModel p = new PrizeModel();
                p.Id = int.Parse(cols[0]);
                p.PlaceNumber = int.Parse(cols[1]);
                p.PlaceName = cols[2];
                p.PrizeAmount = decimal.Parse(cols[3]);
                p.PrizePercentage = double.Parse(cols[4]);

                output.Add(p);
            }

            return output;
        }


        public static List<PersonModel> ConvertToPersonModels(this List<string> lines)
        {
            List<PersonModel> output = new List<PersonModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                PersonModel p = new PersonModel();
                p.Id = int.Parse(cols[0]);
                p.FirstName = cols[1];
                p.LastName = cols[2];
                p.EmailAddress = cols[3];
                p.CellphoneNumber = cols[4];

                output.Add(p);
            }

            return output;
        }

        public static List<TeamModel> ConvertToTeamModels(this List<string> lines)
        {
            //id, team name, list of ids seperated by the pipe
            //3, Tim's Team, 1|3|5

            List<TeamModel> output = new List<TeamModel>();
            List<PersonModel> people = GlobalConfig.PeopleFile.FullFilePath().LoadFile().ConvertToPersonModels();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                TeamModel t = new TeamModel();
                t.Id = int.Parse(cols[0]);
                t.TeamName = cols[1];

                string[] personId = cols[2].Split('|'); // ??

                foreach (string id in personId)
                {
                    t.TeamMembers.Add(people.Where(x => x.Id == int.Parse(id)).First()); //the first one mean the first item, see this in video 34:00
                    
                //    t.TeamMembers.Add((from x in people
                //                      where x.Id == int.Parse(id)
                //                      select x).First());
                }

                output.Add(t);
            }

            return output;
        }

        public static List<TournamentModel> ConvertToTournamentModels(
            this List<string> lines)
        {
            // id = 0
            // TournamentName = 1
            // EntryFee = 2
            // EnteredTeams = 3
            // Prizes = 4
            // Rounds = 5
            // TournamentFile structure -> id, TournamentName, EntryFee, (id|id|id - Entered Teams), (id|id|id - Prizes), (Rounds - id^id^id|id^id^id|id^id^id)

            List<TournamentModel> output = new List<TournamentModel>();
            List<TeamModel> teams = GlobalConfig.TeamFile.FullFilePath().LoadFile().ConvertToTeamModels();
            List<PrizeModel> prizes = GlobalConfig.PrizesFile.FullFilePath().LoadFile().ConvertToPrizeModels();
            List<MatchupModel> matchups = GlobalConfig.MatchupFile
                .FullFilePath()
                .LoadFile()
                .ConvertToMatchupModel();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                
                TournamentModel tm = new TournamentModel();
                tm.Id = int.Parse(cols[0]);
                tm.TournamentName = cols[1];
                tm.Entryfee = decimal.Parse(cols[2]);

                string[] TeamIds = cols[3].Split('|');
                foreach (string id in TeamIds)
                {
                    tm.EnteredTeams.Add(teams.Where(x => x.Id == int.Parse(id)).First());
                }

                if (cols[4].Length > 0)
                {
                    string[] PrizeIds = cols[4].Split('|');
                    foreach (string id in PrizeIds)
                    {
                        tm.Prizes.Add(prizes.Where(x => x.Id == int.Parse(id)).First());
                    } 
                }

                // Capture Rounds information
                string[] rounds = cols[5].Split('|');
                List<MatchupModel> ms = new List<MatchupModel>();

                foreach (string round in rounds)
                {
                    string[] msText = round.Split('^');
                    
                    foreach (string matchupModelTextId in msText)
                    {
                        //ms.Add(matchups.Where(x => x.Id == int.Parse(matchupModelTextId)).First());
                        ms.Add(LookupMatchupById(int.Parse(matchupModelTextId)));
                    }
                    tm.Rounds.Add(ms);
                    ms = new List<MatchupModel>();
                }

                output.Add(tm);
            }
            return output;
        }
        
        public static void SaveToTournamentFile(this List<TournamentModel> models)
        {
            List<string> lines = new List<string>();

            foreach (TournamentModel tm in models)
            {
                //lines.Add($@"{ tm.Id },{ tm.TournamentName },{ tm.Entryfee },{ ConvertTeamListToString(tm.EnteredTeams) },{ ConvertPrizeListToString(tm.Prizes) },{ ConvertRoundListToString(tm.Rounds) } ");
                lines.Add(
                    $"{ tm.Id },"+
                    $"{ tm.TournamentName },"+
                    $"{ tm.Entryfee },"+
                    $"{ ConvertTeamListToString(tm.EnteredTeams) },"+
                    $"{ ConvertPrizeListToString(tm.Prizes) },"+
                    $"{ ConvertRoundListToString(tm.Rounds) } ");
            }

            File.WriteAllLines(GlobalConfig.TournamentFile.FullFilePath(), lines);
        }

        public static void SaveToPrizeFile(this List<PrizeModel> models)
        {
            List<string> lines = new List<string>();

            foreach (PrizeModel p in models)
            {
                lines.Add($"{ p.Id },{ p.PlaceNumber },{ p.PlaceName },{ p.PrizeAmount },{ p.PrizePercentage }");
            }

            File.WriteAllLines(GlobalConfig.PrizesFile.FullFilePath(), lines); // WriteAllLines this method will overwrite the file every time and that is what we want
        }

        public static void SaveToPeopleFile(this List<PersonModel> models)
        {
            List<string> lines = new List<string>();

            foreach (PersonModel p in models)
            {
                lines.Add($"{ p.Id },{ p.FirstName },{ p.LastName },{ p.EmailAddress },{ p.CellphoneNumber }");
            }

            File.WriteAllLines(GlobalConfig.PeopleFile.FullFilePath(), lines);
        }

        public static void SaveToTeamFile(this List<TeamModel> model)
        {
            List<string> lines = new List<string>();

            foreach (TeamModel t in model)
            {
                lines.Add($"{ t.Id },{ t.TeamName },{ ConvertPeopleListToString(t.TeamMembers) }");
            }

            File.WriteAllLines(GlobalConfig.TeamFile.FullFilePath(), lines);
        }

        public static void SaveRoundsToFile(this TournamentModel model)
        {
            // Loop through each Round
            // Loop through each Matchup
            // Get the id for the new matchup and save the record
            // Loop through each Entry, get the id, and save it 

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel matchup in round)
                {
                    // Load all of the matchups from file
                    // Get the top id and add one
                    // Store the id
                    // Save the matchup record
                    matchup.SaveMatchupToFile();
                }
            }
        }

        public static List<MatchupModel> ConvertToMatchupModel(this List<string> lines)
        {
            // id=0, 
            // entries=1(pipe delimited by id),  ( id|id )
            // winner=2, 
            // MatchupRound=3

            List<MatchupModel> output = new List<MatchupModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');

                MatchupModel p = new MatchupModel();
                p.Id = int.Parse(cols[0]);
                p.Entris = ConvertStringToMatchupEntryModel(cols[1]);

                if (cols[2].Length == 0)
                {
                    p.Winner = null;
                }
                else
                {
                    p.Winner = LookupTeamById(int.Parse(cols[2]));
                }
                p.MatchupRound = int.Parse(cols[3]);
                output.Add(p);
            }

            return output;
        }

        private static List<MatchupEntryModel> ConvertStringToMatchupEntryModel(string input)
        {
            string[] ids = input.Split('|');
            List<MatchupEntryModel> output = new List<MatchupEntryModel>();

            //這就是造成 infinite Loop 的地方, 為了找出與之對應的id 我們去file抓出所有的紀錄做比對 但每次都會在parentmatchup 那邊做同樣的事 一直去找然後就一直輪迴
            //List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile().ConvertToMatchupEntryModel(); 所以這行是不行的

            List<string> entries = GlobalConfig.MatchupEntryFile.FullFilePath().LoadFile();
            List<string> matchingEntries = new List<string>();

            foreach (string id in ids)
            {
                foreach (string entry in entries)
                {
                    string[] cols = entry.Split(',');

                    if (id == cols[0])
                    {
                        matchingEntries.Add(entry);
                    }
                }
            }
            output = matchingEntries.ConvertToMatchupEntryModel(); //這樣做 就只會針對我們要找的那一個id而去Load the file, 這樣做的原因是 當在同一個file裡面不可能存在兩個一樣的id
                                                                   //所以之前的用法 會發生自己找自己的情況 所以會出現無限迴圈 但現在我只針對我要找的id 就不會發生這樣的問題了
                                                                   //下面的LookupTeamById & LookupMatchupById 也是一樣的道理
            return output;
        }

        private static TeamModel LookupTeamById(int id)
        {
            List<string> teams = GlobalConfig.TeamFile.FullFilePath().LoadFile();

            foreach (string team in teams)
            {
                string[] cols = team.Split(',');
                if (cols[0] == id.ToString())
                {
                    List<string> matchingTeam = new List<string>();
                    matchingTeam.Add(team);
                    return matchingTeam.ConvertToTeamModels().First();
                }
            }

            return null;
        }

        private static MatchupModel LookupMatchupById(int id)
        {
            List<string> matchups = GlobalConfig.MatchupFile.FullFilePath().LoadFile();

            foreach (string matchup in matchups)
            {
                string[] cols = matchup.Split(',');
                if (cols[0] == id.ToString())
                {
                    List<string> matchingMatchups = new List<string>();
                    matchingMatchups.Add(matchup);
                    return matchingMatchups.ConvertToMatchupModel().First();
                }
            }

            return null;
        }

        public static List<MatchupEntryModel> ConvertToMatchupEntryModel(this List<string> lines)
        {
            // id=0
            // TeamCompeting=1  id
            // Score=2
            // ParentMatchup=3  id

            List<MatchupEntryModel> output = new List<MatchupEntryModel>();

            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                MatchupEntryModel me = new MatchupEntryModel();

                me.Id = int.Parse(cols[0]);

                if (cols[1].Length == 0)
                {
                    me.TeamCompeting = null;
                }
                else
                {
                    me.TeamCompeting = LookupTeamById(int.Parse(cols[1]));
                }
                me.Score = double.Parse(cols[2]);

                int parentId = 0;
                if (int.TryParse(cols[3], out parentId))
                {
                    me.ParentMatchup = LookupMatchupById(parentId);
                }
                else
                {
                    me.ParentMatchup = null;
                }

                output.Add(me);
            }
            return output;
        }

        public static void SaveMatchupToFile(this MatchupModel matchup)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupFile
                .FullFilePath()
                .LoadFile()
                .ConvertToMatchupModel();
            List<string> lines = new List<string>();

            int currentId = 1;
            if (matchups.Count > 0)
            {
                currentId = matchups.OrderByDescending(x => x.Id).First().Id + 1;
            }

            matchup.Id = currentId;
            matchups.Add(matchup);

            foreach (MatchupEntryModel entry in matchup.Entris)
            {
                entry.SaveEntryToFile();
            }

            // save to file
            lines = new List<string>();

            foreach (MatchupModel m in matchups)
            {
                string winner = "";
                if (m.Winner != null)

                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{ m.Id },{ ConvertMatchupEntryListToString(m.Entris) },{ winner },{ m.MatchupRound }");
            }
            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }

        public static void UpdateMatchupToFile(this MatchupModel matchup)
        {
            List<MatchupModel> matchups = GlobalConfig.MatchupFile
                .FullFilePath()
                .LoadFile()
                .ConvertToMatchupModel();
            List<string> lines = new List<string>();

            MatchupModel oldMatchup = new MatchupModel();
            foreach (MatchupModel m in matchups)
            {
                if (m.Id == matchup.Id)
                {
                    oldMatchup = m;
                }
            }

            matchups.Remove(oldMatchup);
            matchups.Add(matchup);


            foreach (MatchupEntryModel entry in matchup.Entris)
            {
                entry.UpdateEntryToFile();
            }

            // save to file
            lines = new List<string>();

            // 因為在更新winner && Score 的時候是把對應的id拿出來進行更新的 所以重新寫入的時候id 會錯亂 所以這邊用orderby 重新整理一次
            IEnumerable<MatchupModel> sortMatchups = matchups.OrderBy(x => x.Id);

            foreach (MatchupModel m in sortMatchups)
            {
                string winner = "";
                if (m.Winner != null)

                {
                    winner = m.Winner.Id.ToString();
                }
                lines.Add($"{ m.Id },{ ConvertMatchupEntryListToString(m.Entris) },{ winner },{ m.MatchupRound }");
            }
            File.WriteAllLines(GlobalConfig.MatchupFile.FullFilePath(), lines);
        }

        public static void SaveEntryToFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile
                .FullFilePath()
                .LoadFile()
                .ConvertToMatchupEntryModel();
            List<string> lines = new List<string>();


            int currentId = 1;
            if (entries.Count > 0)
            {
                currentId = entries.OrderByDescending(x => x.Id).First().Id + 1;
            }

            entry.Id = currentId;
            entries.Add(entry);

            //save to file
            foreach (MatchupEntryModel e in entries)
            {
                string parent = "";
                if (e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                string teamCompete = "";
                if (e.TeamCompeting != null)
                {
                    teamCompete = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{ e.Id },{ teamCompete },{ e.Score },{ parent }");
            }
            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        public static void UpdateEntryToFile(this MatchupEntryModel entry)
        {
            List<MatchupEntryModel> entries = GlobalConfig.MatchupEntryFile
                .FullFilePath()
                .LoadFile()
                .ConvertToMatchupEntryModel();
            List<string> lines = new List<string>();
            MatchupEntryModel oldMatchupEntryModel = new MatchupEntryModel();

            foreach (MatchupEntryModel e in entries)
            {
                if (e.Id == entry.Id)
                {
                    oldMatchupEntryModel = e;
                }
            }
            entries.Remove(oldMatchupEntryModel);
            entries.Add(entry);

            IEnumerable<MatchupEntryModel> sortEntries = entries.OrderBy(x => x.Id);
            //save to file
            foreach (MatchupEntryModel e in sortEntries)
            {
                string parent = "";
                if (e.ParentMatchup != null)
                {
                    parent = e.ParentMatchup.Id.ToString();
                }
                string teamCompete = "";
                if (e.TeamCompeting != null)
                {
                    teamCompete = e.TeamCompeting.Id.ToString();
                }
                lines.Add($"{ e.Id },{ teamCompete },{ e.Score },{ parent }");
            }
            File.WriteAllLines(GlobalConfig.MatchupEntryFile.FullFilePath(), lines);
        }

        private static string ConvertPeopleListToString(List<PersonModel> people)
        {
            string output = "";

            if (people.Count == 0)
            {
                return "";
            }

            // 2|3|
            foreach (PersonModel p in people)
            {
                output += $"{p.Id}|";
            }
            output = output.Substring(0, output.Length - 1);

            // 2|3
            return output;
        }

        private static string ConvertTeamListToString(List<TeamModel> teams)
        {
            string output = "";
            if (teams.Count == 0)
            {
                return "";
            }

            foreach (TeamModel t in teams)
            {
                output += $"{t.Id}|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertPrizeListToString(List<PrizeModel> teams)
        {
            string output = "";
            if (teams.Count == 0)
            {
                return "";
            }

            foreach (PrizeModel p in teams)
            {
                output += $"{p.Id}|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertRoundListToString(List<List<MatchupModel>> rounds)
        {
            //(Rounds - id^id^id|id^id^id|id^id^id)

            string output = "";
            if (rounds.Count == 0)
            {
                return "";
            }

            foreach (List<MatchupModel> r in rounds)
            {
                output += $"{ ConvertMatchupListToString(r) }|";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }

        private static string ConvertMatchupListToString(List<MatchupModel> matchups)
        {
            string output = "";
            if (matchups.Count == 0)
            {
                return "";
            }

            foreach (MatchupModel m in matchups)
            {
                output += $"{m.Id}^";
            }
            output = output.Substring(0, output.Length - 1);

            return output;
        }
        
        private static string ConvertMatchupEntryListToString(List<MatchupEntryModel> matchupEntry)
        {
            string output = "";
            if (matchupEntry.Count == 0)
            {
                return "";
            }

            foreach (MatchupEntryModel m in matchupEntry)
            {
                output += $"{ m.Id }|";
            }

            output = output.Substring(0, output.Length - 1);
            return output;
        }

        // trying Generic method
        //private static string ConvertListToString<T>(List<T> model) where T : 
        //{
        //    string output = "";
        //    if (model.Count == 0)
        //    {
        //        return "";
        //    }

        //    foreach (var p in model)
        //    {
        //        output += $"{p.Id}|";
        //    }
        //    output = output.Substring(0, output.Length - 1);

        //    return output;
        //}
    }
}
