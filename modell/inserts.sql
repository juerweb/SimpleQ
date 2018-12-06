use SimpleQDB;
set dateformat ymd;
set nocount on;
go


begin transaction;
insert into Customer values ('420420', 'inge gmbh.', 'g@b.i', 'asdfjklö', null, null, 1, 'Haasenplatz', '420', 'Guntramsdorf', 'España', 'DE', 12, 3, (select getdate()), 1, 5, 0, null, null);
insert into Department values (1, 'development', '420420');
insert into Department values (2, 'putzkleschn', '420420');
insert into Person values (null); -- PersId 1
insert into Person values (null); -- PersId 2
insert into Person values (null); -- PersId 3
insert into Person values (null); -- PersId 4
insert into Person values (null); -- PersId 5
insert into Employs values (1, '420420', 1);
insert into Employs values (1, '420420', 2);
insert into Employs values (1, '420420', 3);
insert into Employs values (2, '420420', 3);
insert into Employs values (2, '420420', 4);
insert into Employs values (2, '420420', 5);

insert into SurveyCategory values (1, '420420', 'Politische Fragen', 0);
insert into SurveyCategory values (2, '420420', 'Persönliche Fragen', 0);
insert into SurveyCategory values (3, '420420', 'Unnötige Fragen', 0);

insert into Survey values (1, '420420', 'Sind Sie politisch interessiert?', '2018-08-24', '2019-08-24', 5, null, 1, 0, 0) -- SvyId 1
insert into Survey values (2, '420420', 'Sind Sie foisch?', '2018-08-24', '2019-08-24', 5, null, 1, 0, 0) -- SvyId 2
insert into Survey values (2, '420420', 'Le gusta café?', '2018-08-24', '2019-08-24', 5, null, 2, 0, 0); -- SvyId 3
insert into Survey values (3, '420420', 'Was halten Sie von Nico Srnka?', '2018-08-24', '2019-08-24', 5, null, 7, 0, 0) -- SvyId 4
insert into Survey values (3, '420420', 'Beschreiben Sie Nico Srnka in einem Wort', '2018-08-24', '2019-08-24', 5, null, 4, 0, 0) -- SvyId 5

insert into Asking values (1, 1, '420420');
insert into Asking values (1, 2, '420420');
insert into Asking values (2, 1, '420420');
insert into Asking values (3, 1, '420420');
insert into Asking values (3, 2, '420420');
insert into Asking values (4, 2, '420420');
insert into Asking values (5, 1, '420420');

-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 1 nötig (YesNo) => AnsIds 1, 2
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 2 nötig (YesNo) => AnsIds 3, 4
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 3 nötig (YesNoDontKnow) => AnsIds 5, 6, 7
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 5 nötig (Open) => AnsId 8
insert into AnswerOption values (4, 'Nix', null); -- AnsId 9
insert into AnswerOption values (4, 'Siebzehn', null);  -- AnsId 10
insert into AnswerOption values (4, 'Ja, aber in gruen', null); -- AnsId 11
insert into AnswerOption values (4, 'No me gusta mucho', null);  -- AnsId 12
insert into AnswerOption values (4, 'No, pero en verde', null);  -- AnsId 13
insert into AnswerOption values (4, 'Es el mejor', null);  -- AnsId 14

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
insert into Chooses values (11, 9);
insert into Chooses values (11, 11);

insert into Vote values (null); -- VoteId 12
insert into Chooses values (12, 11);
insert into Chooses values (12, 12);

insert into Vote values (null); -- VoteId 13
insert into Chooses values (13, 12);

insert into Vote values (null); -- VoteId 14
insert into Chooses values (14, 12);

insert into Vote values (null); -- VoteId 15
insert into Chooses values (15, 13);

insert into Vote values (null); -- VoteId 16
insert into Chooses values (16, 13);

insert into Vote values (null); -- VoteId 17
insert into Chooses values (17, 14);

insert into Vote values ('Foisch'); -- VoteId 18
insert into Chooses values (18, 8);

insert into Vote values ('Abgehoben'); -- VoteId 19
insert into Chooses values (19, 8);

insert into Vote values ('Topf3'); -- VoteId 20
insert into Chooses values (20, 8);
commit;
go

-- ++++++++ TRENDANALYSE-TESTDATEN ++++++++
insert into SurveyCategory values (4, '420420', 'Ist der Chef unbeliebt?', 0);

insert into Survey values (4, '420420', 'Finden Sie der Chef ist ein Arschloch?', '2018-07-01', '2018-07-15', 5, null, 2, 0, 0); -- SvyId 6
insert into Asking values (6, 1, '420420');
insert into Asking values (6, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 6 nötig (YesNoDontKnow) => AnsIds 15, 16, 17
insert into Vote values (null) -- VoteId 21;
insert into Chooses values (21, 15);
insert into Vote values (null) -- VoteId 22;
insert into Chooses values (22, 15);
insert into Vote values (null) -- VoteId 23;
insert into Chooses values (23, 16);
insert into Vote values (null) -- VoteId 24;
insert into Chooses values (24, 17);

insert into Survey values (4, '420420', 'Finden Sie jetzt der Chef ist ein Arschloch?', '2018-07-16', '2018-07-31', 5, null, 2, 0, 0); -- SvyId 7
insert into Asking values (7, 1, '420420');
insert into Asking values (7, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 7 nötig (YesNoDontKnow) => AnsIds 18, 19, 20
insert into Vote values (null) -- VoteId 25;
insert into Chooses values (25, 18);
insert into Vote values (null) -- VoteId 26;
insert into Chooses values (26, 19);
insert into Vote values (null) -- VoteId 27;
insert into Chooses values (27, 19);
insert into Vote values (null) -- VoteId 28;
insert into Chooses values (28, 20);

insert into Survey values (4, '420420', 'Und wie schauts jetzt aus mit Chef=Arschloch?', '2018-08-01', '2018-08-15', 5, null, 2, 0, 0); -- SvyId 8
insert into Asking values (8, 1, '420420');
insert into Asking values (8, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 8 nötig (YesNoDontKnow) => AnsIds 21, 22, 23
insert into Vote values (null) -- VoteId 29;
insert into Chooses values (29, 21);
insert into Vote values (null) -- VoteId 30;
insert into Chooses values (30, 21);
insert into Vote values (null) -- VoteId 31;
insert into Chooses values (31, 21);
insert into Vote values (null) -- VoteId 32;
insert into Chooses values (32, 21);
insert into Vote values (null) -- VoteId 33;
insert into Chooses values (33, 23);
insert into Vote values (null) -- VoteId 34;
insert into Chooses values (34, 23);

insert into Survey values (4, '420420', 'Es jefe igual a culo?', '2018-08-16', '2018-08-31', 5, null, 2, 0, 0); -- SvyId 9
insert into Asking values (9, 1, '420420');
insert into Asking values (9, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 9 nötig (YesNoDontKnow) => AnsIds 24, 25, 26
insert into Vote values (null) -- VoteId 35;
insert into Chooses values (35, 24);
insert into Vote values (null) -- VoteId 36;
insert into Chooses values (36, 24);
insert into Vote values (null) -- VoteId 37;
insert into Chooses values (37, 25);
insert into Vote values (null) -- VoteId 38;
insert into Chooses values (38, 25);
insert into Vote values (null) -- VoteId 39;
insert into Chooses values (39, 25);
insert into Vote values (null) -- VoteId 40;
insert into Chooses values (40, 26);

insert into Survey values (4, '420420', 'Jefe=culo?', '2018-09-01', '2018-09-15', 5, null, 2, 0, 0); -- SvyId 10
insert into Asking values (10, 1, '420420');
insert into Asking values (10, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 10 nötig (YesNoDontKnow) => AnsIds 27, 28, 29
insert into Vote values (null) -- VoteId 41;
insert into Chooses values (41, 27);
insert into Vote values (null) -- VoteId 42;
insert into Chooses values (42, 27);
insert into Vote values (null) -- VoteId 43;
insert into Chooses values (43, 27);
insert into Vote values (null) -- VoteId 44;
insert into Chooses values (44, 27);
insert into Vote values (null) -- VoteId 45;
insert into Chooses values (45, 28);
insert into Vote values (null) -- VoteId 46;
insert into Chooses values (46, 29);

update Survey set [Sent] = 1;

select * from Person;
select * from Employs order by persId;
select * from Customer;
select * from AnswerOption;
select * from Survey;

-- Test survey creation
--delete from Employs where persId = 3 and depid = 2;
--insert into person values(null);
--insert into employs values (2, '420420', 6);
--insert into employs values (2, '420420', 1)
--insert into employs values (2, '420420', 2);

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
