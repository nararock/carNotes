﻿using CarNotes.CnDb;
using CarNotes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Web;
using Microsoft.AspNet.Identity;

namespace CarNotes.Classes
{
    public class RepairHelper
    {
        public List<RepairModel> GetList(int vehicleId)
        {
            var db = new CnDbContext();
            var vehicle = db.Vehicles.Include(v=>v.RepairEvents.Select(r=>r.Parts)).FirstOrDefault(x => x.Id == vehicleId);
            if (vehicle == null) return null;
            var list = vehicle.RepairEvents.Select(x => new RepairModel {Id = x.Id, Date=x.Date.ToString(), Mileage=x.Mileage, Repair=x.Repair,
                CarService=x.CarService, RepairCost=x.RepairCost, Comments=x.Comments, Parts=x.Parts.Select(y=>new CarPartModel { Article=y.Article,
                    CarManufacturer=y.CarManufacturer, Name=y.Name, Price=y.Price}).ToList() }).ToList();
            return list;
        }
        public void SaveToDataBase(RepairModel rm, int vehicleId)
        {
            var database = new CnDbContext();
            var repairEvent = new RepairEvent();
            repairEvent.CarService = rm.CarService;
            repairEvent.Comments = rm.Comments;
            var tempDate = new DateTime();
            DateTime.TryParse(rm.Date, out tempDate);
            repairEvent.Date = tempDate;
            repairEvent.Mileage = rm.Mileage;
            repairEvent.Repair = rm.Repair;
            repairEvent.RepairCost = rm.RepairCost;
            repairEvent.VehicleId = vehicleId;
            repairEvent.Parts = new List<CarPart>();
            for (int i = 0; i < rm.Parts.Count; i++)
            {
                var carPart = new CarPart();
                carPart.Article = rm.Parts[i].Article;
                carPart.CarManufacturer = rm.Parts[i].CarManufacturer;
                carPart.Name = rm.Parts[i].Name;
                carPart.Price = rm.Parts[i].Price;
                carPart.CarSubsystemId = rm.Parts[i].CarSubsystem;
                repairEvent.Parts.Add(carPart);
            }
            database.RepairEvents.Add(repairEvent);
            database.SaveChanges();
        }

        public void Delete(int id, HttpContextBase hc)
        {
            var data = new CnDbContext();
            var repair = data.RepairEvents.Include(x=>x.Parts).FirstOrDefault(x => x.Id == id);
            if (repair?.Vehicle?.UserId == hc.User.Identity.GetUserId())
            {
                var parts = repair.Parts;
                for(int i = 0; i < parts.Count;)
                {
                    parts.RemoveAt(i);
                }
                data.RepairEvents.Remove(repair);
                data.SaveChanges();
            }
        }

        public RepairModel GetDataEdit(int id)
        {
            var db = new CnDbContext();
            var editRepair = db.RepairEvents
                .Include(x => x.Parts)
                .Include(x => x.Parts.Select(p => p.CarSubsystem))
                .FirstOrDefault(y => y.Id == id);
            if (editRepair == null)
            {
                return new RepairModel();
            }
            var editRepairModel = new RepairModel();            
            editRepairModel.Id = editRepair.Id;
            editRepairModel.Mileage = editRepair.Mileage;
            editRepairModel.Repair = editRepair.Repair;
            editRepairModel.RepairCost = editRepair.RepairCost;
            editRepairModel.Date = editRepair.Date.ToString("dd.MM.yyyy");
            editRepairModel.CarService = editRepair.CarService;
            editRepairModel.Comments = editRepair.Comments;
            editRepairModel.Parts = new List<CarPartModel>();
            for (int i = 0; i < editRepair.Parts.Count; i++)
            {
                var editCarPartModel = new CarPartModel();
                editCarPartModel.CarSubsystemModel = new CarSubsystemModel();
                editCarPartModel.Id = editRepair.Parts[i].Id;
                editCarPartModel.Article = editRepair.Parts[i].Article;
                editCarPartModel.CarManufacturer = editRepair.Parts[i].CarManufacturer;
                editCarPartModel.Name = editRepair.Parts[i].Name;
                editCarPartModel.Price = editRepair.Parts[i].Price;
                editCarPartModel.CarSubsystemModel.Id = editRepair.Parts[i].CarSubsystem.Id;
                editCarPartModel.CarSubsystemModel.Name = editRepair.Parts[i].CarSubsystem.Name;
                editCarPartModel.CarSubsystemModel.CarSubsystemId = editRepair.Parts[i].CarSubsystem.CarsystemId;
                editCarPartModel.CarSubsystem = editRepair.Parts[i].CarSubsystemId;
                editRepairModel.Parts.Add(editCarPartModel);
            }
            return editRepairModel;
        }

        public void ChangeData(RepairModel rm)
        {
            var db = new CnDbContext();
            var repairEvent = db.RepairEvents.Include(x => x.Parts).Where(y => y.Id == rm.Id).FirstOrDefault();
            repairEvent.CarService = rm.CarService;
            repairEvent.Comments = rm.Comments;
            var tempDate = new DateTime();
            DateTime.TryParse(rm.Date, out tempDate);
            repairEvent.Date = tempDate;
            repairEvent.Mileage = rm.Mileage;
            repairEvent.Repair = rm.Repair;
            repairEvent.RepairCost = rm.RepairCost;
            var carPartsDelete = new List<CarPart>();
            foreach(var p in rm.Parts)
            {
                if (p.Id != 0 && p.IsDeleted == true)
                {
                    repairEvent.Parts.Remove(repairEvent.Parts.Where(x => x.Id == p.Id).FirstOrDefault());
                } 
                else if (p.Id != 0)
                {
                    var carPartDb = repairEvent.Parts.Where(x => x.Id == p.Id).FirstOrDefault();
                    carPartDb.Article = p.Article;
                    carPartDb.CarManufacturer = p.CarManufacturer;
                    carPartDb.Name = p.Name;
                    carPartDb.Price = p.Price;
                } 
                else if (p.Id == 0 && p.IsDeleted != true)
                {
                    var carParts = new CarPart();
                    carParts.Id = -1;
                    carParts.Name = p.Name;
                    carParts.Price = p.Price;
                    carParts.Article = p.Article;
                    carParts.CarManufacturer = p.CarManufacturer;
                    repairEvent.Parts.Add(carParts);
                }
            }
            db.SaveChanges();
        }
        
       public List<CarSystemModel> GetSystemList()
        {
            var db = new CnDbContext();
            var listSystem = db.CarSystems.Include(c => c.Subsystems).ToList()
                .Select(cs => new CarSystemModel
                {
                    Name = cs.Name,
                    Id = cs.Id,
                    CarSubsystems = cs.Subsystems.Select(ss => new CarSubsystemModel { Name = ss.Name, Id = ss.Id }).ToList()
                }).ToList();
            return listSystem;
        }
    }
}