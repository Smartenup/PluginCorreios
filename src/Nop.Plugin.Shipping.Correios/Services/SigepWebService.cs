using Nop.Core.Data;
using Nop.Plugin.Shipping.Correios.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Shipping.Correios.Services
{
    public class SigepWebService : ISigepWebService
    {

        private readonly IRepository<PlpSigepWeb> _plpRepository;
        private readonly IRepository<PlpSigepWebShipment> _plpShipmentRepository;
        private readonly IRepository<PlpSigepWebEtiqueta> _plpSigepWebEtiquetaRepository;
        

        public SigepWebService(IRepository<PlpSigepWeb> plpRepository, 
            IRepository<PlpSigepWebShipment> plpShipmentRepository,
            IRepository<PlpSigepWebEtiqueta> plpSigepWebEtiquetaRepository
            )
        {
            _plpRepository = plpRepository;
            _plpShipmentRepository = plpShipmentRepository;
            _plpSigepWebEtiquetaRepository = plpSigepWebEtiquetaRepository;

        }

        public void DeletePlp(PlpSigepWeb plpSigepWeb)
        {
            if (plpSigepWeb == null)
                throw new ArgumentNullException("plpSigepWeb");

            plpSigepWeb.Deleted = true;
            UpdatePlp(plpSigepWeb);
        }

        public void DeletePlpShipment(PlpSigepWebShipment plpSigepWebShipment)
        {
            if (plpSigepWebShipment == null)
                throw new ArgumentNullException("plpSigepWebShipment");

            _plpShipmentRepository.Delete(plpSigepWebShipment);
        }

        public PlpSigepWeb GetPlp(PlpSigepWebStatus status)
        {
            var query = _plpRepository.Table;

            query = query.Where(o => o.PlpStatusId == (int)status );
            query = query.Where(o => !o.Deleted);

            if (query.Count() != 0)
                return query.FirstOrDefault();

            return null;
        }

        public PlpSigepWeb GetPlp(int Id)
        {
            var query = _plpRepository.Table;

            query = query.Where(o => o.Id == (int)Id);
            query = query.Where(o => !o.Deleted);

            if (query.Count() != 0)
                return query.FirstOrDefault();

            return null;
        }

        public void InsertPlp(PlpSigepWeb plpSigepWeb)
        {
            if (plpSigepWeb == null)
                throw new ArgumentNullException("plpSigepWeb");

            _plpRepository.Insert(plpSigepWeb);
        }

        public void InsertPlpShipment(PlpSigepWebShipment plpSigepWebShipment)
        {
            if (plpSigepWebShipment == null)
                throw new ArgumentNullException("plpSigepWebShipment");

            _plpShipmentRepository.Insert(plpSigepWebShipment);

        }

        public void UpdateEtiquetaCorreios(PlpSigepWebEtiqueta etiqueta)
        {
            if (etiqueta == null)
                throw new ArgumentNullException("etiqueta");

            _plpSigepWebEtiquetaRepository.Update(etiqueta);

        }

        public void InsertEtiquetaCorreios(PlpSigepWebEtiqueta etiqueta)
        {
            if (etiqueta == null)
                throw new ArgumentNullException("etiqueta");

            _plpSigepWebEtiquetaRepository.Insert(etiqueta);
        }
        

        public void UpdatePlp(PlpSigepWeb plpSigepWeb)
        {
            if (plpSigepWeb == null)
                throw new ArgumentNullException("plpSigepWeb");

            _plpRepository.Update(plpSigepWeb);
        }


        public IList<PlpSigepWebShipment> GetShipmentsByIds(int[] plpSigepWebShimentId)
        {

            if (plpSigepWebShimentId == null || plpSigepWebShimentId.Length == 0)
                return new List<PlpSigepWebShipment>();

            var query = from o in _plpShipmentRepository.Table
                        where plpSigepWebShimentId.Contains(o.Id)
                        select o;
            var shipments = query.ToList();
            //sort by passed identifiers
            var sortedShipments = new List<PlpSigepWebShipment>();
            foreach (int id in plpSigepWebShimentId)
            {
                var shipment = shipments.Find(x => x.Id == id);
                if (shipment != null)
                    sortedShipments.Add(shipment);
            }
            return sortedShipments;
        }


        public PlpSigepWebShipment GetPlpShipment(int Id)
        {
            if (Id == 0)
                return null;

            return _plpShipmentRepository.GetById(Id);
        }

        

    }
}
