use SimpleQDB;
set nocount on;
go


insert into PaymentMethod values (1, 'Visa');
insert into Customer values ('m4rku5', 'musterreis gmbh.', 'm@rk.us', 'asdfjklö', null, 'teststraße', '420', 'teststadt', 'testland', 1, 0);
insert into Department values ('abteilung', 'm4rku5');
insert into [Group] values ('m4rku5', 'Testgruppe');
insert into [Contains] values (1, 'abteilung', 'm4rku5', 3);
insert into AskedPerson values ('abteilung', 'm4rku5');
insert into AskedPerson values ('abteilung', 'm4rku5');
insert into AskedPerson values ('abteilung', 'm4rku5');
insert into AskedPerson values ('abteilung', 'm4rku5');
insert into SurveyType values (1, 'Ja/Nein');
insert into SurveyType values (2, 'Ampel');
insert into SurveyType values (3, '1Wort');
insert into SurveyCategory values ('m4rku5', 'Testfrage');
insert into Answer values (1, 'Ja');
insert into Answer values (2, 'Nein');
insert into Answer values (3, 'Grün');
insert into Answer values (4, 'Gelb');
insert into Answer values (5, 'Rot');
select * from Customer;