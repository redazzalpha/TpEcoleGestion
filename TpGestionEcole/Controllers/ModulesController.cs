using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TpGestionEcole.Models;

namespace TpGestionEcole.Controllers
{

    [Authorize(Roles = "Admin")]
    public class ModulesController : Controller
    {
        // class members
        private IWebHostEnvironment _webHostEnvironment;
        private readonly EcoleDbEntities _context;

        // constructors
        public ModulesController(EcoleDbEntities context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // controller actions
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return _context.Modules != null ?
                        View(await _context.Modules.ToListAsync()) :
                        Problem("Entity set 'EcoleDbEntities.Modules'  is null.");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Modules == null)
            {
                return NotFound();
            }

            var @module = await _context.Modules
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@module == null)
            {
                return NotFound();
            }

            return View(@module);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile logo, Module module)
        {
            if (ModelState.IsValid)
            {
                await InsertImg(logo, module);
                _context.Add(module);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(module);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Modules == null)
            {
                return NotFound();
            }

            var module = await _context.Modules.FindAsync(id);
            if (module == null)
            {
                return NotFound();
            }
            return View(module);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? logo, Module module)
        {
            if (id != module.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string? logoStr = (from m in _context.Modules where m.Id == id select m.logo).FirstOrDefault();
                    int? moduleParcourId = (from m in _context.Modules where m.Id == id select m.ParcoursId).FirstOrDefault();

                    module.ParcoursId = moduleParcourId;
                    await InsertImg(logo, module, logoStr);
                    _context.Update(module);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModuleExists(module.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(module);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Modules == null)
            {
                return NotFound();
            }

            var module = await _context.Modules
                .FirstOrDefaultAsync(m => m.Id == id);
            if (module == null)
            {
                return NotFound();
            }

            return View(module);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Modules == null)
            {
                return Problem("Entity set 'EcoleDbEntities.Modules'  is null.");
            }
            var @module = await _context.Modules.FindAsync(id);
            if (@module != null)
            {
                _context.Modules.Remove(@module);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // methods
        private bool ModuleExists(int id)
        {
            return (_context.Modules?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private async Task InsertImg(IFormFile? logo, Module module, string? logoStr = null)
        {

            if (logo != null)
            {
                string rootPath = _webHostEnvironment.WebRootPath;
                string fileName = Path.GetFileNameWithoutExtension(logo.FileName) + "_" +
                           Guid.NewGuid() +
                           Path.GetExtension(logo.FileName);
                string path = Path.Combine(rootPath + "/assets/", fileName);
                var fileStream = new FileStream(path, FileMode.Create);
                await logo.CopyToAsync(fileStream);
                fileStream.Close();
                module.logo = "https://localhost:7278/assets/" + fileName;
            }
            else if (logoStr != null) module.logo = logoStr;
            else module.logo = "https://localhost:7278/assets/no-image.png";
        }

    }
}
