use SimpleQDB;
go

set nocount on;
go

drop table DsgvoConstraint;
drop table Vote;
drop table Answer;
drop table Survey;
drop table SurveyCategory;
drop table SurveyType;
drop table AskedPerson;
drop table [Contains];
drop table [Group];
drop table Department;
drop table Bill;
drop table Customer;
drop table PaymentMethod;

-- Nicht kundenabhängig
create table PaymentMethod
(
	PaymentMethodId int primary key,
	PaymentMethodDesc varchar(max) not null
);
go


create table Customer
(
	CustCode char(6) primary key,
	CustName varchar(max) not null,
	CustEmail varchar(max) not null,
	CustPwdTmp varchar(max),
	CustPwdHash varbinary(max),
	Street varchar(max) not null,
	Plz varchar(16) not null,
	City varchar(max) not null,
	Country varchar(max) not null,
	PaymentMethodId int not null references PaymentMethod,
	CostBalance money not null
);
go

-- Kundenabhängig
create table Bill
(
	BillId int identity,
	CustCode char(6) references Customer,
	BillAmount money not null,
	BillDate datetime not null,
	Paid bit not null,
	primary key (BillId, CustCode)
);
go

-- Kundenabhängig
create table Department
(
	DepName varchar(512),
	CustCode char(6) references Customer,
	primary key (DepName, CustCode)
);
go

-- Kundenabhängig
create table [Group] 
(
	GroupId int identity,
	CustCode char(6) references Customer,
	GroupDesc varchar(max) not null,
	primary key (GroupId, CustCode)
);
go

-- Kundenabhängig
create table [Contains]
(
	GroupId int,
	DepName varchar(512),
	CustCode char(6), 
	Amount int not null,
	primary key (GroupId, DepName, CustCode),
	foreign key (DepName, CustCode) references Department,
	foreign key (GroupId, CustCode) references [Group]
);
go

-- Kundenabhängig
create table AskedPerson
(
	PersId int identity,
	DepName varchar(512) not null,
	CustCode char(6),
	primary key (PersId, CustCode),
	foreign key (DepName, CustCode) references Department
);
go

-- Nicht kundenabhängig
create table SurveyType
(
	TypeId int primary key,
	TypeDesc varchar(max) not null
);
go

-- Kundenabhängig
create table SurveyCategory
(
	CatId int identity,
	CustCode char(6) references Customer, 
	CatName varchar(max) not null,
	primary key (CatId, CustCode)
);
go

-- Kundenabhängig
create table Survey
(
	SvyId int identity,
	CustCode char(6) references Customer,
	SvyDesc varchar(max) not null,
	StartDate datetime not null,
	EndDate datetime not null,
	TypeId int not null references SurveyType,
	CatId int not null
	primary key (SvyId, CustCode),
	foreign key (CatId, CustCode) references SurveyCategory
);
go

-- Nicht kundenabhängig
create table Answer
(
	AnsId int primary key,
	AnsDesc varchar(max) not null,
);
go

-- Kundenabhängig
create table Vote
(
	VoteId int identity primary key,
	SvyId int not null,
	CustCode char(6),
	AnsId int not null references Answer,
	Note varchar(max) null,
	foreign key (SvyId, CustCode) references Survey
);
go

-- Nicht kundenabhängig
create table DsgvoConstraint
(
	ConstrName varchar(512) primary key,
	ConstrValue int not null,
);
go

insert into DsgvoConstraint values ('MIN_GROUP_SIZE', 3); -- Nur Testwert
go


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
go

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