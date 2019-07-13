use SimpleQDB;
set dateformat ymd;
set nocount on;
go

begin transaction;
insert into Customer values ('420420', 'ACME Inc.', 'acme@domain.com', 'asdfjklö', null, null, 1, 'Kärntner Straße 1', '1010', 'Wien', 'Österreich', 24, 3, (select getdate()), 1, 5, 0, null, null, 0);

--insert into Bill values('420420', 420.00, (select dateadd(month, -9, (select getdate()))), 1, 1);
insert into Bill values('420420', 0.50, (select dateadd(month, -6, convert(date,'2019-01-26'))), 1, 1);
insert into Bill values('420420', 5.25, (select dateadd(month, -3, convert(date,'2019-01-26'))), 1, 1);

insert into Department values (1, 'Development', '420420');
insert into Department values (2, 'Marketing', '420420');

insert into Person values (null, null); -- PersId 1
insert into Person values (null, null); -- PersId 2
insert into Person values (null, null); -- PersId 3
insert into Person values (null, null); -- PersId 4
insert into Person values (null, null); -- PersId 5

insert into Employs values (1, '420420', 1);
insert into Employs values (1, '420420', 2);
insert into Employs values (1, '420420', 3);
insert into Employs values (2, '420420', 3);
insert into Employs values (2, '420420', 4);
insert into Employs values (2, '420420', 5);

insert into SurveyCategory values (1, '420420', 'Mitarbeiterzufriedenheit', 0);
insert into SurveyCategory values (2, '420420', 'Arbeitsplatzgestaltung', 0);
insert into SurveyCategory values (3, '420420', 'Entscheidungsqualität', 0);

insert into Survey values (1, '420420', 'Fühlen Sie sich i.A. im Unternehmen wohl?', '2018-08-24', '2019-08-24', 5, null, 1, 0, 1, null) -- SvyId 1
insert into Survey values (2, '420420', 'Gefallen Ihnen die neuen Bürostühle?', '2018-08-24', '2019-08-24', 5, null, 1, 0, 1, null) -- SvyId 2
insert into Survey values (2, '420420', 'Sind die Arbeitsräumlichkeiten Ihrer Ansicht nach hell genug?', '2018-08-24', '2019-08-24', 5, null, 2, 0, 1, null); -- SvyId 3
insert into Survey values (2, '420420', 'Welche der folgenden Büroeinrichtungen halten Sie für sinnvoll?', '2018-08-24', '2019-08-24', 5, null, 7, 0, 1, null) -- SvyId 4
insert into Survey values (3, '420420', 'Beschreiben Sie die kürzlich erfolgten Richtungsentscheidungen in einem Wort', '2018-08-24', '2019-08-24', 5, null, 4, 0, 1, null) -- SvyId 5

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
insert into AnswerOption values (4, 'Neue Farblaserdrucker mit WLAN-Unterstützung', null); -- AnsId 9
insert into AnswerOption values (4, 'Hellere Lichtquellen mit Tageslichtqualität', null);  -- AnsId 10
insert into AnswerOption values (4, 'Wasserspender für jeden Büroraum', null); -- AnsId 11
insert into AnswerOption values (4, 'Monitore mit Curved Displays', null);  -- AnsId 12
insert into AnswerOption values (4, 'Espressomaschinen für jeden Büroraum', null);  -- AnsId 13
insert into AnswerOption values (4, 'Neue Computer mit aktuellem Technologiestand', null);  -- AnsId 14

-- +++ Beantwortungen über Handy +++
insert into Vote values (null, null); -- VoteId 1
insert into Chooses values (1, 1);

insert into Vote values (null, null); -- VoteId 2
insert into Chooses values (2, 1);

insert into Vote values (null, null); -- VoteId 3
insert into Chooses values (3, 1);

insert into Vote values (null, null); -- VoteId 4
insert into Chooses values (4, 1);

insert into Vote values (null, null); -- VoteId 5
insert into Chooses values (5, 3);

insert into Vote values (null, null); -- VoteId 6
insert into Chooses values (6, 4);

insert into Vote values (null, null); -- VoteId 7
insert into Chooses values (7, 5);

insert into Vote values (null, null); -- VoteId 8
insert into Chooses values (8, 5);

insert into Vote values (null, null); -- VoteId 9
insert into Chooses values (9, 6);

insert into Vote values (null, null); -- VoteId 10
insert into Chooses values (10, 7);

insert into Vote values (null, null); -- VoteId 11
insert into Chooses values (11, 9);
insert into Chooses values (11, 11);

insert into Vote values (null, null); -- VoteId 12
insert into Chooses values (12, 11);
insert into Chooses values (12, 12);

insert into Vote values (null, null); -- VoteId 13
insert into Chooses values (13, 12);

insert into Vote values (null, null); -- VoteId 14
insert into Chooses values (14, 12);

insert into Vote values (null, null); -- VoteId 15
insert into Chooses values (15, 13);

insert into Vote values (null, null); -- VoteId 16
insert into Chooses values (16, 13);

insert into Vote values (null, null); -- VoteId 17
insert into Chooses values (17, 14);

insert into Vote values ('Richtiger Weg', null); -- VoteId 18
insert into Chooses values (18, 8);

insert into Vote values ('Längst überfällige', null); -- VoteId 19
insert into Chooses values (19, 8);

insert into Vote values ('Etwas übereilt', null); -- VoteId 20
insert into Chooses values (20, 8);

-- ++++++++ TRENDANALYSE-TESTDATEN ++++++++
insert into SurveyCategory values (4, '420420', 'Unternehmensleitung', 0);

insert into Survey values (4, '420420', 'Wünschen Sie einen Wechsel in der Führungsposition?', '2018-07-01', '2018-07-15', 5, null, 2, 1, 1, null); -- SvyId 6
insert into Asking values (6, 1, '420420');
insert into Asking values (6, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 6 nötig (YesNoDontKnow) => AnsIds 15, 16, 17
insert into Vote values (null, null) -- VoteId 21;
insert into Chooses values (21, 15);
insert into Vote values (null, null) -- VoteId 22;
insert into Chooses values (22, 15);
insert into Vote values (null, null) -- VoteId 23;
insert into Chooses values (23, 16);
insert into Vote values (null, null) -- VoteId 24;
insert into Chooses values (24, 17);

insert into Survey values (4, '420420', 'Wünschen Sie einen Wechsel in der Führungsposition?', '2018-07-16', '2018-07-31', 5, null, 2, 0, 1, null); -- SvyId 7
insert into Asking values (7, 1, '420420');
insert into Asking values (7, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 7 nötig (YesNoDontKnow) => AnsIds 18, 19, 20
insert into Vote values (null, null) -- VoteId 25;
insert into Chooses values (25, 18);
insert into Vote values (null, null) -- VoteId 26;
insert into Chooses values (26, 19);
insert into Vote values (null, null) -- VoteId 27;
insert into Chooses values (27, 19);
insert into Vote values (null, null) -- VoteId 28;
insert into Chooses values (28, 20);

insert into Survey values (4, '420420', 'Wünschen Sie einen Wechsel in der Führungsposition?', '2018-08-01', '2018-08-15', 5, null, 2, 0, 1, null); -- SvyId 8
insert into Asking values (8, 1, '420420');
insert into Asking values (8, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 8 nötig (YesNoDontKnow) => AnsIds 21, 22, 23
insert into Vote values (null, null) -- VoteId 29;
insert into Chooses values (29, 21);
insert into Vote values (null, null) -- VoteId 30;
insert into Chooses values (30, 21);
insert into Vote values (null, null) -- VoteId 31;
insert into Chooses values (31, 21);
insert into Vote values (null, null) -- VoteId 32;
insert into Chooses values (32, 21);
insert into Vote values (null, null) -- VoteId 33;
insert into Chooses values (33, 23);
insert into Vote values (null, null) -- VoteId 34;
insert into Chooses values (34, 23);

insert into Survey values (4, '420420', 'Wünschen Sie einen Wechsel in der Führungsposition?', '2018-08-16', '2018-08-31', 5, null, 2, 0, 1, null); -- SvyId 9
insert into Asking values (9, 1, '420420');
insert into Asking values (9, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 9 nötig (YesNoDontKnow) => AnsIds 24, 25, 26
insert into Vote values (null, null) -- VoteId 35;
insert into Chooses values (35, 24);
insert into Vote values (null, null) -- VoteId 36;
insert into Chooses values (36, 24);
insert into Vote values (null, null) -- VoteId 37;
insert into Chooses values (37, 25);
insert into Vote values (null, null) -- VoteId 38;
insert into Chooses values (38, 25);
insert into Vote values (null, null) -- VoteId 39;
insert into Chooses values (39, 25);
insert into Vote values (null, null) -- VoteId 40;
insert into Chooses values (40, 26);

insert into Survey values (4, '420420', 'Wünschen Sie einen Wechsel in der Führungsposition?', '2018-09-01', '2018-09-15', 5, null, 2, 0, 1, null); -- SvyId 10
insert into Asking values (10, 1, '420420');
insert into Asking values (10, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 10 nötig (YesNoDontKnow) => AnsIds 27, 28, 29
insert into Vote values (null, null) -- VoteId 41;
insert into Chooses values (41, 27);
insert into Vote values (null, null) -- VoteId 42;
insert into Chooses values (42, 27);
insert into Vote values (null, null) -- VoteId 43;
insert into Chooses values (43, 27);
insert into Vote values (null, null) -- VoteId 44;
insert into Chooses values (44, 27);
insert into Vote values (null, null) -- VoteId 45;
insert into Chooses values (45, 28);
insert into Vote values (null, null) -- VoteId 46;
insert into Chooses values (46, 29);

insert into Survey values (4, '420420', 'Sind Sie mit der Haltung Ihrer Vorgesetzen einverstanden?', '2018-11-01', '2018-11-15', 5, null, 2, 0, 1, null); -- SvyId 11
insert into Asking values (11, 1, '420420');
insert into Asking values (11, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 11 nötig (YesNoDontKnow) => AnsIds 30, 31, 32
insert into Vote values (null, null) -- VoteId 47;
insert into Chooses values (47, 31);
insert into Vote values (null, null) -- VoteId 48;
insert into Chooses values (48, 32);
insert into Vote values (null, null) -- VoteId 49;
insert into Chooses values (49, 31);
insert into Vote values (null, null) -- VoteId 50;
insert into Chooses values (50, 31);
insert into Vote values (null, null) -- VoteId 51;
insert into Chooses values (51, 31);
insert into Vote values (null, null) -- VoteId 52;
insert into Chooses values (52, 31);

insert into Survey values (4, '420420', 'Sind Sie mit der Haltung Ihrer Vorgesetzen einverstanden?', '2018-11-15', '2018-11-30', 5, null, 2, 0, 1, null); -- SvyId 12
insert into Asking values (12, 1, '420420');
insert into Asking values (12, 2, '420420');
-- Wegen Trigger kein insert into AnswerOptions(...) für Svy 12 nötig (YesNoDontKnow) => AnsIds 33, 34, 35
insert into Vote values (null, null) -- VoteId 53;
insert into Chooses values (53, 34);
insert into Vote values (null, null) -- VoteId 54;
insert into Chooses values (54, 34);
insert into Vote values (null, null) -- VoteId 55;
insert into Chooses values (55, 34);
insert into Vote values (null, null) -- VoteId 56;
insert into Chooses values (56, 34);
insert into Vote values (null, null) -- VoteId 57;
insert into Chooses values (57, 34);
insert into Vote values (null, null) -- VoteId 58;
insert into Chooses values (58, 34);
commit;



begin transaction;
insert into Customer values ('180517', 'castiel gmbh.', 'jack@castiel.com', 'jackkline', null, null, 1, 'stairway', '0000', 'to', 'heaven', 24, 6, (select getdate()), 1, 3, 0, null, null, 10);

insert into Bill values('180517', 0.34, (select dateadd(month, -7, convert(date,'2019-01-26'))), 1, 1);
insert into Bill values('180517', 1.35, (select dateadd(month, -1, convert(date,'2019-01-26'))), 1, 1);

insert into Department values (1, 'Angel', '180517');
insert into Department values (2, 'Nephilim', '180517');

insert into Person values (null, null); -- PersId 6
insert into Person values (null, null); -- PersId 7
insert into Person values (null, null); -- PersId 8

insert into Employs values (1, '180517', 6);
insert into Employs values (1, '180517', 7);
insert into Employs values (2, '180517', 8);

insert into SurveyCategory values (1, '180517', 'Supernatural questions', 0);

insert into Survey values (1, '180517', 'Is Jack stronger than Lucifer?', '2018-05-18', '2018-07-18', 3, null, 5, 0, 1, null); -- SvyId 13
insert into Asking values (13, 1, '180517');
insert into Asking values (13, 2, '180517');
insert into AnswerOption values (13, 'Yes', null); -- AnsId 36
insert into AnswerOption values (13, 'Definitely', null); -- AnsId 37

insert into Vote values (null, null); -- VoteId 59
insert into Chooses values (59, 36);
insert into Vote values (null, null); -- VoteId 60
insert into Chooses values (60, 37);
insert into Vote values (null, null); -- VoteId 61
insert into Chooses values (61, 37);

insert into Survey values (1, '180517', 'Is Lucifer a bitch?', '2018-12-24', '2019-01-04', 3, null, 13, 0, 1, null); -- SvyId 14
insert into Asking values (14, 1, '180517');
insert into Asking values (14, 2, '180517');
-- Due to trigger AnswerOptions (38, 39, 40, 41) are created automatically for SvyId 14 (LikertScale6)
insert into AnswerOption values (14, 'A little', 1); -- AnsId 42
insert into AnswerOption values (14, 'Totally', 0); -- AnsId 43

insert into Vote values (null, null); -- VoteId 62
insert into Chooses values (62, 43);
insert into Vote values (null, null); -- VoteId 63
insert into Chooses values (63, 43);
insert into Vote values (null, null); -- VoteId 64
insert into Chooses values (64, 41);
insert into Vote values (null, null); -- VoteId 65
insert into Chooses values (65, 40);
insert into Vote values (null, null); -- VoteId 66
insert into Chooses values (66, 40);

insert into Survey values (1, '180517', 'Do you like the Chevy Impala?', '2018-12-26', '2018-12-27', 3, null, 3, 0, 1, null); -- SvyId 15
insert into Asking values (15, 1, '180517');
insert into Asking values (15, 2, '180517');
-- Due to trigger no insert into AnswerOptions necessary for SvyId 15 (TrafficLight) => AnsIds 44, 45, 46

insert into Vote values (null, null); -- VoteId 67
insert into Chooses values (67, 44);
insert into Vote values (null, null); -- VoteId 68
insert into Chooses values (68, 44);
insert into Vote values (null, null); -- VoteId 69
insert into Chooses values (69, 44);
insert into Vote values (null, null); -- VoteId 70
insert into Chooses values (70, 45);

insert into Survey values (1, '180517', 'Do you like the Chevy Impala?', '2019-01-29', '2019-01-30', 3, null, 3, 0, 1, null); -- SvyId 16
insert into Asking values (16, 1, '180517');
insert into Asking values (16, 2, '180517');
-- Due to trigger no insert into AnswerOptions necessary for SvyId 16 (TrafficLight) => AnsIds 47, 48, 49

insert into Vote values (null, null); -- VoteId 71
insert into Chooses values (71, 47);
insert into Vote values (null, null); -- VoteId 72
insert into Chooses values (72, 47);
insert into Vote values (null, null); -- VoteId 73
insert into Chooses values (73, 47);
insert into Vote values (null, null); -- VoteId 74
insert into Chooses values (74, 48);

insert into Survey values (1, '180517', 'Do you like the Chevy Impala?', '2019-01-01', '2019-01-02', 3, null, 3, 1, 1, null); -- SvyId 17
insert into Asking values (17, 1, '180517');
insert into Asking values (17, 2, '180517');
-- Due to trigger no insert into AnswerOptions necessary for SvyId 17 (TrafficLight) => AnsIds 50, 51, 52

insert into Vote values (null, null); -- VoteId 75
insert into Chooses values (75, 50);
insert into Vote values (null, null); -- VoteId 76
insert into Chooses values (76, 50);
insert into Vote values (null, null); -- VoteId 77
insert into Chooses values (77, 50);
insert into Vote values (null, null); -- VoteId 78
insert into Chooses values (78, 50);
commit;


begin transaction;
insert into Survey values (1, '420420', 'Wie produktiv würden Sie ihren heutigen Arbeitstag selbst einschätzen?', (select dateadd(day, -2, (select getdate()))), (select dateadd(day, -1, (select getdate()))), 5, null, 16, 1, 1, 2592000000000); -- SvyId 18
insert into Asking values (18, 1, '420420');
insert into Asking values (18, 2, '420420');
-- Due to trigger AnswerOptions (53 - 59) are created automatically for SvyId 18 (LikertScale9)
insert into AnswerOption values (18, 'Wenig', 1);
insert into AnswerOption values (18, 'Sehr', 0); 
commit;
go

--select * from Person;
--select * from Employs order by persId;
--select * from Customer;
--select * from AnswerOption;
--select * from Survey;