use SimpleQDB;
set dateformat ymd;
set nocount on;
go


begin transaction;
insert into Customer values ('m4rku5', 'musterreis gmbh.', 'm@rk.us', 'asdfjklö', null, 'Haasenplatz', '420', 'Guntramsdorf', 'España', 'DE', 6, 1, 0);
insert into Department values ('development', 'm4rku5'); -- DepId 1
insert into Department values ('putzkleschn', 'm4rku5'); -- DepId 2
insert into Person values (1, null); -- PersId 1
insert into Person values (1, null); -- PersId 2
insert into Person values (1, null); -- PersId 3
insert into Person values (2, null); -- PersId 4
insert into Person values (2, null); -- PersId 5

insert into SurveyCategory values ('m4rku5', 'Politische Fragen'); -- CatId 1
insert into SurveyCategory values ('m4rku5', 'Persönliche Fragen'); -- CatId 2
insert into SurveyCategory values ('m4rku5', 'Unnötige Fragen'); -- CatId 3

insert into Survey values (1, 'm4rku5', 'Ist N.H. ein Nazi?', '2018-08-24', '2019-08-24', 1) -- SvyId 1
insert into Survey values (2, 'm4rku5', 'Sind sie foisch?', '2018-08-24', '2019-08-24', 1) -- SvyId 2
insert into Survey values (2, 'm4rku5', 'Sind Sie ein Pajero?', '2018-08-24', '2019-08-24', 2); -- SvyId 3
insert into Survey values (3, 'm4rku5', 'Was halten Sie von Nico Srnka?', '2018-08-24', '2019-08-24', 7) -- SvyId 4

insert into Asking values (1, 1);
insert into Asking values (1, 2);
insert into Asking values (2, 1);
insert into Asking values (3, 1);
insert into Asking values (3, 2);
insert into Asking values (4, 2);

-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 1 nötig (YesNo) => AnsIds 1, 2
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 2 nötig (YesNo) => AnsIds 3, 4
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 3 nötig (YesNoDontKnow) => AnsIds 5, 6, 7
insert into AnswerOption values (4, 'Nix'); -- AnsId 8
insert into AnswerOption values (4, 'Foa in oasch');  -- AnsId 9
insert into AnswerOption values (4, 'Ja, aber in grün'); -- AnsId 10
insert into AnswerOption values (4, 'Besa mi culo');  -- AnsId 11
insert into AnswerOption values (4, 'Vete a la mierda');  -- AnsId 12
insert into AnswerOption values (4, 'Hijo de puta');  -- AnsId 13


-- +++ Beantwortungen über Handy +++
insert into Vote values (null); -- VoteId 1
insert into Chooses values (1, 1);

insert into Vote values (null); -- VoteId 2
insert into Chooses values (2, 1);

insert into Vote values (null); -- VoteId 3
insert into Chooses values (3, 1);

insert into Vote values (null); -- VoteId 4
insert into Chooses values (4, 1);

insert into Vote values (null); -- VoteId 5
insert into Chooses values (5, 3);

insert into Vote values (null); -- VoteId 6
insert into Chooses values (6, 4);

insert into Vote values (null); -- VoteId 7
insert into Chooses values (7, 5);

insert into Vote values (null); -- VoteId 8
insert into Chooses values (8, 5);

insert into Vote values (null); -- VoteId 9
insert into Chooses values (9, 6);

insert into Vote values (null); -- VoteId 10
insert into Chooses values (10, 7);

insert into Vote values (null); -- VoteId 11
insert into Chooses values (11, 8);

insert into Vote values (null); -- VoteId 12
insert into Chooses values (12, 10);

insert into Vote values (null); -- VoteId 13
insert into Chooses values (13, 11);

insert into Vote values (null); -- VoteId 14
insert into Chooses values (14, 11);

insert into Vote values (null); -- VoteId 15
insert into Chooses values (15, 12);

insert into Vote values (null); -- VoteId 16
insert into Chooses values (16, 12);

insert into Vote values (null); -- VoteId 17
insert into Chooses values (17, 13);
commit;
go

select * from Person;
select * from Customer;
select * from AnswerOption;

-- Test stored procedure
--begin transaction;
--update Survey set StartDate = '2018-03-01' where SvyId = 2
--exec sp_CheckExceededSurveyData;
--select * from Survey;
--select * from Asking;
--select * from AnswerOption;
--select * from Chooses;
--select * from Vote;
--rollback;