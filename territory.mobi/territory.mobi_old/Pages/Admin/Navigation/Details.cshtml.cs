﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using territory.mobi.Models;

namespace territory.mobi.Pages.Admin.Congregation.Navigation
{
    public class DetailsModel : PageModel
    {
        private readonly territory.mobi.Models.TerritoryContext _context;

        public DetailsModel(territory.mobi.Models.TerritoryContext context)
        {
            _context = context;
        }

        public tmPage tmPage { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            tmPage = await _context.tmPage.FirstOrDefaultAsync(m => m.PageId == id);

            if (tmPage == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}