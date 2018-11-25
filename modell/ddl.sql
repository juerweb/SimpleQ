use SimpleQDB;
go

set nocount on;
go

drop table FaqEntry;
drop table DataConstraint;
drop table Chooses;
drop table Vote;
drop table AnswerOption;
drop table Asking;
drop table Survey;
drop table SurveyCategory;
drop table PredefinedAnswerOption;
drop table Activates;
drop table AnswerType;
drop table BaseQuestionType;
drop table Employs;
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
	RegistrationDate date null,
	Street varchar(max) not null,
	Plz varchar(16) not null,
	City varchar(max) not null,
	Country varchar(max) not null,
	LanguageCode char(3) not null,
	DataStoragePeriod int not null check(DataStoragePeriod > 0), -- in Monaten
	AccountingPeriod int not null check(AccountingPeriod in (1, 3, 6, 12)), -- in Monaten
	PaymentMethodId int not null references PaymentMethod,
    MinGroupSize int not null,
	CostBalance money not null
);
go

-- Rechnung
-- KUNDENSPEZIFISCH
create table Bill --Clinton
(
	BillId int identity primary key,
	CustCode char(6) collate Latin1_General_CS_AS not null references Customer,
	BillPrice money not null,
	BillDate datetime not null,
	[Sent] bit not null,
	Paid bit not null
);
go

-- Abteilung
-- KUNDENSPEZIFISCH
create table Department
(
	DepId int not null,
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
    DeviceId varchar(max) null
);
go

-- In Abteilung angestellte Personen
-- KUNDENSPEZIFISCH
create table Employs
(
    DepId int,
    CustCode char(6) collate Latin1_General_CS_AS,
    PersId int references Person,
    primary key (DepId, CustCode, PersId),
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

-- Vom Kunden aktivierte Beantwortungsarten (standardmäßig alle)
-- KUNDENSPEZIFISCH
create table Activates
(
	CustCode char(6) collate Latin1_General_CS_AS references Customer,
	TypeId int references AnswerType,
	primary key (CustCode, TypeId)
);
go

-- Vordefinierte Antwortmöglichkeit
-- NICHT KUNDENSPEZIFISCH
create table PredefinedAnswerOption
(
    PreAnsId int primary key identity,
    PreAnsText varchar(max) not null,
    TypeId int not null references AnswerType
);
go

-- Umfragekategorie
-- KUNDENSPEZIFISCH
create table SurveyCategory
(
	CatId int not null,
	CustCode char(6) collate Latin1_General_CS_AS not null references Customer, 
	CatName varchar(max) not null,
	Deactivated bit not null default 0,
    primary key (CatId, CustCode),
);
go

-- Umfrage
-- KUNDENSPEZIFISCH
create table Survey
(
	SvyId int identity primary key,
	CatId int not null,
	CustCode char(6) collate Latin1_General_CS_AS not null,
	SvyText varchar(max) not null,
	StartDate datetime not null,
	EndDate datetime not null,
    Amount int not null,
	PricePerClick money null,
	TypeId int not null references AnswerType,
	Template bit not null default 0,
	[Sent] bit not null default 0,
    foreign key (CatId, CustCode) references SurveyCategory,
	check (StartDate < EndDate)
);
go

-- Mit Umfrage befragte Abteilungen
-- KUNDENSPEZIFISCH
create table Asking
(
	SvyId int references Survey,
	DepId int not null,
    CustCode char(6) collate Latin1_General_CS_AS not null
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
	AnsText varchar(max) not null,
	FirstPosition bit null default null -- 1: ganz vorne, 0: ganz hinten, NULL: egal
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
create table DataConstraint
(
	ConstrName varchar(512) primary key,
	ConstrValue int not null,
);
go


-- FAQ-Eintrag für Supportbereich
-- NICHT KUNDENSPEZIFISCH
create table FaqEntry
(
    FaqTitle varchar(128) primary key,
    FaqContent varchar(max) not null
);
go



-- Hasht das Passwort und setzt das im Klartext Eingegebene NULL, aktiviert standardmäßig alle AnswerTypes für den neuen Kunden
create trigger tr_CustomerIns
on Customer
after insert as
begin
	declare @CustCode char(6), @hash varbinary(max);
	declare c cursor local for select CustCode, dbo.fn_GetHash(CustPwdTmp) from inserted;
	
	open c;
	fetch c into @CustCode, @hash;

	while(@@FETCH_STATUS = 0)
	begin
	    update Customer set RegistrationDate = getdate()
		where CustCode = @CustCode;

		if(@hash is not null)
		begin
			update Customer set CustPwdHash = @hash, CustPwdTmp = null
			where CustCode = @CustCode
		end

		insert into Activates (CustCode, TypeId) select @CustCode, TypeId
												 from AnswerType;
		
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
	    declare c cursor local for select CustCode, dbo.fn_GetHash(CustPwdTmp) from inserted;
	
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
    declare @svyId int, @typeId int, @amount int;
    declare c cursor local for select SvyId, TypeId, Amount from inserted;

    open c;
    fetch c into @svyId, @typeId, @amount;

    while(@@FETCH_STATUS = 0)
    begin
		update Survey set PricePerClick = dbo.fn_CalcPricePerClick(@amount)
		where SvyId = @svyId;

        insert into AnswerOption (SvyId, AnsText) select @svyId, PreAnsText
                                                  from PredefinedAnswerOption
                                                  where TypeId = @typeId;

        fetch c into @svyId, @typeId, @amount;
    end
    
    close c;
    deallocate c;
end
go

-- Fügt bei den entsprechenden Beantwortungsarten die vordefinierten Antworten als Antwortmöglichkeit ein
create trigger tr_SurveyUpd
on Survey
after update as
begin
    if(update(TypeId))
    begin
        declare @svyId int, @typeId int;
        declare c cursor local for select SvyId, TypeId from inserted;

        open c;
        fetch c into @svyId, @typeId;

        delete from AnswerOption where SvyId = @svyId;

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
end
go


-- Löscht ggf. Umfragekategorien falls diese als deaktiviert gekennzeichnet wurden und nicht mehr verwendet werden
create trigger tr_SurveyDel
on Survey
after delete as
begin
	declare @catId int;
	declare c cursor local for select distinct d.CatId
									  from deleted d
									  join SurveyCategory c on d.CatId = c.CatId
									  where Deactivated = 1;

	open c;
	fetch c into @catId;
	while(@@FETCH_STATUS = 0)
	begin
		if (not exists(select * from Survey where CatId = @catId))
		begin
			delete from SurveyCategory where CatId = @catId;
		end
		fetch c into @catId;
	end
	close c;
	deallocate c;
end
go


-- Zum Überprüfen, ob je Vote nur für AnswerOptions einer einzigen Survey gestimmt werden
-- sowie zum Aufbuchen der Per-Click-Kosten des Kunden
create trigger tr_ChoosesIns
on Chooses
after insert as
begin
	declare @voteId int, @svyId int, @err bit = 0;
	declare c cursor local for select VoteId, SvyId 
							   from inserted i
							   join AnswerOption a on i.AnsId = a.AnsId;

	open c;
	fetch c into @voteId, @svyId;

	while(@@FETCH_STATUS = 0)
	begin
		if (exists(select * from Chooses c join AnswerOption a on c.AnsId = a.AnsId where VoteId = @voteId and SvyId <> @svyId))
		begin
			raiserror('A Vote cannot include AnswerOptions of multiple Surveys! VoteId: %d, SvyId: %d', 16, 1, @voteId, @svyId);
			set @err = 1;
		end
		fetch c into @voteId, @svyId;
	end

	close c;
	deallocate c;
	
	if(@err = 1)
		rollback;
	else
	begin
		declare @custCode char(6), @pricePerClick money;
		declare c2 cursor local for select s.CustCode, PricePerClick
									from inserted i
									join AnswerOption a on i.AnsId = a.AnsId
									join Survey s on a.SvyId = s.SvyId
									where i.VoteId not in (select VoteId from (select * from Chooses
																			   except
																			   select * from inserted) Chooses_before); -- nur für neue VoteIds

	
		open c2;
		fetch c2 into @custCode, @pricePerClick;

		while(@@FETCH_STATUS = 0)
		begin
			update Customer
			set CostBalance += @pricePerClick
			where CustCode = @custCode;

			fetch c2 into @custCode, @pricePerClick;
		end

		close c2;
		deallocate c2;
	end
end
go

-- Zum Löschen einer bestimmten Umfrage und allen zugehörigen Daten
drop procedure sp_DeleteSurvey;
go
create procedure sp_DeleteSurvey(@svyId int)
as
begin
	delete from Vote where VoteId in (select VoteId
                                      from Chooses c 
                                      join AnswerOption a on c.AnsId = a.AnsId
                                      where a.SvyId = @svyId);
    delete from AnswerOption where SvyId = @svyId;
	delete from Asking where SvyId = @svyId;
	delete from Survey where SvyId = @svyId;
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
							   where iif(datepart(day, s.EndDate) > datepart(day, getdate()),
										 datediff(month, s.EndDate, getdate()) - 1,
										 datediff(month, s.EndDate, getdate())
										) >= c.DataStoragePeriod
							   and Template = 0
							   and [Sent] = 1;
	
	open c;
	fetch c into @svyId;
	
	set @svyCount = 0;

	while(@@FETCH_STATUS = 0)
	begin
		exec sp_DeleteSurvey @svyId;
		
		set @svyCount += 1;
		fetch c into @svyId;
	end
	close c;
	deallocate c;

	select @svyCount as 'Deleted surveys'
end
go


-- Zum Erstellen von Rechnungen aller Kunden
drop procedure sp_CreateBills;
go
create procedure sp_CreateBills
as
begin
	declare @custCode char(6), @costBalance money;
	declare @table table (BillId int);
	declare c cursor local for select c.CustCode, CostBalance
							   from Customer c
							   where iif(datepart(day, (select coalesce(max(BillDate), c.RegistrationDate) from Bill b where b.CustCode = c.CustCode)) > datepart(day, getdate()),
										 datediff(month, (select coalesce(max(BillDate), c.RegistrationDate) from Bill b where b.CustCode = c.CustCode), getdate()) - 1,
										 datediff(month, (select coalesce(max(BillDate), c.RegistrationDate) from Bill b where b.CustCode = c.CustCode), getdate())
										) >= c.AccountingPeriod
							   and CostBalance > 0
							   for update;

	open c;
	fetch c into @custCode, @costBalance;

	while(@@FETCH_STATUS = 0)
	begin
		insert into Bill values (@custCode, @costBalance, getdate(), 0, 0);
		insert into @table values (IDENT_CURRENT('Bill'));

		update Customer
		set CostBalance = 0
		where current of c;

		fetch c into @custCode, @costBalance;
	end

	close c;
	deallocate c;

	select BillId from @table;
end
go


-- Zum Berechnen der Per-Click-Kosten bei einer bestimmten Befragtenanzahl
drop function fn_CalcPricePerClick;
go
create function fn_CalcPricePerClick(@amount int)
returns money
as
begin
	declare @value money;

	if (@amount between 0 and 20)			-- f(x) = 0.125 | 0 <= x <= 20
		set @value = 0.125;
	else if (@amount between 21 and 200)	-- f(x) = -0.000135789x + 0.12631578 | 21 <= x <= 200
		set @value = -0.000135789 * @amount + 0.12631578;
	else if (@amount between 201 and 5000)	-- f(x) = 0.107*e^(-0.000335*x) | 201 <= x <= 5000
		set @value = 0.107 * exp(-0.000335 * @amount);
	else if (@amount > 5000)				-- f(x) = 0.02 | x > 5000
		set @value = 0.02;
	else									-- f(x) = -1 | x < 0
		set @value = -1;

	return @value;
end
go


-- Zum hashen eines Strings
drop function fn_GetHash;
go
create function fn_GetHash(@str varchar(max))
returns varbinary(max)
as
begin
	return hashbytes('SHA2_512', @str);
end
go


-- Fixe inserts (für alle gleich!)
begin transaction;
insert into DataConstraint values ('MIN_GROUP_SIZE', 3); -- Nur Testwert

insert into FaqEntry values ('Gegenfrage', 'Aso na doch ned.'); -- Nur Testwert
insert into FaqEntry values ('Porqué no te callas?', 'No quiero callarme porque tú eres un culo muy grande.'); -- Nur Testwert

insert into PaymentMethod values (1, 'OnAccount');

insert into BaseQuestionType values (1, 'OpenQuestion');
insert into BaseQuestionType values (2, 'DichotomousQuestion');
insert into BaseQuestionType values (3, 'PolytomousQuestion');
insert into BaseQuestionType values (4, 'LikertScaleQuestion')
insert into BaseQuestionType values (5, 'FixedAnswerQuestion');

insert into AnswerType values (1, 'YesNo', 5);
insert into AnswerType values (2, 'YesNoDontKnow', 5);
insert into AnswerType values (3, 'TrafficLight', 5);
insert into AnswerType values (4, 'Open', 1);
insert into AnswerType values (5, 'Dichotomous', 2);
insert into AnswerType values (6, 'PolytomousUnorderedSingle', 3);
insert into AnswerType values (7, 'PolytomousUnorderedMultiple', 3);
insert into AnswerType values (8, 'PolytomousOrderedSingle', 3);
insert into AnswerType values (9, 'PolytomousOrderedMultiple', 3);
insert into AnswerType values (10, 'LikertSkale3', 4);
insert into AnswerType values (11, 'LikertSkale4', 4);
insert into AnswerType values (12, 'LikertSkale5', 4);
insert into AnswerType values (13, 'LikertSkale6', 4);
insert into AnswerType values (14, 'LikertSkale7', 4);
insert into AnswerType values (15, 'LikertSkale8', 4);
insert into AnswerType values (16, 'LikertSkale9', 4);

insert into PredefinedAnswerOption values ('Yes', 1);
insert into PredefinedAnswerOption values ('No', 1);
insert into PredefinedAnswerOption values ('Yes', 2);
insert into PredefinedAnswerOption values ('No', 2);
insert into PredefinedAnswerOption values ('DontKnow', 2);
insert into PredefinedAnswerOption values ('Green', 3);
insert into PredefinedAnswerOption values ('Yellow', 3);
insert into PredefinedAnswerOption values ('Red', 3);
insert into PredefinedAnswerOption values ('FreeText', 4);
insert into PredefinedAnswerOption values ('2', 10);
insert into PredefinedAnswerOption values ('2', 11);
insert into PredefinedAnswerOption values ('3', 11);
insert into PredefinedAnswerOption values ('2', 12);
insert into PredefinedAnswerOption values ('3', 12);
insert into PredefinedAnswerOption values ('4', 12);
insert into PredefinedAnswerOption values ('2', 13);
insert into PredefinedAnswerOption values ('3', 13);
insert into PredefinedAnswerOption values ('4', 13);
insert into PredefinedAnswerOption values ('5', 13);
insert into PredefinedAnswerOption values ('2', 14);
insert into PredefinedAnswerOption values ('3', 14);
insert into PredefinedAnswerOption values ('4', 14);
insert into PredefinedAnswerOption values ('5', 14);
insert into PredefinedAnswerOption values ('6', 14);
insert into PredefinedAnswerOption values ('2', 15);
insert into PredefinedAnswerOption values ('3', 15);
insert into PredefinedAnswerOption values ('4', 15);
insert into PredefinedAnswerOption values ('5', 15);
insert into PredefinedAnswerOption values ('6', 15);
insert into PredefinedAnswerOption values ('7', 15);
insert into PredefinedAnswerOption values ('2', 16);
insert into PredefinedAnswerOption values ('3', 16);
insert into PredefinedAnswerOption values ('4', 16);
insert into PredefinedAnswerOption values ('5', 16);
insert into PredefinedAnswerOption values ('6', 16);
insert into PredefinedAnswerOption values ('7', 16);
insert into PredefinedAnswerOption values ('8', 16);
commit;
go