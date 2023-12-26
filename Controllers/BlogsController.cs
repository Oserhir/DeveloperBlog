using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services.Interfaces;

namespace TheBlogProject.Controllers
{
    public class BlogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlogService _blogService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IImageService _imageService;

        #region Constructor
        public BlogsController(ApplicationDbContext context, IBlogService blogService, UserManager<BTUser> userManager, IImageService imageService)
        {
            _context = context;
            _blogService = blogService;
            _userManager = userManager;
            _imageService = imageService;
        }
        #endregion

        #region // GET: Blogs
        // GET: Blogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Blogs;
            return View(applicationDbContext); // return View(await applicationDbContext.ToListAsync());
        }

        #endregion

        #region  // GET: Blogs/Details/5
        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            //var blog = await _context.Blogs
            //    .Include(b => b.BlogUser)
            //    .FirstOrDefaultAsync(m => m.Id == id);

            Blog blog = await _blogService.GetBlogByIdAsync(id.Value);


            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
        #endregion

        [Authorize]
        #region // GET: Blogs/Create
        // GET: Blogs/Create
        public IActionResult Create()
        {
            return View();
        }
        #endregion

        #region // POST: Blogs/Create
        // POST: Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Image")] Blog blog)
        {
            if (ModelState.IsValid)
            {
                blog.Created = DateTime.UtcNow;
                blog.BlogUserId = _userManager.GetUserId(User);

                blog.ImageData = await _imageService.EncodeImageAsync(blog.Image);
                blog.ImageType = _imageService.ContentType(blog.Image);
               
                //...

                await _blogService.AddNewBlogAsync(blog);
                return RedirectToAction(nameof(Index));
            }

            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogUserId);
            return View(blog);
        }
        #endregion

        [Authorize]
        #region // GET: Blogs/Edit/5
        // GET: Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            Blog blog = await _blogService.GetBlogByIdAsync(id.Value);


            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
        #endregion

        #region // POST: Blogs/Edit/5
        // POST: Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Blog blog, IFormFile newImage)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Blog newblog = await _blogService.GetBlogByIdAsync(blog.Id);

                    newblog.Updated = DateTime.UtcNow;

                    if (newblog.Name != blog.Name)
                    {
                        newblog.Name = blog.Name;
                    }

                    if (newblog.Description != blog.Description) 
                    {
                        newblog.Description = blog.Description;
                    }

                    if (newImage is not null)
                    {
                        newblog.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newblog.ImageType = _imageService.ContentType(newImage);
                    }

                    // await _blogService.UpdateBlogAsync(newblog);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
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

            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", blog.BlogUserId);
            return View(blog);
        }
        #endregion

        #region // GET: Blogs/Delete/5
        // GET: Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _blogService.GetBlogByIdAsync(id.Value);


            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
        #endregion

        #region // POST: Blogs/Delete/5
        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Blog blog = await _blogService.GetBlogByIdAsync(id);

            if (blog != null)
            {
                _blogService.RemoveBlogAsync(blog);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region BlogExists
        private bool BlogExists(int id)
        {
            return (_context.Blogs?.Any(e => e.Id == id)).GetValueOrDefault();
        } 
        #endregion
    }
}
