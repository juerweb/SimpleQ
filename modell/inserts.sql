use SimpleQDB;
set dateformat ymd;
set nocount on;
go


begin transaction;
insert into Customer values ('m4rku5', 'musterreis gmbh.', 'm@rk.us', 'asdfjklö', null, 'Haasenplatz', '420', 'Guntramsdorf', 'España', 'DE', 6, 1, 0);
insert into Department values ('abteilung', 'm4rku5'); -- DepId 1
insert into Department values ('abteilung2', 'm4rku5'); -- DepId 2
insert into [Group] values ('m4rku5', 'Testgruppe'); -- GroupId 1
insert into [Contains] values (1, 1, 'm4rku5', 3);
insert into AskedPerson values (1, 'm4rku5'); -- PersId 1
insert into AskedPerson values (1, 'm4rku5'); -- PersId 2
insert into AskedPerson values (1, 'm4rku5'); -- PersId 3
insert into AskedPerson values (1, 'm4rku5'); -- PersId 4
insert into SurveyCategory values ('m4rku5', 'Testfrage');
insert into Survey values ('m4rku5', 'Ist N.H. ein Nazi?', '2018-08-24', '2019-08-24', 1, 1) -- SvyId 1
insert into Survey values ('m4rku5', 'Sind Sie ein Pajero?', '2018-08-24', '2019-08-24', 4, 1); -- SvyId 2
insert into Asking values (1, 1, 'm4rku5');
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Ye man'); -- SpecId 1
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Schau i so aus?') -- SpecId 2
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo'); -- SpecId 3
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo4'); -- SpecId 4
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo5'); -- SpecId 5
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo6'); -- SpecId 6
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo7'); -- SpecId 7
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo8'); -- SpecId 8
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo9'); -- SpecId 9
insert into SpecifiedTextAnswer values (2, 'm4rku5', 'Besa mi culo10'); -- SpecId 10
commit;
go

select * from AskedPerson;

select * from Customer;
select v.VoteId, s.SvyId, s.SvyText as 'Frage', t.TypeDesc as 'Fragetyp', a.AnsDesc as 'Antwort', v.VoteText as 'Ein-Wort-Antwort', sp.SpecText as 'Vorgegebene Antwort', s.StartDate as 'Datum'
from Vote v 
join Answer a on v.AnsId = a.AnsId
join Survey s on v.SvyId = s.SvyId 
join AnswerType t on a.TypeId = t.TypeId
left join SpecifiedTextAnswer sp on v.SpecId = sp.SpecId;


-- update Survey set StartDate = '2018-03-01' where SvyId = 1
-- exec sp_CheckExceededSurveyData;