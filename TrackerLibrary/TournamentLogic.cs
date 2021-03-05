﻿using System;
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
                // Alert users
                //EmailLogic.SendEmail();
            }
        }



        private static int CheckCurrentRound(this TournamentModel model)
        {
            int output = 1;

            foreach (List<MatchupModel> round in model.Rounds)
            {
                output += 1;
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
