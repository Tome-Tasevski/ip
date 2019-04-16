USE [IdsDatabase]
GO

INSERT INTO [dbo].[UserClaims]
           ([Id]
           ,[UserId]
           ,[ClaimId]
           ,[Value])
     VALUES
           ('1','2','15','Alice Smith'),
		   ('2','2','13','Alice'),
		   ('3','2','14','Smith'),
		   ('4','2','18','AliceSmith@email.com'),
		   ('5','2','20','User'),
		   ('12','2','17','test2'),
		   ('6','1','15','Bob Smith'),
		   ('7','1','13','Bob'),
		   ('8','1','14','Smith'),
		   ('9','1','18','BobSmith@email.com'),
		   ('10','1','20','SP2.Admin'),
		   ('11','1','17','test2'),
		   ('13','3','15','Admin'),
		   ('14','3','13','Admin'),
		   ('15','3','14','A'),
		   ('16','3','18','admin@admin.com'),
		   ('17','3','20','SP1.Admin'),
		   ('18','3','17','test2')

GO


