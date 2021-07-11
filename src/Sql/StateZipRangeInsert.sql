--https://pt.wikipedia.org/wiki/C%C3%B3digo_de_Endere%C3%A7amento_Postal

select * from [dbo].[StateProvince] where CountryId = 17 order by DisplayOrder 
select * from [dbo].[Country] where name like '%Brasil%'
select * from [dbo].[SU_StateProvinceZipPostalRange]

select * from 
	[dbo].[StateProvince] sp
		inner join [dbo].[SU_StateProvinceZipPostalRange] spzpr
			on spzpr.StateProvinceId = sp.Id
where 
	CountryId = 17 
order by ZipPostalStart 


INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 76, '01000','09999')--São Paulo
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 76, '11000','19999')--São Paulo
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 77, '20000','28999')--Rio de Janeiro
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 86, '29000','29999')--Espirito Santo
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 78, '30000','30000')--Minas Gerais
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 83, '40000','48999')--Bahia
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 101, '49000','49999')--Sergipe
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 94, '50000','56999')--Pernambuco
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 80, '57000','57999')--Alagoas
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 92, '58000','58999')--Paraíba
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 96, '59000','59999')--Rio Grande do Norte
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 84, '60000','63990')--Ceará
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 95, '64000','64990')--Piauí
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 88, '65000','65990')--Maranhão
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 91, '66000','68890')--Pará
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 81, '68900','68999')--Amapá
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 82, '69000','69299')--Amazonas
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 82, '69500','69999')--Amazonas
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 79, '69400','69499')--Acre
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 99, '69300','69399')--Roraima
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 85, '70000','73699')--Distrito Federal
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 87, '73700','76799')--Goiás
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 98, '76800','76999')--Rondônia
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 102, '77000','77999')--Tocantins
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 89, '78000','78899')--Mato Grosso
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 90, '79000','79999')--Mato Grosso do Sul
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 93, '80000','87999')--Paraná
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 100, '88000','89999')--Santa Catarina
INSERT [DBO].[SU_StateProvinceZipPostalRange] (StateProvinceId, ZipPostalStart, ZipPostalEnd) values ( 97, '90000','99999')--Rio Grande do Sul


