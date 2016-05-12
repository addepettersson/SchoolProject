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
    public class CarController : ApiController
    {

        [HttpGet]
        [ActionName("GetColors")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(List<ColorModel>))]
        public HttpResponseMessage GetColors()
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var colors = db.Colour.ToList();
                        List<ColorModel> colourlist = new List<ColorModel>();
                        foreach (var item in colors)
                        {
                            ColorModel color = new ColorModel
                            {
                                Id = item.Id,
                                Color = item.Colour1
                            };
                            colourlist.Add(color);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, colourlist);
                    }
                }
                catch (Exception e)
                {
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
                }
            }
            return Request.CreateResponse(HttpStatusCode.MethodNotAllowed);
        }

        [HttpGet]
        [ActionName("GetCarType")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(List<CarType>))]
        public HttpResponseMessage GetCarType()
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var carTypes = db.CarType.ToList();
                        List<CarType> carTypeList = new List<CarType>();
                        foreach (var item in carTypes)
                        {
                            CarType color = new CarType
                            {
                                Id = item.Id,
                                Type = item.Type
                            };
                            carTypeList.Add(color);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, carTypeList);
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
        [ActionName("GetYearToShow")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(List<YearModel>))]
        public HttpResponseMessage GetYear()
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var years = db.Year.ToList();
                        List<YearModel> yearList = new List<YearModel>();
                        foreach (var item in years)
                        {
                            YearModel color = new YearModel
                            {
                                Id = item.Id,
                                Year = item.Year1
                            };
                            yearList.Add(color);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, yearList);
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
        [ActionName("GetFuelTypeToShow")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(List<FuelType>))]
        public HttpResponseMessage GetFuelTypes()
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var fuelTypes = db.FuelType.ToList();
                        List<FuelTypeModel> fuelTypeList = new List<FuelTypeModel>();
                        foreach (var item in fuelTypes)
                        {
                            FuelTypeModel color = new FuelTypeModel
                            {
                                Id = item.Id,
                                FuelType = item.FuelType1
                            };
                            fuelTypeList.Add(color);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, fuelTypeList);
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
        [ActionName("GetCars")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(List<CarModel>))]
        public HttpResponseMessage GetCars()
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var cars = db.Car.ToList();
                        List<CarModel> carList = new List<CarModel>();
                        foreach (var item in cars)
                        {
                            CarModel car = new CarModel
                            {
                                Regnr = item.Regnr,
                                Id = item.Id,
                                FuelType_Id = item.FuelType_Id
                            };
                            carList.Add(car);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, carList);
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
        [ActionName("GetCostType")]
        [Authorize(Roles = "User")]
        [ResponseType(typeof(List<CostTypeModel>))]
        public HttpResponseMessage GetTypeOfCost()
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        var typesOfCost = db.TypeOfCost.ToList();
                        List<CostTypeModel> typeOfCostList = new List<CostTypeModel>();
                        foreach (var item in typesOfCost)
                        {
                            CostTypeModel car = new CostTypeModel
                            {
                                Type = item.Type,
                                Id = item.Id
                            };
                            typeOfCostList.Add(car);
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, typeOfCostList);
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
        [ActionName("CreateNewCar")]
        [Authorize(Roles = "User")]
        public HttpResponseMessage CreateNewCar([FromBody]CarModel car)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        if (db.Car.Any(x => x.Regnr == car.Regnr))
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }

                        if (string.IsNullOrWhiteSpace(car.Description))
                        {
                            car.Description = null;
                        }
                        Car newCar = new Car
                        {
                            Regnr = car.Regnr,
                            CarType_Id = car.CarType_Id,
                            Colour_Id = car.Colour_Id,
                            FuelType_Id = car.FuelType_Id,
                            OriginalMileage = car.OriginalMileage,
                            Year_Id = car.Year_Id,
                            Description = car.Description
                        };
                        db.Car.Add(newCar);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
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
        [ActionName("CreateNewCost")]
        [Authorize(Roles = "User")]
        public HttpResponseMessage CreateNewCost([FromBody]CostModel cost)
        {
            if (ClaimsPrincipal.Current.IsInRole("User"))
            {
                try
                {
                    using (var db = new WpfprojectEntities())
                    {
                        if (db.AdminJournal.Any(x => x.Car_Id == cost.CarId && x.TypeOfCost_Id == cost.TypeOfCost && x.Date == cost.Datepicker))
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound);
                        }
                        if (string.IsNullOrWhiteSpace(cost.Comment))
                        {
                            cost.Comment = null;
                        }
                        var costToAdd = new AdminJournal
                        {
                            Car_Id = cost.CarId,
                            Comment = cost.Comment,
                            Cost = cost.Cost,
                            Date = cost.Datepicker,
                            TypeOfCost_Id = cost.TypeOfCost

                        };
                        db.AdminJournal.Add(costToAdd);
                        db.SaveChanges();
                        return Request.CreateResponse(HttpStatusCode.OK);
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
