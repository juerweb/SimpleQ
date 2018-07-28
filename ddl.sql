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
	CustId int primary key,
	CustName varchar(1000) unique not null,
	CustEmail varchar(1000) unique not null,
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

create table Bill
(
	BillId int identity,
	CustId int references Customer,
	BillAmount money not null,
	BillDate datetime not null,
	Payed bit not null
	primary key (BillId, CustId)
);
go

create table Department
(
	DepId int primary key,
	CustId int not null references Customer,
	DepName varchar(max) not null
);
go

create table [Group] 
(
	GroupId int primary key,
	GroupDesc varchar(max)
);
go

create table [Contains]
(
	GroupId int references [Group],
	DepId int references Department,
	Amount int not null,
	primary key (GroupId, DepId)
);
go

create table AskedPerson
(
	PersId int identity,
	DepId int references Department,
	PersFirstName varchar(max) not null,
	PersLastName varchar(max) not null,
	PersEmail varchar(max) not null,
	PersPwdTmp varchar(max),
	PersPwdHash varbinary(max),
	primary key (PersId, DepId)
);
go

create table SurveyType
(
	TypeId int primary key,
	TypeDesc varchar(max) not null
);
go

create table Survey
(
	SvyId int identity (100, 10),
	CustId int references Customer,
	SvyDesc varchar(max) not null,
	StartDate datetime not null,
	EndDate datetime not null,
	TypeId int not null references SurveyType,
	primary key (SvyId, CustId)
);
go

create table Answer
(
	AnsId int identity primary key,
	AnsDesc varchar(max) not null
);
go

create table Vote
(
	SvyId int,
	CustId int,
	AnsId int references Answer,
	primary key (SvyId, CustId, AnsId),
	foreign key (SvyId, CustId) references Survey
);
go

create table DsgvoConstraint
(
	ConstrName varchar(100) primary key,
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
	declare @custId int, @hash varbinary(max);
	declare c cursor local for select CustId, hashbytes('SHA2_512', CustPwdTmp) from inserted;
	
	open c;
	fetch c into @custId, @hash;

	while(@@FETCH_STATUS = 0)
	begin
		if(@hash is not null)
		begin
			update Customer set CustPwdHash = @hash, CustPwdTmp = null
			where CustId = @custId
		end
		
		fetch c into @custId, @hash
	end

	close c;
	deallocate c;
end
go

create trigger tr_AskedPersonInsUpd
on AskedPerson
after insert, update as
begin
	declare @persId int, @hash varbinary(max);
	declare c cursor local for select PersId, hashbytes('SHA2_512', PersPwdTmp) from inserted;
	
	open c;
	fetch c into @persId, @hash;

	while(@@FETCH_STATUS = 0)
	begin
		if(@hash is not null)
		begin
			update AskedPerson set PersPwdHash = @hash, PersPwdTmp = null
			where PersID = @persId
		end

		fetch c into @persId, @hash
	end

	close c;
	deallocate c;
end
go



insert into DsgvoConstraint select 'MIN_GROUP_SIZE', 20;

insert into PaymentMethod values (1, 'Bank payment');
insert into Customer values (1, 'Mbappé Inc.', 'mb@p.pe', 'asdf1234', null, 'street', '1010', 'city', 'country', 1, 0);
insert into Department values (1, 1, 'dep1');
insert into Department values (2, 1, 'dep2');
insert into Department values (3, 1, 'dep3');
insert into [Group] values (1, 'Group 1');
insert into [Group] values (2, 'Group 2');
insert into [Contains] values (1, 1, 7);
insert into [Contains] values (1, 2, 13);
insert into [Contains] values (2, 3, 2);
insert into [Contains] values (2, 2, 8);

select * from vw_InvalidGroupSizes;
select * from customer;
select * from customer where CustPwdHash = hashbytes('SHA2_512', 'asdf1234');