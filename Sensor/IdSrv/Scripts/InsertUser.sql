USE [IdsDatabase]
GO

INSERT INTO [dbo].[Users]
           ([UserId]
           ,[Username]
           ,[Password]
           ,[TenantId]
           ,[Provider]
           ,[IsExternalUser]
           ,[ExternalUserId])
     VALUES
           ('1','bob','bob','2','local',0, null),
		   ('2','alice','alice','2','local',0, null),
		   ('3','admin','admin','2','local',0, null)
GO


