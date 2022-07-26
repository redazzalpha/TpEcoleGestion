using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using TpGestionEcole.Models;

namespace TpGestionEcole.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ParcoursController : Controller
    {
        // class members
        private readonly EcoleDbEntities _context;
        private IWebHostEnvironment _webHostEnvironment;

        // constructors
        public ParcoursController(EcoleDbEntities context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // controller actions
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return _context.Parcours != null ?
                        View(await _context.Parcours.ToListAsync()) :
                        Problem("Entity set 'EcoleDbEntities.Parcours'  is null.");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Parcours == null)
            {
                return NotFound();
            }

            var parcours = await _context.Parcours
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parcours == null)
            {
                return NotFound();
            }

            IQueryable<Module> modules = from m in _context.Modules where m.ParcoursId == id select m;
            ViewBag.modules = modules.ToList();

            return View(parcours);
        }

        public IActionResult Create()
        {
            IQueryable<Module> moduleList = from m in _context.Modules select m;
            ViewBag.modules = moduleList.ToList();
            return View("Create");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IFormFile? logo, Parcours parcours)
        {
            if (ModelState.IsValid)
            {
                await InsertImg(logo, parcours);
                AddSelectedModule(parcours);
                _context.Add(parcours);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(parcours);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Parcours == null)
            {
                return NotFound();
            }

            var parcours = await _context.Parcours.FindAsync(id);
            if (parcours == null)
            {
                return NotFound();
            }

            IQueryable<Module> moduleList = from m in _context.Modules select m;
            ViewBag.modules = moduleList.ToList();

            return View(parcours);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile? logo, Parcours parcours)
        {
            if (id != parcours.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {
                try
                {
                    string? logoStr = (from p in _context.Parcours where p.Id == id select p.logo).FirstOrDefault();

                    await InsertImg(logo, parcours, logoStr);
                    AddSelectedModule(parcours);
                    _context.Update(parcours);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ParcoursExists(parcours.Id))
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
            return View(parcours);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Parcours == null)
            {
                return NotFound();
            }

            var parcours = await _context.Parcours
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parcours == null)
            {
                return NotFound();
            }

            return View(parcours);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Parcours == null)
            {
                return Problem("Entity set 'EcoleDbEntities.Parcours'  is null.");
            }
            
            var parcours = await _context.Parcours.FindAsync(id);
            if (parcours != null)
            {
                Module? module = (from m in _context.Modules where m.ParcoursId == parcours.Id select m).FirstOrDefault();
                if (module != null)
                {
                    module.ParcoursId = null;
                    _context.Update(module);
                }
                _context.Parcours.Remove(parcours);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Lier(int IdParcours, Module module)
        {
            var p = _context.Parcours.FirstOrDefault(x => x.Id == IdParcours);
            p.Modules.Add(module);

            _context.SaveChanges();
            return View("Parcours/Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delier(int id)
        {
            Module? module = (from m in _context.Modules where m.ParcoursId == id select m).FirstOrDefault();
            if(module != null)
            {
                module.ParcoursId = null;
                _context.Update(module);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", "Parcours", new { id });
        }

        // Methods
        private bool ParcoursExists(int id)
        {
            return (_context.Parcours?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private async Task InsertImg(IFormFile? logo, Parcours parcours, string? logoStr = null)
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
                parcours.logo = "https://localhost:7278/assets/" + fileName;
            }
            else if(logoStr != null) parcours.logo = logoStr;
            else parcours.logo = "https://localhost:7278/assets/no-image.png";
        }
        private void AddSelectedModule(Parcours parcours)
        {
            int moduleId;
            StringValues moduleIdStr;
            bool isModuleId;

            isModuleId = Request.Form.TryGetValue("moduleId", out moduleIdStr);
            if (isModuleId && int.TryParse(moduleIdStr, out moduleId))
            {
                IQueryable<Module> modules = from m in _context.Modules where m.Id == moduleId select m;
                parcours.Modules.Add(modules.ToList()[0]);
            }
        }
    }
}
