use SimpleQDB;
go;

drop procedure sp_createPersons;
go
create procedure sp_createPersons(@count int)
as
begin
	declare @persId integer;
	set @persId = (select max(PersId) from Person);
	while (@count > 0)
	begin
		set @persId += 1;
		insert into Person values (null);
		insert into Employs values (1, '420420', @persId);
		set @count -=1;
	end
end
go

exec sp_createPersons 10000;
