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
using TheBlogProject.Enums;
using TheBlogProject.Models;
using TheBlogProject.Services;
using TheBlogProject.Services.Interfaces;
using X.PagedList;

namespace TheBlogProject.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPostService _postService;
        private readonly ISlugService _slugService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IImageService _imageService;
        private readonly BlogSearchService _blogSearchService;
        private readonly ICategoryService _categoryService;

        #region Constructor
        public PostsController(ApplicationDbContext context, IPostService postService, ISlugService slugService,
                  IImageService imageService, UserManager<BTUser> userManager, BlogSearchService blogSearchService, ICategoryService categoryService)
        {
            _context = context;
            _postService = postService;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
            _categoryService = categoryService;
        }
        #endregion


        #region Search Index
        public async Task<IActionResult> SearchIndex(int? page, string searchTerm)
        {
            ViewData["SearchTerm"] = searchTerm;

            var pageNumber = page ?? 1;
            var pageSize = 6;
            var posts =  _blogSearchService.Search(searchTerm);

            return View(await posts.ToPagedListAsync(pageNumber, pageSize));
        }
        #endregion

        #region // GET: Posts
        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.PostUser);
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
            ViewData["CategoryId"] = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "title");

            return View();
        }
        #endregion
        
        #region // POST: Posts/Create
        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,CategoryId,Abstract,Content,ReadyStatus,Image")] Post post, List<string> tagValues)
        {
            if (ModelState.IsValid)
            {
                post.Created = DateTime.UtcNow;

                var authorId =  _userManager.GetUserId(User);
                post.PostUserId = authorId;

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
                    ViewData["CategoryId"] = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "title");

                    return View(post);
                }

                post.Slug = slug;

                await _postService.AddNewPostAsync(post);


                // Loop over incoming list of string
                foreach (var tagText in tagValues)
                {
                    _context.Add(new Tag()
                    {
                        PostId = post.Id,
                        BlogUserId = authorId,
                        Text = tagText
                    });
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["TagValues"] = string.Join(",", tagValues);
            ViewData["CategoryId"] = new SelectList(await _categoryService.GetAllCategoriesAsync(), "Id", "title");

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

            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));

            return View(post);
        }

        #endregion

        #region  // POST: Posts/Edit/5
        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("Id,Title,Abstract,Content,ReadyStatus")] Post post,
                    IFormFile newImage, List<string> tagValues) // name of selectlist tagValues has to match, important!
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
                            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));
                            return View(post);
                        }
                    }

                    if (newImage is not null)
                    {
                        newPost.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newPost.ImageType = _imageService.ContentType(newImage);
                    }

                    // await _postService.UpdatePostAsync(newPost);

                    // Remove all Tags previously associated with this post
                    _context.Tags.RemoveRange(newPost.Tags);

                    // Add new tags from the form
                    foreach (var tagText in tagValues)
                    {
                        _context.Add(new Tag()
                        {
                            PostId = post.Id,
                            BlogUserId = newPost.PostUserId,
                            Text = tagText
                        });
                    }

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


           // ViewData["BlogId"] = new SelectList(await _BlogService.GetAllBlogsAsync(), "Id", "Name", post.BlogId);
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.PostUserId);

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
