use SimpleQDB;
go

set nocount on;
go

drop table DsgvoConstraint;
drop table Vote;
drop table SpecifiedTextAnswer;
drop table Answer;
drop table Asking;
drop table Survey;
drop table SurveyCategory;
drop table AnswerType;
drop table AskedPerson;
--drop table [Contains];
--drop table [Group];
drop table Department;
drop table Bill;
drop table Customer;
drop table PaymentMethod;
go

-- Zahlungsmethode
-- Nicht kundenabhängig
create table PaymentMethod
(
	PaymentMethodId int primary key,
	PaymentMethodDesc varchar(max) not null
);
go

-- Kunde
create table Customer
(
	CustCode char(6) collate Latin1_General_CS_AS primary key,
	CustName varchar(max) not null,
	CustEmail varchar(max) not null,
	CustPwdTmp varchar(max),
	CustPwdHash varbinary(max),
	Street varchar(max) not null,
	Plz varchar(16) not null,
	City varchar(max) not null,
	Country varchar(max) not null,
	LanguageCode char(3) not null,
	DataStoragePeriod int not null, -- in Monaten
	PaymentMethodId int not null references PaymentMethod,
	CostBalance money not null
);
go

-- Rechnung
-- Kundenabhängig
create table Bill
(
	BillId int identity,
	CustCode char(6) collate Latin1_General_CS_AS references Customer,
	BillAmount money not null,
	BillDate datetime not null,
	Paid bit not null,
	primary key (BillId, CustCode)
);
go

-- Abteilung
-- Kundenabhängig
create table Department
(
	DepId int identity,
	DepName varchar(max) not null,
	CustCode char(6) collate Latin1_General_CS_AS references Customer,
	primary key (DepId, CustCode)
);
go
/*
-- Befragtenruppe
-- Kundenabhängig
create table [Group] 
(
	GroupId int identity,
	CustCode char(6) collate Latin1_General_CS_AS references Customer,
	GroupDesc varchar(max) not null,
	primary key (GroupId, CustCode)
);
go

-- Information welche Gruppe wieviele Personen aus welchen Abteilungen beinhaltet
-- Kundenabhängig
create table [Contains]
(
	GroupId int,
	DepId int,
	CustCode char(6) collate Latin1_General_CS_AS, 
	Amount int not null,
	primary key (GroupId, DepId, CustCode),
	foreign key (DepId, CustCode) references Department,
	foreign key (GroupId, CustCode) references [Group]
);
go
*/

-- Befragte Person
-- Kundenabhängig
create table AskedPerson
(
	PersId int identity,
	DepId int not null,
	CustCode char(6) collate Latin1_General_CS_AS,
	primary key (PersId, CustCode),
	foreign key (DepId, CustCode) references Department
);
go

-- Beantwortungsart
-- Nicht kundenabhängig
create table AnswerType
(
	TypeId int primary key,
	TypeDesc varchar(max) not null
);
go

-- Umfragekategorie
-- Kundenabhängig
create table SurveyCategory
(
	CatId int identity,
	CustCode char(6) collate Latin1_General_CS_AS references Customer, 
	CatName varchar(max) not null,
	primary key (CatId, CustCode)
);
go

-- Umfrage
-- Kundenabhängig
create table Survey
(
	SvyId int identity,
	CustCode char(6) collate Latin1_General_CS_AS references Customer,
	SvyText varchar(max) not null,
	StartDate datetime not null,
	EndDate datetime not null,
	TypeId int not null references AnswerType,
	CatId int not null
	primary key (SvyId, CustCode),
	foreign key (CatId, CustCode) references SurveyCategory
);
go

-- Mit Umfrage befragte Gruppen
-- Kundenabhängig
create table Asking
(
	SvyId int,
	DepId int,
	CustCode char(6) collate Latin1_General_CS_AS,
	primary key (SvyId, DepId, CustCode),
	foreign key (SvyId, CustCode) references Survey,
	foreign key (DepId, CustCode) references Department
);
go

-- AntwortMÖGLICHKEIT
-- Nicht kundenabhängig
create table Answer
(
	AnsId int primary key,
	AnsDesc varchar(max) not null,
	TypeId int not null references AnswerType
);
go

-- Vorgegebene Textantwort (wird nur benötigt falls entsprechender Antworttyp für die Umfrage verwendet wird)
-- kundenabhängig
create table SpecifiedTextAnswer
(
	SpecId int identity,
	SvyId int,
	CustCode char(6) collate Latin1_General_CS_AS,
	SpecText varchar(max),
	primary key (SpecId, SvyId, CustCode),
	foreign key (SvyId, CustCode) references Survey
);
go

-- Antwort auf eine Umfrage
-- Kundenabhängig
create table Vote
(
	VoteId int identity primary key,
	SvyId int not null,
	CustCode char(6) collate Latin1_General_CS_AS,
	AnsId int not null references Answer,
	VoteText varchar(max) null, -- optional, nur wenn Antworttyp 1-Wort-Antwort
	SpecId int null, -- optional, nur wenn Antworttyp vorgegebene Textantwort
	foreign key (SvyId, CustCode) references Survey,
	foreign key (SpecId, SvyId, CustCode) references SpecifiedTextAnswer
);
go

-- DSGVO-spezifische Bestimmung
-- Nicht kundenabhängig
create table DsgvoConstraint
(
	ConstrName varchar(512) primary key,
	ConstrValue int not null,
);
go



/*
-- Alle Gruppen mit ungültigen Gruppengrößen
drop view vw_InvalidGroupSizes;
go
create view vw_InvalidGroupSizes as
select g.GroupId, GroupDesc, sum(Amount) as Amount, ConstrValue as MinSize
from [Group] g
join [Contains] c on g.GroupId = c.GroupId
cross join DsgvoConstraint d
where d.ConstrName = 'MIN_GROUP_SIZE'
group by g.GroupId, GroupDesc, ConstrValue
having sum(Amount) < ConstrValue;
go*/



-- Hasht das Passwort und setzt das im Klartext eingegebene NULL
create trigger tr_CustomerInsUpd
on Customer
after insert, update as
begin
	declare @CustCode char(6), @hash varbinary(max);
	declare c cursor local for select CustCode, hashbytes('SHA2_512', CustPwdTmp) from inserted;
	
	open c;
	fetch c into @CustCode, @hash;

	while(@@FETCH_STATUS = 0)
	begin
		if(@hash is not null)
		begin
			update Customer set CustPwdHash = @hash, CustPwdTmp = null
			where CustCode = @CustCode
		end
		
		fetch c into @CustCode, @hash
	end

	close c;
	deallocate c;
end
go



-- Zum Löschen der abgelaufenen Umfragedaten
drop procedure sp_CheckExceededSurveyData;
go
create procedure sp_CheckExceededSurveyData
as
begin
	declare @svyId int, @svyCount int;
	declare c cursor local for select svyId from Survey s
											join Customer c on s.CustCode = c.CustCode
											where datediff(day, s.StartDate, getdate()) >= c.DataStoragePeriod * 30;
	
	open c;
	fetch c into @svyId;
	
	set @svyCount = 0;

	while(@@FETCH_STATUS = 0)
	begin
		delete from Vote where SvyId = @svyId;
		delete from SpecifiedTextAnswer where SvyId = @svyId;
		delete from Asking where SvyID = @svyId;
		delete from Survey where SvyId = @svyId;
		
		set @svyCount += 1;
		fetch c into @svyId;
	end
	close c;
	deallocate c;

	select @svyCount as 'Deleted surveys'
end
go

-- Fixe inserts (für alle gleich!)
begin transaction;
insert into DsgvoConstraint values ('MIN_GROUP_SIZE', 3); -- Nur Testwert
insert into PaymentMethod values (1, 'Visa'); -- Nur Testwert

insert into AnswerType values (1, 'YesNo');
insert into AnswerType values (2, 'TrafficLight');
insert into AnswerType values (3, 'OneWord');
insert into AnswerType values (4, 'SpecifiedText');
insert into Answer values (1, 'Yes', 1);
insert into Answer values (2, 'No', 1);
insert into Answer values (3, 'Green', 2);
insert into Answer values (4, 'Yellow', 2);
insert into Answer values (5, 'Red', 2);
insert into Answer values (6, 'OneWord', 3);
insert into Answer values (7, 'SpecifiedText', 4);
commit;
go