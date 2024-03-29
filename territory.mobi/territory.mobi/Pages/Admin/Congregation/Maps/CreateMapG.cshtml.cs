﻿using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using territory.mobi.Models;
using static System.Net.WebRequestMethods;
using System.Security.Cryptography;
using System.Xml;
using Newtonsoft.Json;
using System.Diagnostics;

namespace territory.mobi.Pages
{


    public class CreateMapgModel : PageModel
    {
        private readonly territory.mobi.Models.TerritoryContext _context;

        public CreateMapgModel(territory.mobi.Models.TerritoryContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Map Map { get; set; }
        public string GoogleKey { get; set; }
        public IList<Setting> Setting { get; set; }
        public IList<MapFeature> MapPolygons { get; set; }
        public IList<MapFeature> MapMarkers { get; set; }
        public string MapCentreLat { get; set; } = "";
        public string MapCentreLng { get; set; } = "";

        public int MapZoom { get; set; } = 16;

        public Images CurrentImage { get; set; }


        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Setting set  = await _context.Setting.Where(s => s.SettingType == "GoogleAPIKey").FirstOrDefaultAsync();
            GoogleKey = set.SettingValue;

            Map = await _context.Map.Where(m => m.MapId == id).FirstOrDefaultAsync();

            CurrentImage = await _context.Images.FirstOrDefaultAsync(m => m.ImgId == Map.ImgId);
            if (CurrentImage == null)
            {
                CurrentImage = new Images();
                ViewData["catme"] = CurrentImage.Cat;
            }

            MapPolygons = await _context.MapFeature.Where(m => m.MapId == id && m.Type =="Polygon").ToListAsync();
            MapMarkers = await _context.MapFeature.Where(m => m.MapId == id && m.Type == "Marker").ToListAsync();
            MapFeature MapCentre = await _context.MapFeature.Where(m => m.MapId == id && m.Type == "Centre").FirstOrDefaultAsync();
            
            if (MapCentre != null) {
                dynamic Coords = JsonConvert.DeserializeObject(MapCentre.Position);
                MapCentreLat = Coords.lat;
                MapCentreLng = Coords.lng;
                MapZoom = MapCentre.Zoom;
            }


            return Page();
        }

        public IActionResult OnPost()
        {
            return new BadRequestResult();
        }

        public async Task<IActionResult> OnPostMapCentre(Guid id, string position, int zoom)
        {
            return await AddUpdateFeature(id, "Centre", position,"",0.5m,0,"",zoom);
        }

        public async Task<IActionResult> OnPostPolygon(Guid id, string position, string color, decimal opacity, int zIndex)
        {
            return await AddUpdateFeature(id, "Polygon", position, color, opacity, zIndex);
        }

        public async Task<IActionResult> OnPostLabel(Guid id, string position, int zIndex, string title)
        {
            return await AddUpdateFeature(id, "Marker", position,"" ,0.5m ,zIndex,title);
        }


        public async Task<IActionResult> OnPostDeletePolygon(Guid id, int zIndex)
        {
            return await DeleteFeature(id, "Polygon", zIndex);
        }

        public async Task<IActionResult> OnPostDeleteLabel(Guid id, int zIndex)
        {
            return await DeleteFeature(id, "Marker",zIndex);
        }



        public async Task<IActionResult> DeleteFeature(Guid id, string type,int zIndex)
        {
            MapFeature MF = await _context.MapFeature.Where(m => m.MapId == id && m.Type == type && m.ZIndex == zIndex).FirstOrDefaultAsync();
            if (MF == null)
            {
                return new OkResult();
            }
            else
            {
                _context.MapFeature.Remove(MF);
                try
                {
                    await _context.SaveChangesAsync();
                    return new OkResult();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.InnerException);
                    return new BadRequestResult();
                }
            }
        }

        private async Task<IActionResult> AddUpdateFeature(Guid id, string type, string position, string color = "", decimal opacity = 0.5m, int zIndex = 0, string title = "", int zoom = 16)
        {

            MapFeature MF = await _context.MapFeature.Where(m => m.MapId == id && m.Type == type && m.ZIndex == zIndex).FirstOrDefaultAsync();
            if (MF == null)
            {
                MapFeature newMF = new MapFeature
                {
                    MapFeatureId = Guid.NewGuid(),
                    MapId = id,
                    Type = type,
                    Position = position,
                    Color = color,
                    Opacity = opacity,
                    Title = title,
                    Updatedatetime = DateTime.UtcNow,
                    ZIndex = zIndex,
                    Zoom = zoom
                };
                _context.MapFeature.Add(newMF);
                
            }
            else
            {
                MF.Position = position;
                MF.Color = color;
                MF.Opacity = opacity;
                MF.Title = title;
                MF.Updatedatetime = DateTime.UtcNow;
                MF.Zoom = zoom;
                _context.Attach(MF).State = EntityState.Modified;

            }
            try
            {

                await _context.SaveChangesAsync();
                return new OkResult();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException);
                return new BadRequestResult();
            }
        }

        private bool MapExists(Guid id)
        {
            return _context.Map.Any(e => e.MapId == id);
        }
    }
}
