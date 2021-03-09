using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using TrackerLibrary.Models;

namespace TrackerLibrary
{
    public static class TournamentLogic
    {
        // TODO - Create our matchups
        // Order our list randomly of teams
        // Check if it is big enough - if not, add in byes (加入一個空的team)
        // 2 * 2 * 2 * 2 這個tournament 的team 必須是2的倍數 才能運作
        // Create our first round of matchups
        // Create every round after that - 8 matchups - 4 matchups - 2 matchups - 1 matchups

        public static void CreateRounds(TournamentModel model)
        {
            List<TeamModel> randomizedTeams = RandomizeTeamOrder(model.EnteredTeams);
            int rounds = FindNumberOfRounds(randomizedTeams.Count);
            int byes = NumberOfByes(rounds, randomizedTeams.Count);
            model.Rounds.Add(CreateFirstRound(byes, randomizedTeams));

            CreateOtherRounds(model, rounds); // again we're not passing back a value, because we're operating
                                              // directly on our tournament model instance therefore no pass back 
                                              // is necessary
        }

        public static void UpdateTournamentResults(TournamentModel model)
        {
            int startingRound = model.CheckCurrentRound();
            // the matchup need to be score
            List<MatchupModel> toScore = new List<MatchupModel>();

            foreach (List<MatchupModel> round in model.Rounds)
            {
                foreach (MatchupModel rm in round)
                {
                    if (rm.Winner == null && (rm.Entris.Any(x => x.Score != 0) || rm.Entris.Count == 1))
                    {
                        toScore.Add(rm);
                    } 
                }
            }

            MarkWinnerInMatchups(toScore);
            AdvanceWinners(toScore, model);

            toScore.ForEach(x => GlobalConfig.Connection.UpdateMatchup(x));
            int endingRound = model.CheckCurrentRound();
            if (endingRound > startingRound)
            {
                model.AlterUsersToNewRound();
            }
        }

        public static void AlterUsersToNewRound(this TournamentModel model)
        {
            int currentRoundNumber = model.CheckCurrentRound();
            List<MatchupModel> currentRound = model.Rounds.Where(x => x.First().MatchupRound == currentRoundNumber).First();

            foreach (MatchupModel matchup in currentRound)
            {
                foreach (MatchupEntryModel me in matchup.Entris)
                {
                    foreach (PersonModel p in me.TeamCompeting.TeamMembers)
                    {
                        AlterPersonToNewRound(p, me.TeamCompeting.TeamName, matchup.Entris.Where(x => x.TeamCompeting != me.TeamCompeting).FirstOrDefault());
                    }
                }
            }
        }

        private static void AlterPersonToNewRound(PersonModel p, string teamName, MatchupEntryModel competitor)
        {
            // To validate email you can use regular Expression or regex
            if (p.EmailAddress.Length == 0)
            {
                // if there is no Email, then we dont do anything.
                return;
            }

            string to = "";
            string subject = "";
            StringBuilder body = new StringBuilder();
            
            if (competitor != null)
            {
                subject = $"You have a new matchup with { competitor.TeamCompeting.TeamName }";

                body.AppendLine("<h1>You have a new matchup</h1>");
                body.Append("<strong>Competitor: </strong>");
                body.Append(competitor.TeamCompeting.TeamName);
                body.AppendLine();
                body.AppendLine();
                body.AppendLine("Have a great time!");
                body.AppendLine("~ Tournament Tracker");
            }
            else
            {
                subject = "You have a bye week this round";
                body.AppendLine("Enjoy your rond off!");
                body.AppendLine("~ Tournament Tracker");
            }

            to = p.EmailAddress;


            EmailLogic.SendEmail(to, subject, body.ToString());
        }

        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;

            foreach (List<MatchupModel> round in model.Rounds)
            {
                if (round.All(x => x.Winner != null))
                {
                    output += 1; 
                }
                else
                {
                    return output;
                }
            }

            // 如果程式跑到這裡 就代表他在上面沒有找到任何的winner = null, 也就是說我們的tournament 完成所有比賽了 所以這邊就要結束這個tournament
            // Tournament is complete
            CompleteTournament(model);
            return output - 1;
        }

        private static void CompleteTournament(TournamentModel model)
        {
            GlobalConfig.Connection.CompleteTournament(model);
            TeamModel winners = model.Rounds.Last().First().Winner;
            TeamModel runnerUp = model.Rounds.Last().First().Entris.Where(x => x.TeamCompeting != winners).First().TeamCompeting;

            decimal winnerPrize = 0;
            decimal runnerUpPrize = 0;


            if (model.Prizes.Count > 0)
            {
                decimal totalIncome = model.EnteredTeams.Count * model.Entryfee;
                PrizeModel firstPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 1).FirstOrDefault();
                PrizeModel SecondPlacePrize = model.Prizes.Where(x => x.PlaceNumber == 2).FirstOrDefault();

                if (firstPlacePrize != null)
                {
                    winnerPrize = firstPlacePrize.CalucatePrizePayout(totalIncome);
                }
                if (SecondPlacePrize != null)
                {
                    runnerUpPrize = SecondPlacePrize.CalucatePrizePayout(totalIncome);
                }
            }

            // Send Email to all users
            string subject = "";

            subject = $"In { model.TournamentName }, { winners.TeamName } has won this Tournament!!";
            StringBuilder body = new StringBuilder();
            body.AppendLine("<h1>We have a WINNER!</h1>");
            body.Append("<p>congratulations to our winner on a great tournament</p>");
            body.AppendLine("<br/>");

            if (winnerPrize > 0)
            {
                body.AppendLine($"<p>{ winners.TeamName } will receive { winnerPrize }</p>");
            }
            if (runnerUpPrize > 0)
            {
                body.AppendLine($"<p>{ runnerUp.TeamName } will receive { runnerUpPrize }</p>");
            }
            body.AppendLine("<p>Thanks for a great tournament everyone!");
            body.AppendLine("~Tournament Tracker");

            List<string> bcc = new List<string>();
            foreach (TeamModel t in model.EnteredTeams)
            {
                foreach (PersonModel p in t.TeamMembers)
                {
                    if (p.EmailAddress.Length > 0)
                    {
                        bcc.Add(p.EmailAddress);
                    }
                }
            }
            EmailLogic.SendEmail(new List<string>(), bcc, subject, body.ToString());

            // Complete Tournament
            model.CompleteTournament();
        }

        private static decimal CalucatePrizePayout(this PrizeModel prize, decimal totalIncome)
        {
            decimal output = 0;

            if (prize.PrizeAmount > 0)
            {
                output = prize.PrizeAmount;
            }
            else
            {
                output = Decimal.Multiply(totalIncome, Convert.ToDecimal(prize.PrizePercentage / 100));
            }

            return output;
        }

        private static void AdvanceWinners(List<MatchupModel> models, TournamentModel tournament)
        {
            //// update the teamCompeting for the next round
            foreach (MatchupModel m in models)
            {
                foreach (List<MatchupModel> matchups in tournament.Rounds)
                {
                    foreach (MatchupModel matchup in matchups)
                    {
                        foreach (MatchupEntryModel me in matchup.Entris)
                        {
                            if (me.ParentMatchup != null)
                            {
                                if (me.ParentMatchup.Id == m.Id)
                                {
                                    me.TeamCompeting = m.Winner;
                                    GlobalConfig.Connection.UpdateMatchup(matchup);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void MarkWinnerInMatchups(List<MatchupModel> models)
        {
            // greater or lesser
            string greaterWins = ConfigurationManager.AppSettings["greaterWins"];

            foreach (MatchupModel m in models)
            {
                // Checks for bye week entry
                if (m.Entris.Count == 1)
                {
                    m.Winner = m.Entris[0].TeamCompeting;
                    continue;
                }

                // 0 means false, or low score wins
                if (greaterWins == "0")
                {
                    if (m.Entris[0].Score < m.Entris[1].Score)
                    {
                        m.Winner = m.Entris[0].TeamCompeting;
                    }
                    else if (m.Entris[1].Score < m.Entris[0].Score)
                    {
                        m.Winner = m.Entris[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("we do not allow ties in this application");
                    }
                }
                else
                {
                    // 1 means true, or high score wins
                    if (m.Entris[0].Score > m.Entris[1].Score)
                    {
                        m.Winner = m.Entris[0].TeamCompeting;
                    }
                    else if (m.Entris[1].Score > m.Entris[0].Score)
                    {
                        m.Winner = m.Entris[1].TeamCompeting;
                    }
                    else
                    {
                        throw new Exception("we do not allow ties in this application");
                    }
                } 
            }
        }

        private static void CreateOtherRounds(TournamentModel model, int rounds)
        {
            int round = 2;
            List<MatchupModel> previousRound = model.Rounds[0];
            List<MatchupModel> currRound = new List<MatchupModel>();
            MatchupModel currMatchup = new MatchupModel();

            while (round <= rounds)
            {
                foreach (MatchupModel match in previousRound)
                {
                    currMatchup.Entris.Add(new MatchupEntryModel { ParentMatchup = match });

                    if (currMatchup.Entris.Count > 1)
                    {
                        currMatchup.MatchupRound = round;
                        currRound.Add(currMatchup);
                        currMatchup = new MatchupModel();
                    }
                }

                model.Rounds.Add(currRound);
                previousRound = currRound;

                currRound = new List<MatchupModel>();
                round += 1;
            }
        }

        private  static List<MatchupModel> CreateFirstRound(int byes, List<TeamModel> teams)
        {
            List<MatchupModel> output = new List<MatchupModel>();
            MatchupModel curr = new MatchupModel();

            foreach (TeamModel team in teams)
            {
                curr.Entris.Add(new MatchupEntryModel { TeamCompeting = team });

                if (byes > 0 || curr.Entris.Count > 1)
                {
                    curr.MatchupRound = 1;
                    output.Add(curr);
                    curr = new MatchupModel();
                }

                if (byes > 0)
                {
                    byes -= 1;
                }
            }

            return output;
        }

        private static int NumberOfByes(int rounds, int numberOfTeams)
        {
            //Math.Pow(2, rounds); // 2 ^ rounds

            int output = 0;
            int totalTeams = 1;

            for (int i = 1; i <= rounds; i++)
            {
                totalTeams *= 2;
            }

            output = totalTeams - numberOfTeams;
            return output;
        }

        private static int FindNumberOfRounds(int teamCount)
        {
            int output = 1;
            int val = 2;

            while (val < teamCount)
            {
                output += 1;
                val *= 2;
            }

            return output;
        }

        private static List<TeamModel> RandomizeTeamOrder(List<TeamModel> teams)
        {
            // teams.OrderBy(a => Guid.NewGuid()).ToList();

            Random random = new Random();
            var shuffleTeamModel = teams.OrderBy(x => random.Next()).ToList();
            return shuffleTeamModel;
        }
    }
}
