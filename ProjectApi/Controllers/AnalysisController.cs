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
    public class AnalysisController : ApiController
    {
        [HttpGet]
        [ActionName("FillDataGrid")]
        [ResponseType(typeof(BestValueModel))]
        [Authorize(Roles = "Admin")]
        public HttpResponseMessage FillDataGrid()
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
                            db.DriverJournal.Where(x => x.Date.Month == month - 1)
                                .OrderBy(x => x.Car_Id)
                                .ThenBy(x => x.Date)
                                .ToList();

                        if (journals.Count == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NoContent);
                        }

                        var lastjournal = new DriverJournal();

                        List<DatagridModel> datagridList = new List<DatagridModel>();

                        foreach (var car in cars)
                        {
                            decimal gasamount = 0;
                            var sumMiles = 0;
                            decimal fuelPrice = 0;
                            var fuelType = car.FuelType.FuelType1;
                            var cartype = db.CarType.SingleOrDefault(x => x.Id == car.CarType_Id);
                            foreach (var item in journals)
                            {
                                if (item.Car_Id == car.Id && lastjournal.Car_Id == item.Car_Id)
                                {
                                    sumMiles += item.mileage - lastjournal.mileage;
                                    gasamount += item.FuelAmount;
                                    fuelPrice += item.TotalPrice;
                                }
                                else if (item.Car_Id == car.Id)
                                {
                                    int originalMilage =
                                        db.Car.Where(x => x.Id == item.Car_Id)
                                            .Select(x => x.OriginalMileage)
                                            .SingleOrDefault();

                                    sumMiles += item.mileage - originalMilage;
                                    gasamount += item.FuelAmount;
                                    fuelPrice += item.TotalPrice;
                                }
                                lastjournal = item;
                            }
                            if (gasamount != 0 || sumMiles != 0)
                            {
                                DatagridModel carToAdd = new DatagridModel
                                {
                                    Regnr = car.Regnr,
                                    Consumation = gasamount / sumMiles,
                                    CarId = car.Id,
                                    TypeOfCar = cartype.Type,
                                    Mileage = sumMiles,
                                    TotalFuelPrice = fuelPrice,
                                    FuelType = fuelType
                                };
                                datagridList.Add(carToAdd);
                            }
                        }
                        var orderedList = datagridList.OrderBy(x => x.Consumation);
                        return Request.CreateResponse(HttpStatusCode.OK, orderedList);
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
