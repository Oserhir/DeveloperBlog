using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services.Interfaces;

namespace TheBlogProject.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICommentService _comment;
        private readonly IPostService _postServer;
        private readonly UserManager<BTUser> _userManager;

        #region Constructor
        public CommentsController(ApplicationDbContext context, ICommentService comment, IPostService postServer,
            UserManager<BTUser> userManager)
        {
            _context = context;
            _comment = comment;
            _postServer = postServer;
            _userManager = userManager;
        }
        #endregion

        #region // GET: Original Index 
        // GET: Comments
        public async Task<IActionResult> OriginalIndex()
        {
            var originalComment = _context.Comments.ToListAsync();
            return View("Index" , await originalComment);
        }
        #endregion

        #region // GET: Moderated Index 
        // GET: Comments
        public async Task<IActionResult> ModeratedIndex()
        {
            var ModeratedComment = await _context.Comments.Where( m => m.ModeratorId != null   ).ToListAsync();
            return View("Index", ModeratedComment);
        }
        #endregion


        #region // GET: Comments 
        // GET: Comments
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Comments.Include(p => p.BlogUser).Include(p => p.Post);
            return View(await applicationDbContext.ToListAsync());
        }
        #endregion 

        #region // GET: Comments/Details/5 --- NO
        //// GET: Comments/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if ( id == null )
        //    {
        //        return NotFound();
        //    }

        //    var comment = _comment.GetCommentByIdAsync(id.Value);

        //    if (comment == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(comment);
        //}
        #endregion

        #region // GET: Comments/Create ------ NO
        // GET: Comments/Create
        //public IActionResult Create()
        //{
        //    return View();
        //}
        #endregion

        #region // POST: Comments/Create
        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Body")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.BlogUserId = _userManager.GetUserId(User);
                comment.Created = DateTime.UtcNow;


                await _comment.AddNewCommentAsync(comment);

                // return RedirectToAction("Index", "Home");
                return RedirectToAction("Details", "Posts", new { id = comment.PostId }, "commentSection");

            }

            return View(comment);
        }
        #endregion

        #region // GET: Comments/Edit/5
        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            var comment = await _comment.GetCommentByIdAsync(id.Value);

            if (comment == null)
            {
                return NotFound();
            }

            //ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            //ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            ViewData["PostId"] = new SelectList( await _postServer.GetAllPostAsync(), "Id", "Abstract", comment.PostId);

            return View(comment);
        }
        #endregion

        #region // POST: Comments/Edit/5
        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Body")] Comment comment)
        {
            // need fix comment.Id always = 3
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                Comment newComment = await _comment.GetCommentByIdAsync(id);

                try
                {
                                    

                    newComment.Body = comment.Body;
                    newComment.Updated = DateTime.UtcNow;
                    
                    await _context.SaveChangesAsync();
                    //await _comment.UpdateCommentAsync(newComment);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
               // return RedirectToAction("Details", "Posts", new { slug = newComment.Post.Slug }, "commentSection");
                return RedirectToAction("Details", "Posts", new { id = newComment.Id }, "commentSection");
                // https://localhost:7042/Posts/Details?slug=typescript-4-getting-started#commentSection
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
            return View(comment);
        }
        #endregion

        #region  // GET: Comments/Delete/5
        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _comment.GetCommentByIdAsync(id.Value);

            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }
        #endregion

        #region // POST: Comments/Delete/5
        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // need fix id always = 3
            var comment = await _context.Comments.FindAsync(id);

            if (comment != null)
            {
                await _comment.RemoveCommentAsync(comment);
            }

            await _context.SaveChangesAsync();
            //  return RedirectToAction(nameof(Index));
            return RedirectToAction("Index", "Comments");
      
        }
        #endregion

        #region Moderate
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Administrator,Moderator")]
        public async Task<IActionResult> Moderate(int id, [Bind("Id,Body,ModeratedBody,ModerationType")] Comment comment)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);
                try
                {
                    newComment.ModeratedBody = comment.ModeratedBody;
                    newComment.ModerationType = comment.ModerationType;

                    newComment.Moderated = DateTime.UtcNow;
                    newComment.ModeratorId = _userManager.GetUserId(User);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

               // return RedirectToAction("Details", "Posts", new { slug = newComment.Post.Slug }, "commentSection");
                return RedirectToAction("Details", "Posts", new { id = newComment.Post.Id }, "commentSection");
            }

            return View(comment);
        }

        #endregion

        #region CommentExists
        private bool CommentExists(int id)
        {
            return (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
        } 
        #endregion
    }
}
