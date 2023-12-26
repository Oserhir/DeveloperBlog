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
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPostService _postService;
        private readonly IBlogService _BlogService;
        private readonly ISlugService _slugService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IImageService _imageService;

        #region Constructor
        public PostsController(ApplicationDbContext context, IPostService postService, IBlogService blogService, ISlugService slugService,
                  IImageService imageService, UserManager<BTUser> userManager)
        {
            _context = context;
            _postService = postService;
            _BlogService = blogService;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
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

        [Authorize]
        #region // GET: Posts/Create
        // GET: Posts/Create
        public async Task<IActionResult> Create(int? id)
        {

            ViewData["BlogId"] = new SelectList(await _BlogService.GetAllBlogsAsync(), "Id", "Name");

            return View();
        }
        #endregion
        
        #region // POST: Posts/Create
        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogId,Title,Abstract,Content,ReadyStatus,Image")] Post post, List<string> tagValues)
        {
            if (ModelState.IsValid)
            {
                post.Created = DateTime.UtcNow;

                var authorId =  _userManager.GetUserId(User);
                post.BlogUserId = authorId;

                // Use the _imageService to store user image
                post.ImageData = await _imageService.EncodeImageAsync(post.Image);
                post.ImageType = _imageService.ContentType(post.Image);

                // Create the slug and determine if it's unique
                var slug = _slugService.UrlFriendly(post.Title);

                // Create variable to store whether an error has occurred
                var validationError = false;

                // Using else if to avoid collisions, so 2 different errors don't show up for the same property
                if (string.IsNullOrEmpty(slug))
                {
                    ModelState.AddModelError("", "The Title you provided cannot be used as it results in an empty slug.");
                    validationError = true;
                }
                else if (!_slugService.IsUnique(slug))
                {
                    // Add model state err and return the user back to the create view
                    ModelState.AddModelError("Title", "The Title you provided cannot be used as it must be unique.");
                    validationError = true;
                }

                if (validationError)
                {
                    // return the user back to the Create view
                    ViewData["TagValues"] = string.Join(",", tagValues);
                    ViewData["BlogId"] = new SelectList(await _BlogService.GetAllBlogsAsync(), "Id", "Name");
                    return View(post);
                }

                post.Slug = slug;

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
        public async Task<IActionResult> Edit(int? id, [Bind("Id,BlogId,Title,Abstract,Content,ReadyStatus")] Post post,
                    IFormFile newImage )
        {
            // err need fix : newImage

            if (id == null)
            {
                return NotFound();
            }

            var errors = ViewData.ModelState.Where(n => n.Value.Errors.Count > 0).ToList();

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

                    var newSlug = _slugService.UrlFriendly(post.Title);
                    // newPost.Slug is really the old slug
                    if (newSlug != newPost.Slug)
                    {
                        if (_slugService.IsUnique(newSlug))
                        {
                            newPost.Title = post.Title;
                            newPost.Slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "This Title cannot be used as it results in a duplicate slug");
                            //ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));
                            return View(post);
                        }
                    }

                    if (newImage is not null)
                    {
                        newPost.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newPost.ImageType = _imageService.ContentType(newImage);
                    }

                    // await _postService.UpdatePostAsync(newPost);

                    // save specific changes
                    await _context.SaveChangesAsync();

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
