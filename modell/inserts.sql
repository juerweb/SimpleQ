use SimpleQDB;
set dateformat ymd;
set nocount on;
go

insert into DsgvoConstraint values ('MIN_GROUP_SIZE', 3); -- Nur Testwert
go

begin transaction;
insert into PaymentMethod values (1, 'Visa');
insert into Customer values ('m4rku5', 'musterreis gmbh.', 'm@rk.us', 'asdfjklö', null, 'Haasenplatz', '420', 'Guntramsdorf', 'España', 'DE', 6, null, 1, 0);
insert into Department values ('abteilung', 'm4rku5'); -- DepId 1
insert into [Group] values ('m4rku5', 'Testgruppe');
insert into [Contains] values (1, 1, 'm4rku5', 3);
insert into AskedPerson values (1, 'm4rku5'); -- PersId 1
insert into AskedPerson values (1, 'm4rku5'); -- PersId 2
insert into AskedPerson values (1, 'm4rku5'); -- PersId 3
insert into AskedPerson values (1, 'm4rku5'); -- PersId 4
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
insert into SurveyCategory values ('m4rku5', 'Testfrage');
insert into Survey values ('m4rku5', 'Ist N.H. ein Nazi?', '2018-08-24', '2019-08-24', 1, 1) -- SvyId 1
insert into Survey values ('m4rku5', 'Sind Sie ein Pajero?', '2018-08-24', '2019-08-24', 4, 1); -- SvyId 2
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Ye man'); -- SpecId 1
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Schau i so aus?') -- SpecId 2
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo'); -- SpecId 3
commit;
go

select * from Customer;
select * from AskedPerson;