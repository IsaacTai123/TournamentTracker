

Select t.TournamentName, t.EntryFee, t.Active, m.WinnerId, m.MatchupRound, me.ParentMatchupId, me.TeamCompetingId, me.Score
from dbo.Tournaments t
inner join dbo.Matchups m on t.Id = m.TournamentId
inner join dbo.MatchupEntries me on me.MatchupId = m.Id
inner join dbo.Matchups mp on mp.id = me.ParentMatchupId



USE [Tournaments]
Drop table dbo.Tournaments

CREATE TABLE Tournaments (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	TournamentName nvarchar(100) NOT NULL,
	EntryFee money NOT NULL,
)

Alter table dbo.Tournaments Add Active bit NOT NULL

CREATE TABLE TournamentEntries (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	TournamentId int  NOT NULL,
	TeamId int NOT NULL
)

CREATE TABLE Prizes (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	PlaceNumber int NOT NULL,
	PlaceName nvarchar(50) NOT NULL,
	PrizeAmount money,
	PrizePercentage float
)

CREATE TABLE TournamentPrizes (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	TournamentId int  NOT NULL,
	PrizeId int  NOT NULL
)

CREATE TABLE Teams (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	TeamName nvarchar(100) NOT NULL
)

CREATE TABLE TeamMembers (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	TeamId int NOT NULL,
	PersonId int NOT NULL
)

CREATE TABLE People (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	FirstName nvarchar(100) NOT NULL,
	Lastname nvarchar(100) NOT NULL,
	EmailAddress nvarchar(200) NOT NULL,
	CellphoneNumber varchar(20)
)

CREATE TABLE Matchups (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	WinnerId int NOT NULL,
	MatchupRound int NOT NULL
)

-- ALTER TABLE dbo.Matchups ADD Tournament int NOT NULL AFTER `WinnerId`; this is not allow
alter table dbo.Matchups alter column WinnerId int

CREATE TABLE MatchupEntries (
	id int NOT NULL IDENTITY(1, 1) PRIMARY KEY,
	MatchupId int NOT NULL,
	ParentMatchupId int NOT NULL,
	TeamCompetingId int NOT NULL,
	Score int NOT NULL
)

alter table dbo.MatchupEntries alter column ParentMatchupId int
alter table dbo.MatchupEntries alter column TeamCompetingId int
alter table dbo.MatchupEntries alter column Score float



ALTER TABLE TournamentEntries ADD CONSTRAINT FK_TournamentEntries_TournamentId FOREIGN KEY (TournamentId) REFERENCES Tournaments(id) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE TournamentEntries ADD CONSTRAINT FK_TournamentEntries_TeamId FOREIGN KEY (TeamId) REFERENCES Teams(id) ON DELETE CASCADE ON UPDATE CASCADE

ALTER TABLE TournamentPrizes ADD CONSTRAINT FK_TournamentPrizes_TournamentId FOREIGN KEY (TournamentId) REFERENCES Tournaments(id) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE TournamentPrizes ADD CONSTRAINT FK_TournamentPrizes_PrizeId FOREIGN KEY (PrizeId) REFERENCES Prizes(id) ON DELETE CASCADE ON UPDATE CASCADE

ALTER TABLE TeamMembers ADD CONSTRAINT FK_TeamMembers_TeamId FOREIGN KEY (TeamId) REFERENCES Teams(id) ON DELETE CASCADE ON UPDATE CASCADE
ALTER TABLE TeamMembers ADD CONSTRAINT FK_TeamMembers_PersonId FOREIGN KEY (PersonId) REFERENCES People(id) ON DELETE CASCADE ON UPDATE CASCADE

ALTER TABLE Matchups ADD CONSTRAINT FK_Matchups_WinnerId FOREIGN KEY (WinnerId) REFERENCES Teams(id) ON DELETE CASCADE ON UPDATE CASCADE

ALTER TABLE MatchupEntries ADD CONSTRAINT FK_MatchupEntries_MatchupId FOREIGN KEY (MatchupId) REFERENCES Matchups(id)
ALTER TABLE MatchupEntries ADD CONSTRAINT FK_MatchupEntries_ParentMatchupId FOREIGN KEY (ParentMatchupId) REFERENCES Matchups(id)
ALTER TABLE MatchupEntries ADD CONSTRAINT FK_MatchupEntries_TeamCompetingId FOREIGN KEY (TeamCompetingId) REFERENCES Teams(id)

ALTER TABLE Matchups ADD CONSTRAINT FK_Matchups_TournamentId FOREIGN KEY (TournamentId) REFERENCES Tournaments(id) ON DELETE CASCADE ON UPDATE CASCADE
 

ALTER TABLE dbo.Prizes
ADD CONSTRAINT DF_Prizes_PrizeAmount
DEFAULT 0 FOR PrizeAmount

ALTER TABLE dbo.Prizes
ADD CONSTRAINT DF_Prizes_PrizePercentage
DEFAULT 0 FOR PrizePercentage