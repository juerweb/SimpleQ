use SimpleQDB;
go

set nocount on;
go

drop table DsgvoConstraint;
drop table Vote;
drop table Answer;
drop table Survey;
drop table SurveyType;
drop table AskedPerson;
drop table [Contains];
drop table [Group];
drop table Department;
drop table Bill;
drop table Customer;
drop table PaymentMethod;

create table PaymentMethod
(
	PaymentMethodId int primary key,
	PaymentMethodDesc varchar(max) not null
);
go

create table Customer
(
	CustName varchar(256) primary key,
	CustEmail varchar(256) unique not null,
	CustPwdTmp varchar(max),
	CustPwdHash varbinary(max),
	Street varchar(256) not null,
	Plz varchar(16) not null,
	City varchar(256) not null,
	Country varchar(256) not null,
	PaymentMethodId int not null references PaymentMethod,
	CostBalance money not null
);
go

create table Bill
(
	BillId int identity,
	CustName varchar(256) references Customer,
	BillAmount money not null,
	BillDate datetime not null,
	Paid bit not null,
	primary key (BillId, CustName)
);
go

create table Department
(
	DepName varchar(256),
	CustName varchar(256) references Customer,
	primary key (DepName, CustName)
);
go

create table [Group] 
(
	GroupId int identity,
	CustName varchar(256) references Customer,
	GroupDesc varchar(max),
	primary key (GroupId, CustName)
);
go

create table [Contains]
(
	GroupId int,
	DepName varchar(256),
	CustName varchar(256), 
	Amount int not null,
	primary key (GroupId, DepName, CustName),
	foreign key (DepName, CustName) references Department,
	foreign key (GroupId, CustName) references [Group]
);
go

create table AskedPerson
(
	PersEmail varchar(256),
	DepName varchar(256),
	CustName varchar(256),
	PersFirstName varchar(256) not null,
	PersLastName varchar(256) not null,
	PersPwdTmp varchar(max),
	PersPwdHash varbinary(max),
	primary key (PersEmail, CustName),
	foreign key (DepName, CustName) references Department
);
go

create table SurveyType
(
	TypeId int primary key,
	TypeDesc varchar(256) not null
);
go

create table Survey
(
	SvyId int identity,
	CustName varchar(256) references Customer,
	SvyDesc varchar(max) not null,
	StartDate datetime not null,
	EndDate datetime not null,
	TypeId int not null references SurveyType,
	primary key (SvyId, CustName)
);
go

create table Answer
(
	AnsId int identity primary key,
	AnsDesc varchar(256) not null,
);
go

create table Vote
(
	VoteId int identity primary key,
	SvyId int,
	CustName varchar(256),
	AnsId int references Answer,
	Note varchar(max) null,
	foreign key (SvyId, CustName) references Survey
);
go

create table DsgvoConstraint
(
	ConstrName varchar(256) primary key,
	ConstrValue int not null,
);
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
	declare @custName varchar(256), @hash varbinary(max);
	declare c cursor local for select CustName, hashbytes('SHA2_512', CustPwdTmp) from inserted;
	
	open c;
	fetch c into @custName, @hash;

	while(@@FETCH_STATUS = 0)
	begin
		if(@hash is not null)
		begin
			update Customer set CustPwdHash = @hash, CustPwdTmp = null
			where CustName = @custName
		end
		
		fetch c into @custName, @hash
	end

	close c;
	deallocate c;
end
go

create trigger tr_AskedPersonInsUpd
on AskedPerson
after insert, update as
begin
	declare @persEmail varchar(256), @hash varbinary(max);
	declare c cursor local for select PersEmail, hashbytes('SHA2_512', PersPwdTmp) from inserted;
	
	open c;
	fetch c into @persEmail, @hash;

	while(@@FETCH_STATUS = 0)
	begin
		if(@hash is not null)
		begin
			update AskedPerson set PersPwdHash = @hash, PersPwdTmp = null
			where PersEmail = @persEmail
		end

		fetch c into @persEmail, @hash
	end

	close c;
	deallocate c;
end
go



--insert into DsgvoConstraint select 'MIN_GROUP_SIZE', 20;

--insert into PaymentMethod values (1, 'Bank payment');
--insert into Customer values (1, 'Mbappé Inc.', 'mb@p.pe', 'asdf1234', null, 'street', '1010', 'city', 'country', 1, 0);
--insert into Department values (1, 1, 'dep1');
--insert into Department values (2, 1, 'dep2');
--insert into Department values (3, 1, 'dep3');
--insert into [Group] values (1, 'Group 1');
--insert into [Group] values (2, 'Group 2');
--insert into [Contains] values (1, 1, 7);
--insert into [Contains] values (1, 2, 13);
--insert into [Contains] values (2, 3, 2);
--insert into [Contains] values (2, 2, 8);

--select * from vw_InvalidGroupSizes;
--select * from customer;
--select * from customer where CustPwdHash = hashbytes('SHA2_512', 'asdf1234');