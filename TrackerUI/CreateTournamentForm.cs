using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackerLibrary;
using TrackerLibrary.Models;

namespace TrackerUI
{
    public partial class CreateTournamentForm : Form, IPrizeRequester, ITeamRequester
    {
        List<TeamModel> availiableTeams = GlobalConfig.Connection.GetTeam_All();
        List<TeamModel> selectedTeams = new List<TeamModel>();
        List<PrizeModel> selectedPrizes = new List<PrizeModel>();

        public CreateTournamentForm()
        {
            InitializeComponent();
            WireUpLists();
        }

        private void WireUpLists()
        {
            selectTeamDropDown.DataSource = null;
            selectTeamDropDown.DataSource = availiableTeams;
            selectTeamDropDown.DisplayMember = "TeamName";

            tournamentTeamsListBox.DataSource = null;
            tournamentTeamsListBox.DataSource = selectedTeams;
            tournamentTeamsListBox.DisplayMember = "TeamName";

            prizesListBox.DataSource = null;
            prizesListBox.DataSource = selectedPrizes;
            prizesListBox.DisplayMember = "PlaceName";
        }

        private void addTeamButton_Click(object sender, EventArgs e)
        {
            //PersonModel p = (PersonModel)selectTeamMemberDropDown.SelectedItem;

            //if (p != null)
            //{
            //    availableTeammembers.Remove(p);
            //    selectedTeamMembers.Add(p);

            //    WireUpLists(); //call this again to refresh the list

            //    //這幾個方式 沒有用 無法將表格refresh
            //    //selectTeamMemberDropDown.Refresh();
            //    //teamMemberListBox.Refresh(); 
            //}

            // so that takes the selected team in the drop down converts back over to a team model
            TeamModel t = (TeamModel)selectTeamDropDown.SelectedItem; 

            if (t != null)
            {
                availiableTeams.Remove(t);
                selectedTeams.Add(t);

                WireUpLists();
            }
        }

        private void createPrizeButton_Click(object sender, EventArgs e)
        {
            // Call the CreatePrizeForm
            CreatePrizeForm frm = new CreatePrizeForm(this); // "this" keyword represents this specific instance
            frm.Show(); // After clicking the button the form will pop up
        }

        public void PrizeComplete(PrizeModel model)
        {
            // Get back from the form a PrizeModel (important part)
            // Take the PrizeModel and put it into our list of selected prizes
            selectedPrizes.Add(model);
            WireUpLists();
        }

        public void TeamComplete(TeamModel model)
        {
            selectedTeams.Add(model);
            WireUpLists();
        }

        private void createNewTeamLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            CreateTeamForm frm = new CreateTeamForm(this);
            frm.Show();
        }

        private void removeSelectedPlayerButton_Click(object sender, EventArgs e)
        {
            //PersonModel p = (PersonModel)teamMemberListBox.SelectedItem;

            //if (p != null)
            //{
            //    selectedTeamMembers.Remove(p);
            //    availableTeammembers.Add(p);

            //    WireUpLists();
            //}

            TeamModel t = (TeamModel)tournamentTeamsListBox.SelectedItem;

            if (t != null)
            {
                selectedTeams.Remove(t);
                availiableTeams.Add(t);

                WireUpLists();
            }
        }

        private void removeSelectedPrizeButton_Click(object sender, EventArgs e)
        {
            PrizeModel p = (PrizeModel)prizesListBox.SelectedItem;

            if (p != null)
            {
                selectedPrizes.Remove(p);
                WireUpLists();
            }
        }

        private void createTournamentButton_Click(object sender, EventArgs e)
        {
            // Validate data
            decimal fee = 0;
            bool feeAcceptable = decimal.TryParse(entryFeeValue.Text, out fee);

            if (!feeAcceptable)
            {
                MessageBox.Show("You need to enter a valid Entry fee", 
                    "Invalid Fee", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
                return;
            }

            // Create our tournament model
            TournamentModel tm = new TournamentModel();

            tm.TournamentName = tournamentNameValue.Text;
            tm.Entryfee = fee;
            tm.Prizes = selectedPrizes;
            tm.EnteredTeams = selectedTeams;

            // Create our matchups
            TournamentLogic.CreateRounds(tm); // it will put them right in that tournament model variable, because again we don't have to pass it back and forth we pass it in
                                              // now both have that same address that same location therefore they can go on to the next step
            
            // Create Tournament entry
            // Create all of the Prizes entries
            // Create all of team entries
            GlobalConfig.Connection.CreateTournament(tm);
            tm.AlterUsersToNewRound(); //send the Email of the first round when the tournament is generate

            TournamentViewerForm frm = new TournamentViewerForm(tm);
            frm.Show();
            this.Close();
        }
    }
}
