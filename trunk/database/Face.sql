Create database FaceRecognizing
GO
use FaceRecognizing
GO
Create Table Face
(
	id integer not null IDENTITY(1,1)PRIMARY KEY CLUSTERED ,
	name nvarchar(50),
	phone nvarchar(20),
	email nvarchar(50),
	birthday datetime,
	img image
)
set dateformat mdy