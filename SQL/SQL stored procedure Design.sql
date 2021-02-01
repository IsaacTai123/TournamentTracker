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


CREATE PROCEDURE dbo.spPrizes_Insert
	@PlaceNumber int,
	@PlaceName nvarchar(50),
	@PrizeAmount money,
	@PrizePercentage float,
	@id int = 0 output
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO dbo.Prizes (PlaceNumber, PlaceName, PrizeAmount, PrizePercentage)
	VALUES (@PlaceNumber, @PlaceName, @PrizeAmount, @PrizePercentage);

	select @id = SCOPE_IDENTITY(); -- 把最新輸入的Id拿出來, the last identity which is the new Id we just insert into.
END
GO

CREATE PROCEDURE dbo.spPeople_Insert
	@FirstName nvarchar(100),
	@LastName nvarchar(100),
	@EmailAddress nvarchar(100),
	@CellphoneNumber varchar(20),
	@id int = 0 out
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	insert into dbo.People (FirstName, Lastname, EmailAddress, CellphoneNumber)
	values (@FirstName, @Lastname, @EmailAddress, @CellphoneNumber);

	select @id = SCOPE_IDENTITY();
END
GO


CREATE PROC dbo.spPeople_GetAll
AS
BEGIN
	set nocount on;

	select * from People;
END


CREATE PROC dbo.spTeams_Insert
	@TeamName nvarchar(100),
	@id int = 0 output
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO dbo.Teams (TeamName)
	VALUES (@TeamName);

	SELECT @id = SCOPE_IDENTITY();
END
GO


CREATE PROC dbo.spTeamMembers_Insert
	@TeamId int,
	@PersonId int,
	@id int = 0 out
AS
BEGIN
	SET  NOCOUNT ON;

	INSERT INTO TeamMembers (TeamId, PersonId)
	VALUES (@TeamId, @PersonId);

	SELECT @id = SCOPE_iDENTITY();
END
GO