using System;
using System.Collections.Generic;
using System.Text;

namespace TrackerLibrary.Models
{
    public class MatchupEntryModel
    {
        /// <summary>
        /// The unique identifier for the matchupEntry
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The unique identifier for the team
        /// </summary>
        public int TeamCompetingId { get; set; }

        /// <summary>
        /// Represents one team in the matchup
        /// </summary>
        public TeamModel TeamCompeting { get; set; }

        /// <summary>
        /// Represents the score for this team
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The unique identifier for the parent metchup (team)
        /// </summary>
        public int ParentMatchupId { get; set; }

        /// <summary>
        /// Represnts the matchup that this team came
        /// from as the winner
        /// </summary>
        public MatchupModel ParentMatchup { get; set; }
    }
}
