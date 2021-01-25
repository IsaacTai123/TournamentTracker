Create proc spMatchups_GetByTournament
	@TournamentId int
as
begin
	set nocount on;

	select m.id, m.WinnerId, m.MatchupRound 
	from TournamentEntries t
	join Matchups m
	on t.TeamId = m.id
	where TournamentId = @TournamentId;
end