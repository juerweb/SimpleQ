use SimpleQDB;
go

set nocount on;
go

drop table DsgvoConstraint;
drop table Chooses;
drop table Vote;
drop table AnswerOption;
drop table Asking;
drop table Survey;
drop table SurveyCategory;
drop table PredefinedAnswerOption;
drop table AnswerType;
drop table BaseQuestionType;
drop table Person;
drop table Department;
drop table Bill;
drop table Customer;
drop table PaymentMethod;
go


-- Zahlungsmethode
-- NICHT KUNDENSPEZIFISCH
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
	CustPwdTmp varchar(max) collate Latin1_General_CS_AS null,
	CustPwdHash varbinary(max) null,
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
-- KUNDENSPEZIFISCH
create table Bill
(
	BillId int identity primary key,
	CustCode char(6) collate Latin1_General_CS_AS not null references Customer,
	BillPrice money not null,
	BillDate datetime not null,
	Paid bit not null
);
go

-- Abteilung
-- KUNDENSPEZIFISCH
create table Department
(
	DepId int identity not null,
	DepName varchar(max) not null,
	CustCode char(6) collate Latin1_General_CS_AS not null references Customer,
    primary key (DepId, CustCode)
);
go

-- Befragte Person
-- KUNDENSPEZIFISCH
create table Person
(
	PersId int identity primary key,
	DepId int not null,
    CustCode char(6) collate Latin1_General_CS_AS not null,
    DeviceId varchar(max) null,
    foreign key (DepId, CustCode) references Department
);
go

-- Fragetyp (Dichotom, Polytom, ...)
-- NICHT KUNDENSPEZIFISCH
create table BaseQuestionType
(
    BaseId int primary key,
    BaseDesc varchar(max) not null
);
go

-- Beantwortungsart
-- NICHT KUNDENSPEZIFISCH
create table AnswerType
(
	TypeId int primary key,
	TypeDesc varchar(max) not null,
    BaseId int not null references BaseQuestionType
);
go

-- Vordefinierte Antwortmöglichkeit
-- NICHT KUNDENSPEZIFISCH
create table PredefinedAnswerOption
(
    PreAnsId int primary key,
    PreAnsText varchar(max) not null,
    TypeId int not null references AnswerType
);
go

-- Umfragekategorie
-- KUNDENSPEZIFISCH
create table SurveyCategory
(
	CatId int identity primary key,
	CustCode char(6) collate Latin1_General_CS_AS not null references Customer, 
	CatName varchar(max) not null
);
go

-- Umfrage
-- KUNDENSPEZIFISCH
create table Survey
(
	SvyId int identity primary key,
	CatId int not null references SurveyCategory,
	CustCode char(6) collate Latin1_General_CS_AS not null references Customer,
	SvyText varchar(max) not null,
	StartDate datetime not null,
	EndDate datetime not null,
	TypeId int not null references AnswerType
);
go

-- Mit Umfrage befragte Abteilungen
-- KUNDENSPEZIFISCH
create table Asking
(
	SvyId int references Survey,
	DepId int not null,
    CustCode char(6) collate Latin1_General_CS_AS not null,
	primary key (SvyId, DepId, CustCode),
    foreign key (DepId, CustCode) references Department
);
go

-- Antwortmöglichkeit
-- KUNDENSPEZIFISCH
create table AnswerOption
(
	AnsId int identity primary key,
    SvyId int not null references Survey,
	AnsText varchar(max) not null
);
go

-- Antwort auf eine Umfrage
-- KUNDENSPEZIFISCH
create table Vote
(
	VoteId int identity primary key,
	VoteText varchar(max) null, -- optional, nur wenn Antworttyp 1-Wort-Antwort
);
go

-- Mit Umfragenantwort ausgewählte Antwortmöglichkeit(en)
-- KUNDENSPEZIFISCH
create table Chooses
(
    VoteId int references Vote on delete cascade,
    AnsId int not null references AnswerOption
    primary key (VoteId, AnsId)
);
go


-- DSGVO-spezifische Bestimmung
-- NICHT KUNDENSPEZIFISCH
create table DsgvoConstraint
(
	ConstrName varchar(512) primary key,
	ConstrValue int not null,
);
go



-- Hasht das Passwort und setzt das im Klartext Eingegebene NULL
create trigger tr_CustomerIns
on Customer
after insert as
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

-- Hasht das Passwort und setzt das im Klartext Eingegebene NULL (nach Passwortänderung)
create trigger tr_CustomerUpd
on Customer
after update as
begin
    if(trigger_nestlevel() = 1 and update(CustPwdTmp))
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
end
go


-- Fügt bei den entsprechenden Beantwortungsarten die vordefinierten Antworten als Antwortmöglichkeit ein
create trigger tr_SurveyIns
on Survey
after insert as
begin
    declare @svyId int, @typeId int;
    declare c cursor local for select SvyId, TypeId from inserted;

    open c;
    fetch c into @svyId, @typeId;

    while(@@FETCH_STATUS = 0)
    begin
        insert into AnswerOption (SvyId, AnsText) select @svyId, PreAnsText
                                                  from PredefinedAnswerOption
                                                  where TypeId = @typeId;

        fetch c into @svyId, @typeId;
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
	declare c cursor local for select svyId 
                               from Survey s
							   join Customer c on s.CustCode = c.CustCode
							   where datediff(day, s.StartDate, getdate()) >= c.DataStoragePeriod * 30;
	
	open c;
	fetch c into @svyId;
	
	set @svyCount = 0;

	while(@@FETCH_STATUS = 0)
	begin
		delete from Vote where VoteId in (select VoteId
                                          from Chooses c 
                                          join AnswerOption a on c.AnsId = a.AnsId
                                          where a.SvyId = @svyId);
        delete from AnswerOption where SvyId = @svyId;
		delete from Asking where SvyId = @svyId;
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
insert into PaymentMethod values (1, 'SEPA'); -- Nur Testwert

insert into BaseQuestionType values (1, 'OpenQuestion');
insert into BaseQuestionType values (2, 'DichotomousQuestion');
insert into BaseQuestionType values (3, 'PolytomousQuestion');

insert into AnswerType values (1, 'YesNo', 2);
insert into AnswerType values (2, 'YesNoDontKnow', 3);
insert into AnswerType values (3, 'TrafficLight', 3);
insert into AnswerType values (4, 'Open', 1);
insert into AnswerType values (5, 'Dichotomous', 2);
insert into AnswerType values (6, 'PolytomousUnorderedSingle', 3);
insert into AnswerType values (7, 'PolytomousUnorderedMultiple', 3);
insert into AnswerType values (8, 'PolytomousOrderedSingle', 3);
insert into AnswerType values (9, 'PolytomousOrderedMultiple', 3);
insert into AnswerType values (10, 'LikertSkale3', 3);
insert into AnswerType values (11, 'LikertSkale4', 3);
insert into AnswerType values (12, 'LikertSkale5', 3);
insert into AnswerType values (13, 'LikertSkale6', 3);
insert into AnswerType values (14, 'LikertSkale7', 3);
insert into AnswerType values (15, 'LikertSkale8', 3);
insert into AnswerType values (16, 'LikertSkale9', 3);

insert into PredefinedAnswerOption values (1, 'Yes', 1);
insert into PredefinedAnswerOption values (2, 'No', 1);
insert into PredefinedAnswerOption values (3, 'Yes', 2);
insert into PredefinedAnswerOption values (4, 'No', 2);
insert into PredefinedAnswerOption values (5, 'DontKnow', 2);
insert into PredefinedAnswerOption values (6, 'Green', 3);
insert into PredefinedAnswerOption values (7, 'Yellow', 3);
insert into PredefinedAnswerOption values (8, 'Red', 3);
insert into PredefinedAnswerOption values (9, 'FreeText', 4);
commit;
go