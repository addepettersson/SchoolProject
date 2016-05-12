using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Web.Http;
using System.Web.Http.Description;
using ProjectApi.Models;

namespace ProjectApi.Controllers
{
    public class JournalController : ApiController
    {
        #region DriverConsumationWindowRegNr

        [HttpPost]
        [ActionName("GetRegnrOnDriver")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(List<JournalModel>))]
        public HttpResponseMessage GetRegnrOnDriver([FromBody] UserModel user)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals = db.DriverJournal.Where(x => x.Driver_Id == user.UserId).Select(x => x.Car_Id).Distinct().ToList();
                        var cars = db.Car.ToList();

                        List<JournalModel> RegnrList = new List<JournalModel>();
                        foreach (var item in journals)
                        {
                            foreach (var car in cars)
                            {
                                if (car.Id == item)
                                {
                                    var regnr = car.Regnr;
                                    JournalModel journal = new JournalModel
                                    {
                                        CarId = item,
                                        Regnr = regnr
                                    };
                                    RegnrList.Add(journal);
                                }
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, RegnrList);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetAllJournalsDriver")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetAllJournals([FromBody] JournalModel user)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(x => x.Car_Id == user.CarId)
                                .OrderByDescending(x => x.Date)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }

                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 2)
                        {
                            var lastJournal = journals.FirstOrDefault();
                            var firstJournal = journals.LastOrDefault();
                            var sum = 0;
                            if (lastJournal != null && firstJournal != null)
                            {
                                sum = lastJournal.mileage - firstJournal.mileage;
                            }
                            var totalConsumation = gasamount / sum;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        else
                        {
                            var car = db.Car.SingleOrDefault(x => x.Id == user.CarId);

                            var journalmileage = journals.FirstOrDefault();
                            decimal totalConsumation = 0;
                            if (journalmileage != null && car != null)
                            {
                                var mileage = car.OriginalMileage;
                                var sumMileage = journalmileage.mileage - mileage;
                                totalConsumation = journalmileage.FuelAmount / sumMileage;
                            }
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationOnMonth")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetJournalOnMonth([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(
                                x => x.Car_Id == journal.CarId &&
                                    x.Date.Month == journal.Date.Month && x.Date.Year == journal.Date.Year)
                                .OrderByDescending(x => x.Date)
                                .Where(x => x.Date.Year == journal.Date.Year)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 2)
                        {
                            var lastJournal = journals.FirstOrDefault();
                            var firstJournal = journals.LastOrDefault();
                            var sum = 0;
                            if (lastJournal != null && firstJournal != null)
                            {
                                sum = lastJournal.mileage - firstJournal.mileage;
                            }
                            var totalConsumation = gasamount / sum;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        else
                        {
                            var journalLastmonth = db.DriverJournal.Where(x => x.Car_Id == journal.CarId &&
                                    x.Date.Month == journal.Date.Month - 1 && x.Date.Year == journal.Date.Year)
                                    .OrderByDescending(x => x.Date).ToList();
                            var mileageLastMonth = journalLastmonth.FirstOrDefault();
                            var journalmileage = journals.FirstOrDefault();
                            if (mileageLastMonth != null && journalmileage != null)
                            {
                                var sumMileage = journalmileage.mileage - mileageLastMonth.mileage;
                                var totalConsumation = journalmileage.FuelAmount / sumMileage;
                                consumationModel.Consumation = totalConsumation;
                                consumationModel.Cost = totalFuelPrice;
                                return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                            }
                            else
                            {
                                int originalMilage =
                                db.Car.Where(x => x.Id == journal.CarId)
                                    .Select(x => x.OriginalMileage)
                                    .SingleOrDefault();
                                var sumMileage = journalmileage.mileage - originalMilage;
                                var totalConsumation = journalmileage.FuelAmount / sumMileage;
                                consumationModel.Consumation = totalConsumation;
                                consumationModel.Cost = totalFuelPrice;
                                return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationPerYear")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetJournalPerYear([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals = db.DriverJournal.Where(x => x.Car_Id == journal.CarId && 
                        x.Date.Year == journal.Date.Year).OrderByDescending(x => x.Date)
                        .ToList();

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 2)
                        {
                            var lastJournal = journals.FirstOrDefault();
                            var firstJournal = journals.LastOrDefault();
                            decimal totalConsumation = 0;
                            if (lastJournal != null && firstJournal != null)
                            {
                                var sum = lastJournal.mileage - firstJournal.mileage;
                                totalConsumation = gasamount / sum;
                            }

                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        else
                        {
                            var journalmileage = journals.FirstOrDefault();
                            var lastJournal = db.DriverJournal.Where(x => x.Car_Id == journal.CarId
                                && x.Date < journalmileage.Date).OrderByDescending(x => x.Date).ToList();
                            if (lastJournal.Count != 0)
                            {
                                var latestmilage = lastJournal.FirstOrDefault();

                                decimal totalConsumation = 0;
                                if (latestmilage != null && journalmileage != null)
                                {
                                    var sum = journalmileage.mileage - latestmilage.mileage;
                                    totalConsumation = journalmileage.FuelAmount / sum;
                                }

                                consumationModel.Consumation = totalConsumation;
                                consumationModel.Cost = totalFuelPrice;
                            }
                            else
                            {
                                var car = db.Car.FirstOrDefault(x => x.Id == journal.CarId);
                                decimal totalConsumation = 0;
                                if (car != null && journalmileage != null)
                                {
                                    var sum = journalmileage.mileage - car.OriginalMileage;
                                    totalConsumation = journalmileage.FuelAmount / sum;
                                }

                                consumationModel.Consumation = totalConsumation;
                                consumationModel.Cost = totalFuelPrice;
                            }

                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationOnLastDrive")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetConsumationOnLastDrive([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(x => x.Car_Id == journal.CarId &&
                            x.FuelType.FuelType1 != "El")
                                .OrderByDescending(x => x.Date)
                                .ToList();

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        var latestJournal = journals.FirstOrDefault();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        if (latestJournal != null)
                        {
                            gasamount = latestJournal.FuelAmount;
                            totalFuelPrice = latestJournal.TotalPrice;
                        }

                        var latestjournalToCount =
                            db.DriverJournal.Where(
                                x => x.Car_Id == journal.CarId &&
                                    x.Date < latestJournal.Date).OrderByDescending(x => x.Date).ToList();

                        var mileageToCount = latestjournalToCount.FirstOrDefault();

                        if (latestJournal != null & mileageToCount != null)
                        {
                            var summileage = latestJournal.mileage - mileageToCount.mileage;

                            var totalConsumation = gasamount / summileage;

                            ConsumationModel consumationModel = new ConsumationModel();
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }

                        var car = db.Car.SingleOrDefault(x => x.Id == journal.CarId);
                        if (latestJournal != null)
                        {
                            var summileage = latestJournal.mileage - car.OriginalMileage;

                            var totalConsumation = gasamount / summileage;

                            ConsumationModel consumationModel = new ConsumationModel();
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        #endregion


        #region DriverConsumationWindowAllCars
        [HttpPost]
        [ActionName("GetAllJournalsAllCars")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetAllJournalsAllCars([FromBody] JournalModel user)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals = db.DriverJournal.Where(x => x.Driver_Id == user.DriverId)
                                .OrderByDescending(x => x.Date)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }

                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 1)
                        {
                            var averageGasAmount = gasamount / journals.Count;
                            var averagePrice = totalFuelPrice / journals.Count;
                            consumationModel.Consumation = averageGasAmount;
                            consumationModel.Cost = averagePrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationOnMonthAllCarsDriver")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetConsumationOnMonthAllCarsDriver([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(
                                x => x.Driver_Id == journal.DriverId &&
                                    x.Date.Month == journal.Date.Month && x.Date.Year == journal.Date.Year)
                                    .OrderByDescending(x => x.Date).ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 1)
                        {
                            var totalGasAmount = gasamount / journals.Count;
                            var totalPrice = totalFuelPrice / journals.Count;
                            consumationModel.Consumation = totalGasAmount;
                            consumationModel.Cost = totalPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationPerYearAllCars")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetJournalPerYearAllCars([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(
                                x => x.Driver_Id == journal.DriverId &&
                                    x.Date.Year == journal.Date.Year).OrderByDescending(x => x.Date).ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 1)
                        {
                            var totalGasAmount = gasamount / journals.Count;
                            var totalPrice = totalFuelPrice / journals.Count;
                            consumationModel.Consumation = totalGasAmount;
                            consumationModel.Cost = totalPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationOnLastDriveAllCars")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetJournalOnLastDriveAllCars([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(
                                x => x.Driver_Id == journal.DriverId && x.FuelType.FuelType1 != "El").OrderByDescending(x => x.Date).ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        var latestJournal = journals.FirstOrDefault();
                        if (latestJournal != null)
                        {
                            gasamount = latestJournal.FuelAmount;
                            totalFuelPrice = latestJournal.TotalPrice;
                        }


                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 1)
                        {

                            consumationModel.Consumation = gasamount;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }
        #endregion


        #region AdminConsumationWindow


        [HttpPost]
        [ActionName("GetAllJournalsOnCarId")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetAllJournalsOnCarId([FromBody] JournalModel user)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(x => x.Car_Id == user.CarId)
                                .OrderByDescending(x => x.Date)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }

                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }

                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 2)
                        {
                            var lastJournal = journals.FirstOrDefault();
                            var firstJournal = journals.LastOrDefault();

                            var sum = lastJournal.mileage - firstJournal.mileage;
                            var totalConsumation = gasamount / sum;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }

                        var car = db.Car.SingleOrDefault(x => x.Id == user.CarId);
                        var journalmileage = journals.FirstOrDefault();
                        if (car != null && journalmileage != null)
                        {
                            var mileage = car.OriginalMileage;
                            var sumMileage = journalmileage.mileage - mileage;
                            var totalConsumation = journalmileage.FuelAmount / sumMileage;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetJournalsOnMonthOnCarId")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetJournalsOnMonthOnCarId([FromBody] JournalModel car)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(x => x.Car_Id == car.CarId && x.Date.Month == car.Date.Month)
                                .OrderByDescending(x => x.Date).ThenBy(x => x.mileage)
                                .ToList();

                        journals = journals.OrderByDescending(x => x.mileage).ToList();
                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }

                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 2)
                        {
                            var lastJournal = journals.FirstOrDefault();
                            var firstJournal = journals.LastOrDefault();
                            if (lastJournal != null && firstJournal != null)
                            {
                                var sum = lastJournal.mileage - firstJournal.mileage;
                                var totalConsumation = gasamount / sum;
                                consumationModel.Consumation = totalConsumation;
                                consumationModel.Cost = totalFuelPrice;
                                return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                            }
                        }
                        else
                        {
                            var journalLastmonth = db.DriverJournal.Where(x => x.Car_Id == car.CarId &&
                                        x.Date.Month == car.Date.Month - 1).OrderByDescending(x => x.Date).ToList();
                            var mileageLastMonth = journalLastmonth.FirstOrDefault();
                            var journalmileage = journals.FirstOrDefault();
                            if (mileageLastMonth != null && journalmileage != null)
                            {
                                var sumMileage = journalmileage.mileage - mileageLastMonth.mileage;
                                var totalConsumation = journalmileage.FuelAmount / sumMileage;
                                consumationModel.Consumation = totalConsumation;
                                consumationModel.Cost = totalFuelPrice;
                                return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                            }
                            else
                            {
                                int originalMilage =
                                db.Car.Where(x => x.Id == car.CarId)
                                    .Select(x => x.OriginalMileage)
                                    .SingleOrDefault();
                                var sumMileage = journalmileage.mileage - originalMilage;
                                var totalConsumation = journalmileage.FuelAmount / sumMileage;
                                consumationModel.Consumation = totalConsumation;
                                consumationModel.Cost = totalFuelPrice;
                                return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetJournalsOnYearOnCarId")]
        [Authorize(Roles = "Admin")]
        [ResponseType(typeof(ConsumationModel))]
        public HttpResponseMessage GetJournalsOnYearOnCarId([FromBody] JournalModel user)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {

                        var journals =
                            db.DriverJournal.Where(x => x.Car_Id == user.CarId && x.Date.Year == user.Date.Year)
                                .OrderByDescending(x => x.Date)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;
                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }

                        foreach (var item in journals)
                        {
                            gasamount += item.FuelAmount;
                            totalFuelPrice += item.TotalPrice;
                        }

                        ConsumationModel consumationModel = new ConsumationModel();
                        if (journals.Count >= 2)
                        {
                            var lastJournal = journals.FirstOrDefault();
                            var firstJournal = journals.LastOrDefault();

                            var sum = lastJournal.mileage - firstJournal.mileage;
                            var totalConsumation = gasamount / sum;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }

                        var car = db.Car.SingleOrDefault(x => x.Id == user.CarId);
                        var journalmileage = journals.FirstOrDefault();
                        if (car != null && journalmileage != null)
                        {
                            var mileage = car.OriginalMileage;
                            var sumMileage = journalmileage.mileage - mileage;
                            var totalConsumation = journalmileage.FuelAmount / sumMileage;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetSpecificDriverAllJournals")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetSpecificDriverAllJournals([FromBody] JournalModel user)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.OrderBy(x => x.Car_Id).ThenBy(x => x.Date).ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        var journalOnSelectedDriver = journals.Where(x => x.Driver_Id == user.DriverId).ToList();
                        if (journalOnSelectedDriver.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;

                        foreach (var item in journals)
                        {
                            if (item.Driver_Id == user.DriverId && lastjournal.Car_Id == item.Car_Id)
                            {
                                sumMiles += item.mileage - lastjournal.mileage;
                                gasamount += item.FuelAmount;
                                totalFuelPrice += item.TotalPrice;
                            }
                            else if (item.Driver_Id == user.DriverId)
                            {
                                int originalMilage =
                                    db.Car.Where(x => x.Id == item.Car_Id)
                                        .Select(x => x.OriginalMileage)
                                        .SingleOrDefault();

                                sumMiles += item.mileage - originalMilage;
                                gasamount += item.FuelAmount;
                                totalFuelPrice += item.TotalPrice;
                            }
                            lastjournal = item;
                        }

                        var totalConsumation = gasamount / sumMiles;
                        consumationModel.Consumation = totalConsumation;
                        consumationModel.Cost = totalFuelPrice;
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);

                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationOnMonthAllCarsOnDriverId")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetConsumationOnMonthAllCarsOnDriverId([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(x => x.Date.Month == journal.Date.Month)
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        var journalOnSelectedDriver = journals.Where(x => x.Driver_Id == journal.DriverId).ToList();
                        if (journalOnSelectedDriver.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;

                        foreach (var item in journals)
                        {
                            if (item.Driver_Id == journal.DriverId && lastjournal.Car_Id == item.Car_Id)
                            {
                                sumMiles += item.mileage - lastjournal.mileage;
                                gasamount += item.FuelAmount;
                                totalFuelPrice += item.TotalPrice;
                            }
                            else if (item.Driver_Id == journal.DriverId)
                            {
                                var journalLastmonth = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Month == journal.Date.Month - 1 && x.FuelType.FuelType1 != "El").OrderByDescending(x => x.Date)
                                        .ToList();
                                var mileageLastMonth = journalLastmonth.FirstOrDefault();
                                if (mileageLastMonth != null)
                                {
                                    sumMiles += item.mileage - mileageLastMonth.mileage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                else
                                {
                                    var originalMilage =
                                    db.Car.Where(x => x.Id == item.Car_Id)
                                        .Select(x => x.OriginalMileage)
                                        .SingleOrDefault();

                                    sumMiles += item.mileage - originalMilage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                            }
                            lastjournal = item;
                        }

                        var totalConsumation = gasamount / sumMiles;
                        consumationModel.Consumation = totalConsumation;
                        consumationModel.Cost = totalFuelPrice;
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);

                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationPerYearOnDriverId")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetJournalPerYearOnDriverId([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(x => x.Date.Year == journal.Date.Year)
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        var journalOnSelectedDriver = journals.Where(x => x.Driver_Id == journal.DriverId).ToList();
                        if (journalOnSelectedDriver.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;

                        foreach (var item in journals)
                        {

                            if (item.Driver_Id == journal.DriverId && lastjournal.Car_Id == item.Car_Id)
                            {
                                sumMiles += item.mileage - lastjournal.mileage;
                                gasamount += item.FuelAmount;
                                totalFuelPrice += item.TotalPrice;
                            }
                            else if (item.Driver_Id == journal.DriverId)
                            {
                                var journalLastYear = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Year == journal.Date.Year - 1 && x.FuelType.FuelType1 != "El").OrderByDescending(x => x.Date)
                                        .ToList();
                                var mileageLastYear = journalLastYear.FirstOrDefault();
                                if (mileageLastYear != null)
                                {
                                    sumMiles += item.mileage - mileageLastYear.mileage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                else
                                {
                                    var originalMilage =
                                    db.Car.Where(x => x.Id == item.Car_Id)
                                        .Select(x => x.OriginalMileage)
                                        .SingleOrDefault();

                                    sumMiles += item.mileage - originalMilage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                            }
                            lastjournal = item;
                        }

                        var totalConsumation = gasamount / sumMiles;
                        consumationModel.Consumation = totalConsumation;
                        consumationModel.Cost = totalFuelPrice;
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);

                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpGet]
        [ActionName("GetConsumationsOnAllCars")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetAllJournalsOnAllCars()
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.OrderBy(x => x.Car_Id).ThenBy(x => x.Date).ToList();

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;
                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        foreach (var item in journals)
                        {
                            if (lastjournal.Car_Id == item.Car_Id)
                            {
                                sumMiles += item.mileage - lastjournal.mileage;
                                gasamount += item.FuelAmount;
                                totalFuelPrice += item.TotalPrice;
                            }
                            else
                            {
                                var originalMilage =
                                    db.Car.Where(x => x.Id == item.Car_Id)
                                        .Select(x => x.OriginalMileage)
                                        .SingleOrDefault();

                                sumMiles += item.mileage - originalMilage;
                                gasamount += item.FuelAmount;
                                totalFuelPrice += item.TotalPrice;
                            }
                            lastjournal = item;
                        }

                        var totalConsumation = gasamount / sumMiles;
                        consumationModel.Consumation = totalConsumation;
                        consumationModel.Cost = totalFuelPrice;
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);

                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetConsumationOnMonthAllCars")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetAllJournalsOnMonthOnAllCars([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(x => x.Date.Month == journal.Date.Month && x.Date.Year == journal.Date.Year)
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }

                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;
                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        foreach (var item in journals)
                        {
                            if (lastjournal.Car_Id == item.Car_Id)
                            {
                                sumMiles += item.mileage - lastjournal.mileage;
                                gasamount += item.FuelAmount;
                                totalFuelPrice += item.TotalPrice;
                            }
                            else
                            {
                                var journalLastmonth = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Month == journal.Date.Month - 1 && x.FuelType.FuelType1 != "El").OrderByDescending(x => x.Date)
                                        .ToList();
                                var mileageLastMonth = journalLastmonth.FirstOrDefault();
                                if (mileageLastMonth != null)
                                {
                                    sumMiles += item.mileage - mileageLastMonth.mileage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                else
                                {
                                    int originalMilage =
                                    db.Car.Where(x => x.Id == item.Car_Id)
                                        .Select(x => x.OriginalMileage)
                                        .SingleOrDefault();

                                    sumMiles += item.mileage - originalMilage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                            }
                            lastjournal = item;
                        }

                        var totalConsumation = gasamount / sumMiles;
                        consumationModel.Consumation = totalConsumation;
                        consumationModel.Cost = totalFuelPrice;
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetAllJournalsOnYearOnAllCars")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetAllJournalsOnYearOnAllCars([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var journals =
                            db.DriverJournal.Where(x => x.Date.Year == journal.Date.Year)
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;

                        foreach (var item in journals)
                        {
                            if (lastjournal.Car_Id == item.Car_Id)
                            {
                                sumMiles += item.mileage - lastjournal.mileage;
                                gasamount += item.FuelAmount;
                                totalFuelPrice += item.TotalPrice;
                            }
                            else
                            {
                                var journalLastYear = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Year == journal.Date.Year - 1 && x.FuelType.FuelType1 != "El").OrderByDescending(x => x.Date)
                                        .ToList();
                                var mileageLastYear = journalLastYear.FirstOrDefault();
                                if (mileageLastYear != null)
                                {
                                    sumMiles += item.mileage - mileageLastYear.mileage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                else
                                {
                                    var originalMilage =
                                    db.Car.Where(x => x.Id == item.Car_Id)
                                        .Select(x => x.OriginalMileage)
                                        .SingleOrDefault();

                                    sumMiles += item.mileage - originalMilage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                //int originalMilage =
                                //    db.Car.Where(x => x.Id == item.Car_Id)
                                //        .Select(x => x.OriginalMileage)
                                //        .SingleOrDefault();

                                //sumMiles += item.mileage - originalMilage;
                                //gasamount += item.FuelAmount;
                                //totalFuelPrice += item.TotalPrice;
                            }
                            lastjournal = item;
                        }

                        var totalConsumation = gasamount / sumMiles;
                        consumationModel.Consumation = totalConsumation;
                        consumationModel.Cost = totalFuelPrice;
                        return Request.CreateResponse(HttpStatusCode.OK, consumationModel);

                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetAllJournalsOnMonthOnCarType")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetAllJournalsOnMonthOnCarType([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var cars = db.Car.Where(x => x.CarType_Id == journal.CarTypeId).ToList();

                        var journals =
                            db.DriverJournal.Where(x => x.Date.Month == journal.Date.Month)
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        if (journals.Count == 0 || cars.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;

                        foreach (var car in cars)
                        {
                            foreach (var item in journals)
                            {
                                if (lastjournal.Car_Id == item.Car_Id && item.Car_Id == car.Id)
                                {
                                    sumMiles += item.mileage - lastjournal.mileage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                else if (lastjournal.Car_Id == car.Id && item.Car_Id == car.Id)
                                {
                                    var lastMonth =
                                        db.DriverJournal.Where(x => x.Date.Month == journal.Date.Month - 1)
                                            .OrderByDescending(x => x.Date)
                                            .ToList();
                                    int lastMonthMilage = lastMonth.Where(
                                        x => x.Car_Id == car.Id && x.Car_Id == item.Car_Id)
                                        .Select(x => x.mileage)
                                        .FirstOrDefault();
                                    if (lastMonth.Count != 0 && lastMonthMilage != 0)
                                    {
                                        sumMiles += item.mileage - lastMonthMilage;
                                        gasamount += item.FuelAmount;
                                        totalFuelPrice += item.TotalPrice;
                                    }
                                }
                                else if (car.Id == item.Car_Id)
                                {
                                    var journalLastmonth = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Month == journal.Date.Month - 1 && x.FuelType.FuelType1 != "El").OrderByDescending(x => x.Date)
                                        .ToList();
                                    var mileageLastMonth = journalLastmonth.FirstOrDefault();
                                    if (mileageLastMonth != null)
                                    {
                                        sumMiles += item.mileage - mileageLastMonth.mileage;
                                        gasamount += item.FuelAmount;
                                        totalFuelPrice += item.TotalPrice;
                                    }
                                    else
                                    {
                                        int originalMilage =
                                        db.Car.Where(x => x.Id == item.Car_Id)
                                            .Select(x => x.OriginalMileage)
                                            .SingleOrDefault();

                                        sumMiles += item.mileage - originalMilage;
                                        gasamount += item.FuelAmount;
                                        totalFuelPrice += item.TotalPrice;
                                    }
                                }
                                lastjournal = item;
                            }
                        }
                        if (gasamount != 0 && sumMiles != 0)
                        {
                            var totalConsumation = gasamount / sumMiles;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        else
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetAllJournalsOnYearOnCarType")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetAllJournalsOnYearOnCarType([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var cars = db.Car.Where(x => x.CarType_Id == journal.CarTypeId).ToList();

                        var journals =
                            db.DriverJournal.Where(x => x.Date.Year == journal.Date.Year)
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        if (journals.Count == 0 || cars.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;

                        foreach (var car in cars)
                        {
                            foreach (var item in journals)
                            {
                                if (lastjournal.Car_Id == item.Car_Id && item.Car_Id == car.Id)
                                {
                                    sumMiles += item.mileage - lastjournal.mileage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                else if (lastjournal.Car_Id == car.Id && item.Car_Id == car.Id)
                                {
                                    var lastYear =
                                        db.DriverJournal.Where(x => x.Date.Year == journal.Date.Year - 1)
                                            .OrderByDescending(x => x.Date)
                                            .ToList();
                                    int lastYearMilage = lastYear.Where(
                                        x => x.Car_Id == car.Id && x.Car_Id == item.Car_Id)
                                        .Select(x => x.mileage)
                                        .FirstOrDefault();
                                    if (lastYear.Count != 0)
                                    {
                                        sumMiles += item.mileage - lastYearMilage;
                                        gasamount += item.FuelAmount;
                                        totalFuelPrice += item.TotalPrice;
                                    }
                                    else
                                    {
                                        int originalMilage =
                                            db.Car.Where(x => x.Id == item.Car_Id)
                                                .Select(x => x.OriginalMileage)
                                                .SingleOrDefault();

                                        sumMiles += item.mileage - originalMilage;
                                        gasamount += item.FuelAmount;
                                        totalFuelPrice += item.TotalPrice;
                                    }
                                }
                                else if (car.Id == item.Car_Id)
                                {
                                    var journalLastYear = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Year == journal.Date.Year - 1 && x.FuelType.FuelType1 != "El").OrderByDescending(x => x.Date)
                                        .ToList();
                                    var mileageLastYear = journalLastYear.FirstOrDefault();
                                    if (mileageLastYear != null)
                                    {
                                        sumMiles += item.mileage - mileageLastYear.mileage;
                                        gasamount += item.FuelAmount;
                                        totalFuelPrice += item.TotalPrice;
                                    }
                                    else
                                    {
                                        var originalMilage =
                                        db.Car.Where(x => x.Id == item.Car_Id)
                                            .Select(x => x.OriginalMileage)
                                            .SingleOrDefault();

                                        sumMiles += item.mileage - originalMilage;
                                        gasamount += item.FuelAmount;
                                        totalFuelPrice += item.TotalPrice;
                                    }
                                }
                                lastjournal = item;
                            }
                        }
                        if (gasamount != 0 && sumMiles != 0)
                        {
                            var totalConsumation = gasamount / sumMiles;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }

                        return Request.CreateResponse(HttpStatusCode.NoContent);
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }


        [HttpPost]
        [ActionName("GetAllJournalsAllTimeOnCarType")]
        [ResponseType(typeof(ConsumationModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetAllJournalsAllTimeOnCarType([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var cars = db.Car.Where(x => x.CarType_Id == journal.CarTypeId).ToList();

                        var journals =
                            db.DriverJournal.OrderBy(x => x.Car_Id).ThenBy(x => x.Date).ToList();

                        decimal gasamount = 0;
                        decimal totalFuelPrice = 0;

                        if (journals.Count == 0 || cars.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }
                        ConsumationModel consumationModel = new ConsumationModel();
                        var lastjournal = new DriverJournal();
                        var sumMiles = 0;

                        foreach (var car in cars)
                        {
                            foreach (var item in journals)
                            {
                                if (lastjournal.Car_Id == item.Car_Id && item.Car_Id == car.Id)
                                {
                                    sumMiles += item.mileage - lastjournal.mileage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                else if (lastjournal.Car_Id == car.Id && item.Car_Id == car.Id)
                                {
                                    sumMiles += item.mileage - lastjournal.mileage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }
                                else if (car.Id == item.Car_Id)
                                {
                                    int originalMilage =
                                        db.Car.Where(x => x.Id == item.Car_Id)
                                            .Select(x => x.OriginalMileage)
                                            .SingleOrDefault();

                                    sumMiles += item.mileage - originalMilage;
                                    gasamount += item.FuelAmount;
                                    totalFuelPrice += item.TotalPrice;
                                }

                                lastjournal = item;
                            }
                        }
                        if (gasamount != 0 && sumMiles != 0)
                        {
                            var totalConsumation = gasamount / sumMiles;
                            consumationModel.Consumation = totalConsumation;
                            consumationModel.Cost = totalFuelPrice;
                            return Request.CreateResponse(HttpStatusCode.OK, consumationModel);
                        }
                        return Request.CreateResponse(HttpStatusCode.NoContent);
                    }
                }
                catch (Exception)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("GetAllCostOnRegNr")]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetAllCostOnRegNr([FromBody] JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var costyear =
                            db.AdminJournal.Where(x => x.Car_Id == journal.CarId && x.Date.Year == journal.Date.Year)
                                .ToList();
                        var fuelcostyear =
                            db.DriverJournal.Where(x => x.Car_Id == journal.CarId && x.Date.Year == journal.Date.Year)
                                .ToList();
                        decimal totalcostyear = 0;
                        if (costyear.Count != 0)
                        {
                            foreach (var item in costyear)
                            {
                                totalcostyear += item.Cost;
                            }
                        }
                        if (fuelcostyear.Count != 0)
                        {
                            foreach (var item in fuelcostyear)
                            {
                                totalcostyear += item.TotalPrice;
                            }
                        }
                        var costmonth =
                            db.AdminJournal.Where(x => x.Car_Id == journal.CarId && x.Date.Month == journal.Date.Month)
                                .ToList();
                        var fuelcostmonth =
                            db.DriverJournal.Where(x => x.Car_Id == journal.CarId && x.Date.Month == journal.Date.Month)
                                .ToList();
                        decimal totalcostmonth = 0;
                        if (costmonth.Count != 0)
                        {
                            foreach (var item in costmonth)
                            {
                                totalcostmonth += item.Cost;
                            }
                        }
                        if (fuelcostmonth.Count != 0)
                        {
                            foreach (var item in fuelcostmonth)
                            {
                                totalcostmonth += item.TotalPrice;
                            }
                        }
                        TotalCostModel costModel = new TotalCostModel
                        {
                            CostYear = totalcostyear,
                            CostMonth = totalcostmonth
                        };
                        return Request.CreateResponse(HttpStatusCode.OK, costModel);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        #endregion

        [HttpGet]
        [ActionName("GetBestDriverLastMonth")]
        [ResponseType(typeof(BestValueModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetBestDriverLastMonth()
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var month = DateTime.Now.Month;
                        var drivers = db.User.ToList();

                        var journals =
                            db.DriverJournal.Where(x => x.Date.Month == month && x.FuelType.FuelType1 != "El")
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }

                        var lastjournal = new DriverJournal();

                        List<BestValueModel> bestvalueList = new List<BestValueModel>();

                        foreach (var driver in drivers)
                        {
                            decimal gasamount = 0;
                            var sumMiles = 0;
                            foreach (var item in journals)
                            {
                                if (journals.Count > 1)
                                {
                                    if (item.Driver_Id == driver.Id && lastjournal.Car_Id == item.Car_Id)
                                    {
                                        sumMiles += item.mileage - lastjournal.mileage;
                                        gasamount += item.FuelAmount;
                                    }
                                    else if (item.Driver_Id == driver.Id)
                                    {
                                        var journalLastmonth = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Month == month - 1).OrderByDescending(x => x.Date)
                                        .ToList();
                                        var mileageLastMonth = journalLastmonth.FirstOrDefault();
                                        if (mileageLastMonth != null)
                                        {
                                            sumMiles += item.mileage - mileageLastMonth.mileage;
                                            gasamount += item.FuelAmount;
                                        }
                                        else
                                        {
                                            int originalMilage =
                                            db.Car.Where(x => x.Id == item.Car_Id)
                                                .Select(x => x.OriginalMileage)
                                                .SingleOrDefault();

                                            sumMiles += item.mileage - originalMilage;
                                            gasamount += item.FuelAmount;
                                        }
                                    }
                                }
                                else
                                {
                                    if (item.Driver_Id == driver.Id)
                                    {
                                        var journalLastmonth = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Month == month - 1).OrderByDescending(x => x.Date).ToList();

                                        var mileageLastMonth = journalLastmonth.FirstOrDefault();
                                        if (mileageLastMonth != null)
                                        {
                                            sumMiles += item.mileage - mileageLastMonth.mileage;
                                            gasamount += item.FuelAmount;
                                        }
                                        else
                                        {
                                            int originalMilage =
                                            db.Car.Where(x => x.Id == item.Car_Id)
                                                .Select(x => x.OriginalMileage)
                                                .SingleOrDefault();

                                            sumMiles += item.mileage - originalMilage;
                                            gasamount += item.FuelAmount;
                                        }
                                    }
                                }

                                lastjournal = item;
                            }
                            if (gasamount != 0 && sumMiles != 0)
                            {
                                BestValueModel driverToAdd = new BestValueModel
                                {
                                    Username = driver.UserName,
                                    Value = gasamount / sumMiles
                                };
                                bestvalueList.Add(driverToAdd);
                            }
                        }
                        if (bestvalueList.Count != 0)
                        {
                            var bestdriver = bestvalueList.OrderBy(x => x.Value).FirstOrDefault();
                            return Request.CreateResponse(HttpStatusCode.OK, bestdriver);
                        }
                        return Request.CreateResponse(HttpStatusCode.NoContent);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpGet]
        [ActionName("GetBestCarLastMonth")]
        [ResponseType(typeof(BestValueModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage GetBestCarLastMonth()
        {
            if (ClaimsPrincipal.Current.IsInRole("Admin"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var month = DateTime.Now.Month;
                        var cars = db.Car.ToList();

                        var journals =
                            db.DriverJournal.Where(x => x.Date.Month == month && x.FuelType.FuelType1 != "El")
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }

                        var lastjournal = new DriverJournal();

                        List<BestValueModel> bestvalueList = new List<BestValueModel>();

                        foreach (var car in cars)
                        {
                            decimal gasamount = 0;
                            var sumMiles = 0;
                            foreach (var item in journals)
                            {
                                if (item.Car_Id == car.Id && lastjournal.Car_Id == item.Car_Id)
                                {
                                    sumMiles += item.mileage - lastjournal.mileage;
                                    gasamount += item.FuelAmount;
                                }
                                else if (item.Car_Id == car.Id)
                                {
                                    var journalLastmonth = db.DriverJournal.Where(x => x.Car_Id == item.Car_Id &&
                                        x.Date.Month == month - 1 && x.FuelType.FuelType1 != "El").OrderByDescending(x => x.Date)
                                        .ToList();
                                    var mileageLastMonth = journalLastmonth.FirstOrDefault();
                                    if (mileageLastMonth != null)
                                    {
                                        sumMiles += item.mileage - mileageLastMonth.mileage;
                                        gasamount += item.FuelAmount;
                                    }
                                    else
                                    {
                                        int originalMilage =
                                        db.Car.Where(x => x.Id == item.Car_Id)
                                            .Select(x => x.OriginalMileage)
                                            .SingleOrDefault();

                                        sumMiles += item.mileage - originalMilage;
                                        gasamount += item.FuelAmount;
                                    }
                                }
                                lastjournal = item;
                            }
                            if (gasamount != 0 || sumMiles != 0)
                            {
                                BestValueModel carToAdd = new BestValueModel
                                {
                                    Regnr = car.Regnr,
                                    Value = gasamount / sumMiles
                                };
                                bestvalueList.Add(carToAdd);
                            }
                        }
                        if (bestvalueList.Count != 0)
                        {
                            var bestCar = bestvalueList.OrderBy(x => x.Value).FirstOrDefault();
                            return Request.CreateResponse(HttpStatusCode.OK, bestCar);
                        }
                        return Request.CreateResponse(HttpStatusCode.NoContent);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpPost]
        [ActionName("CreateJournal")]
        [Authorize(Roles = "User")]
        public HttpResponseMessage CreateJournal([FromBody]JournalModel journal)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        if (!db.DriverJournal.Any(x => x.mileage == journal.MileAge && x.FuelType_Id == journal.FuelTypeId
                                     && x.Car_Id == journal.CarId))
                        {
                            DriverJournal newJournal = new DriverJournal
                            {
                                Car_Id = journal.CarId,
                                Date = journal.Date,
                                Driver_Id = journal.DriverId,
                                FuelAmount = journal.FuelAmount,
                                mileage = journal.MileAge,
                                FuelType_Id = journal.FuelTypeId,
                                PricePerUnit = journal.PricePerUnit,
                                TotalPrice = journal.TotalPrice
                            };
                            db.DriverJournal.Add(newJournal);
                            db.SaveChanges();
                            return Request.CreateResponse(HttpStatusCode.OK);
                        }
                        return Request.CreateResponse(HttpStatusCode.NotModified);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }
    }
}