using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Shipping.Correios.Domain.Serialization
{
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {

        private EnvelopeHeader headerField;

        private EnvelopeBody bodyField;

        /// <remarks/>
        public EnvelopeHeader Header
        {
            get
            {
                return this.headerField;
            }
            set
            {
                this.headerField = value;
            }
        }

        /// <remarks/>
        public EnvelopeBody Body
        {
            get
            {
                return this.bodyField;
            }
            set
            {
                this.bodyField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeHeader
    {

        private string xOPNETTransactionTraceField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("X-OPNET-Transaction-Trace", Namespace = "http://opnet.com")]
        public string XOPNETTransactionTrace
        {
            get
            {
                return this.xOPNETTransactionTraceField;
            }
            set
            {
                this.xOPNETTransactionTraceField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeBody
    {

        private buscaEventosResponse buscaEventosResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://resource.webservice.correios.com.br/")]
        public buscaEventosResponse buscaEventosResponse
        {
            get
            {
                return this.buscaEventosResponseField;
            }
            set
            {
                this.buscaEventosResponseField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://resource.webservice.correios.com.br/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://resource.webservice.correios.com.br/", IsNullable = false)]
    public partial class buscaEventosResponse
    {

        private @return returnField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "")]
        public @return @return
        {
            get
            {
                return this.returnField;
            }
            set
            {
                this.returnField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class @return
    {

        private decimal versaoField;

        private byte qtdField;

        private returnObjeto objetoField;

        /// <remarks/>
        public decimal versao
        {
            get
            {
                return this.versaoField;
            }
            set
            {
                this.versaoField = value;
            }
        }

        /// <remarks/>
        public byte qtd
        {
            get
            {
                return this.qtdField;
            }
            set
            {
                this.qtdField = value;
            }
        }

        /// <remarks/>
        public returnObjeto objeto
        {
            get
            {
                return this.objetoField;
            }
            set
            {
                this.objetoField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class returnObjeto
    {

        private string numeroField;

        private string siglaField;

        private string nomeField;

        private string categoriaField;

        private returnObjetoEvento[] eventoField;

        /// <remarks/>
        public string numero
        {
            get
            {
                return this.numeroField;
            }
            set
            {
                this.numeroField = value;
            }
        }

        /// <remarks/>
        public string sigla
        {
            get
            {
                return this.siglaField;
            }
            set
            {
                this.siglaField = value;
            }
        }

        /// <remarks/>
        public string nome
        {
            get
            {
                return this.nomeField;
            }
            set
            {
                this.nomeField = value;
            }
        }

        /// <remarks/>
        public string categoria
        {
            get
            {
                return this.categoriaField;
            }
            set
            {
                this.categoriaField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("evento")]
        public returnObjetoEvento[] evento
        {
            get
            {
                return this.eventoField;
            }
            set
            {
                this.eventoField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class returnObjetoEvento
    {

        private string tipoField;

        private byte statusField;

        private string dataField;

        private string horaField;

        private string descricaoField;

        private string detalheField;

        private string localField;

        private uint codigoField;

        private string cidadeField;

        private string ufField;

        private returnObjetoEventoEndereco enderecoField;

        /// <remarks/>
        public string tipo
        {
            get
            {
                return this.tipoField;
            }
            set
            {
                this.tipoField = value;
            }
        }

        /// <remarks/>
        public byte status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }

        /// <remarks/>
        public string data
        {
            get
            {
                return this.dataField;
            }
            set
            {
                this.dataField = value;
            }
        }

        /// <remarks/>
        public string hora
        {
            get
            {
                return this.horaField;
            }
            set
            {
                this.horaField = value;
            }
        }

        /// <remarks/>
        public string descricao
        {
            get
            {
                return this.descricaoField;
            }
            set
            {
                this.descricaoField = value;
            }
        }

        /// <remarks/>
        public string detalhe
        {
            get
            {
                return this.detalheField;
            }
            set
            {
                this.detalheField = value;
            }
        }

        /// <remarks/>
        public string local
        {
            get
            {
                return this.localField;
            }
            set
            {
                this.localField = value;
            }
        }

        /// <remarks/>
        public uint codigo
        {
            get
            {
                return this.codigoField;
            }
            set
            {
                this.codigoField = value;
            }
        }

        /// <remarks/>
        public string cidade
        {
            get
            {
                return this.cidadeField;
            }
            set
            {
                this.cidadeField = value;
            }
        }

        /// <remarks/>
        public string uf
        {
            get
            {
                return this.ufField;
            }
            set
            {
                this.ufField = value;
            }
        }

        /// <remarks/>
        public returnObjetoEventoEndereco endereco
        {
            get
            {
                return this.enderecoField;
            }
            set
            {
                this.enderecoField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class returnObjetoEventoEndereco
    {
        private uint codigoField;

        private string cepField;

        private string logradouroField;

        private string numeroField;

        private string localidadeField;

        private string bairroField;

        private string ufField;

        public uint codigo
        {
            get
            {
                return codigoField;
            }
            set
            {
                codigoField = value;
            }
        }

        private string cep
        {
            get { return this.cepField; }
            set { cepField = value; }

        }

        /// <remarks/>
        public string logradouro
        {
            get
            {
                return logradouroField;
            }
            set
            {
                logradouroField = value;
            }
        }

        /// <remarks/>
        public string numero
        {
            get
            {
                return numeroField;
            }
            set
            {
                numeroField = value;
            }
        }

        /// <remarks/>
        public string localidade
        {
            get
            {
                return localidadeField;
            }
            set
            {
                localidadeField = value;
            }
        }

        /// <remarks/>
        public string bairro
        {
            get
            {
                return bairroField;
            }
            set
            {
                bairroField = value;
            }
        }

        /// <remarks/>
        public string uf
        {
            get
            {
                return ufField;
            }
            set
            {
                ufField = value;
            }
        }
    }
}
