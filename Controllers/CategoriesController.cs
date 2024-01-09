using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services;
using TheBlogProject.Services.Interfaces;
using X.PagedList;

namespace TheBlogProject.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IImageService _imageService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<BTUser> _userManager;

        public CategoriesController(ApplicationDbContext context, IImageService imageService,
            ISlugService slugService, UserManager<BTUser> userManager, ICategoryService categoryService)
        {
            _context = context;
            _imageService = imageService;
            _slugService = slugService;
            _userManager = userManager;
            _categoryService = categoryService;
        }

        #region // GET: Categories
        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _categoryService.GetAllCategoriesAsync() );
                  
        }
        #endregion

        #region  // GET: Categories/Details/5
        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if ( id == null )
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        #endregion

        #region // GET: Categories/Create
        // GET: Categories/Create
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }
        #endregion

        #region // POST: Categories/Create
        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("title,Image")] Category category)
        {
            if (ModelState.IsValid)
            {
                var slug = _slugService.UrlFriendly(category.title);

                // Use the _imageService to store user image
                category.ImageData = await _imageService.EncodeImageAsync(category.Image);
                category.ImageType = _imageService.ContentType(category.Image);

                category.CategoryUserId =  _userManager.GetUserId(User);

                var validationError = false;

                if (string.IsNullOrEmpty(slug))
                {
                    ModelState.AddModelError("title", "The Title you provided cannot be used as it results in an empty slug.");
                    validationError = true;
                }

                if (!_slugService.is_unique_category_slug(slug))
                {
                    ModelState.AddModelError("Title", "The Title you provided cannot be used as it must be unique.");
                    validationError = true;
                }

                if (validationError)
                {
                    // return the user back to the Create view
                    return View(category);
                }

                category.slug = slug;

                _context.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
        #endregion

        #region // GET: Categories/Edit/5
        // GET: Categories/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if ( id == null )
            {
                return NotFound();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id.Value);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        #endregion

        #region // POST: Categories/Edit/5
        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,title")] Category category, IFormFile newImage)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Category newCategory = await _categoryService.GetCategoryByIdAsync(category.Id);

                    newCategory.title = category.title;

                    var newSlug = _slugService.UrlFriendly(category.title);

                    if(newSlug != category.slug)
                    {
                        if(  _slugService.is_unique_category_slug(newSlug) )
                        {
                            newCategory.title = category.title;
                            newCategory.slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "This Title cannot be used as it results in a duplicate slug");
                            return View(category);
                        }

                    }

                    if (newImage is not null)
                    {
                        newCategory.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newCategory.ImageType = _imageService.ContentType(newImage);
                    }

                    _context.Update(newCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }
        #endregion

        #region  // GET: Categories/Delete/5
        // GET: Categories/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Categories == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
        #endregion

        #region // POST: Categories/Delete/5
        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Categories == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Categories'  is null.");
            }
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region CategoryExists
        private bool CategoryExists(int id)
        {
            return (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        #endregion

        #region  // GET: Category/1
        // GET: PostsByCategory/1
        public async Task<IActionResult> Category(int? id, int? page)
        {

            if (id == null)
            {
                return NotFound();
            }

            var pageNumber = page ?? 1;
            var pageSize = 3;

            // var posts = await _categoryService.GetPostsByCategory(id.Value);

            var posts =  _context.Posts
                 .Include(p => p.Category)
                 .Where(p => p.CategoryId == id).ToPagedListAsync(pageNumber, pageSize);

            if (posts == null)
            {
                return NotFound();
            }

            return View( await posts);
        }
        #endregion

    }
}
