
Create Table [dbo].[ShippingCorreiosConfiguracaoServicos]
(
	[ShippingCorreiosConfiguracaoServicosId] [int] IDENTITY(1,1) NOT NULL,
	[CodigoServicoEstimativa]	varchar(10) not null,
    [CodigoServicoEnvioPLP] varchar(10) not null,
	[DescricaoServicoPedido] varchar(50) not null,
	[CodigoServicoValorDeclaradoEnvioPLP] varchar(10) not null,
	[ValorMinimoValorDeclaradoEnvioPLP] decimal not null,
	[TamanhoMinimo] decimal null,
	[TamanhoMaximo] decimal null,
)


insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '04510','04510', 'PAC sem contrato', '064', 50)

insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '04014','04014', 'SEDEX sem contrato', '019', 75)

insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '04014','04014', 'SEDEX HOJE CONTRATO', '019', 75)

insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '04014','04014', 'SEDEX 10 CONTRATO', '019', 75)

insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '03140','03140', 'SEDEX 12 CONTRATO', '019', 75)

insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '03220','03220', 'SEDEX CONTRATO', '019', 75)

insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '03298','03298', 'PAC CONTRATO', '064', 50)

insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '04227','04227', 'PAC MINI CONTRATO', '064', 50)

insert ShippingCorreiosConfiguracaoServicos (CodigoServicoEstimativa, CodigoServicoEnvioPLP, DescricaoServicoPedido, CodigoServicoValorDeclaradoEnvioPLP, ValorMinimoValorDeclaradoEnvioPLP) 
values ( '03212','03212', 'SEDEX CONTRATO GDES FORMATOS', '019', 75)

select * from PlpSigepWebEtiqueta