﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using territory.mobi.Models;

namespace territory.mobi.Pages.Admin.Congregation.Maps.Image
{
    public class IndexModel : PageModel
    {
        private readonly territory.mobi.Models.TerritoryContext _context;


        public IndexModel(territory.mobi.Models.TerritoryContext context)
        {
            _context = context;
        }

        public IList<Images> Images { get; set; }

        public Images CurrentImage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid ?mapid)
        {

            Map mp = await _context.Map.FirstOrDefaultAsync(m => m.MapId == mapid);
            CurrentImage = await _context.Images.FirstOrDefaultAsync(m => m.ImgId == mp.ImgId);
            Images = await _context.Images.Where(i => i.MapId == mapid && i.ImgId != mp.ImgId).ToListAsync();
            ViewData["MapNo"] = mp.MapDesc;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid ImgId)
        {
            Images CurrentImage = await _context.Images.FirstOrDefaultAsync(m => m.ImgId == ImgId);
            Map Map = await _context.Map.FirstOrDefaultAsync(m => m.MapId == CurrentImage.MapId);
            Map.ImgId = ImgId;
            CurrentImage.Updatedatetime = DateTime.UtcNow;
            _context.Attach(CurrentImage).State = EntityState.Modified;
            _context.Attach(Map).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return await OnGetAsync(CurrentImage.MapId);
        }
    }           
}
