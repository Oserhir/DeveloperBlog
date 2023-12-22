using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services.Interfaces;

namespace TheBlogProject.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPostService _postService;
        private readonly IBlogService _BlogService;

        #region Constructor
        public PostsController(ApplicationDbContext context, IPostService postService, IBlogService blogService)
        {
            _context = context;
            _postService = postService;
            _BlogService = blogService;
        }
        #endregion

        #region // GET: Posts
        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.Blog).Include(p => p.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }
        #endregion

        #region // GET: Posts/Details/5
        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            var post = await _postService.GetPostByIdAsync(id.Value);

            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
        #endregion

        #region // GET: Posts/Create
        // GET: Posts/Create
        public async Task<IActionResult> Create(int? id)
        {

            ViewData["BlogId"] = new SelectList(await _BlogService.GetAllBlogsAsync(), "Id", "Name");
            //ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");

            return View();
        }
        #endregion

        #region // POST: Posts/Create
        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Abstract,Content,ReadyStatus")] Post post)
        {
            if (ModelState.IsValid)
            {

                post.Created = DateTime.UtcNow;

                await _postService.AddNewPostAsync(post);

                return RedirectToAction(nameof(Index));
            }

            ViewData["BlogId"] = new SelectList(await _BlogService.GetAllBlogsAsync(), "Id", "Name", post.BlogId);
            // ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);

            return View(post);
        }
        #endregion

        #region // GET: Posts/Edit/5
        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            var post = await _postService.GetPostByIdAsync(id.Value);

            if (post == null)
            {
                return NotFound();
            }

            ViewData["BlogId"] = new SelectList(await _BlogService.GetAllBlogsAsync(), "Id", "Name", post.BlogId);
            //ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);

            return View(post);
        }

        #endregion

        #region  // POST: Posts/Edit/5
        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,BlogId,Title,Abstract,Content,ReadyStatus")] Post post)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Post newPost = await _postService.GetPostByIdAsync(id.Value);

                    newPost.Updated = DateTime.UtcNow;
                    newPost.BlogId = post.BlogId;
                    newPost.Title = post.Title;
                    newPost.Abstract = post.Abstract;
                    newPost.Content = post.Content;
                    newPost.ReadyStatus = post.ReadyStatus;

                    await _postService.UpdatePostAsync(newPost);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
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
            ViewData["BlogId"] = new SelectList(await _BlogService.GetAllBlogsAsync(), "Id", "Name", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        }
        #endregion

        #region  // GET: Posts/Delete/5
        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _postService.GetPostByIdAsync(id.Value);


            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }
        #endregion

        #region  // POST: Posts/Delete/5
        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);

            if (post != null)
            {
                _postService.RemovePostAsync(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        #endregion

        #region PostExists
        private bool PostExists(int id)
        {
            return (_context.Posts?.Any(e => e.Id == id)).GetValueOrDefault();
        } 
        #endregion
    }
}
