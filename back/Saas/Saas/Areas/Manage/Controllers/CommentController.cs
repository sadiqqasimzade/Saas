using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Saas.DAL;
using Saas.Models;
using Saas.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Saas.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class CommentController : Controller
    {
        private readonly AppDbContext _context;

        public CommentController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            List<Comment> comments = await _context.Comments.ToListAsync();
            return View(comments);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            Comment comment = await _context.Comments.FindAsync(id);
            if (comment == null) return View();
            if (System.IO.File.Exists(Path.Combine(Consts.CommentImgPath, comment.Img)))
                System.IO.File.Delete(Path.Combine(Consts.CommentImgPath, comment.Img));

            _context.Remove(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Comment comment)
        {
            if (comment == null) return View();
            if (!ModelState.IsValid||comment.File==null) return View(comment);
            if(!comment.File.ContentType.Contains("image"))
            {
                ModelState.AddModelError("File", "Wrong file type");
                return View(comment);
            }
            if(comment.File.Length/1024>Consts.CommentImgSizeKb)
            {
                ModelState.AddModelError("File", "File size cant be greater than:"+Consts.CommentImgSizeKb+"KB");
                return View(comment);
            }
            string filename = Guid.NewGuid().ToString() + comment.File.FileName;
            if (filename.Length > Consts.CommentImgNameLength)
                filename = filename.Substring(filename.Length - Consts.CommentImgNameLength, Consts.CommentImgNameLength);
            using (FileStream fs = new FileStream(Path.Combine(Consts.CommentImgPath, filename),FileMode.Create))
                await comment.File.CopyToAsync(fs);

            comment.Img = filename;
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Update(int id)
        {
            Comment comment = await _context.Comments.FindAsync(id);
            if (comment == null) return RedirectToAction("Index");
            return View(comment);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Comment comment)
        {
            if (!ModelState.IsValid) return View(comment);
            Comment dbcomment = await _context.Comments.FindAsync(comment.Id);
            if(dbcomment==null) return RedirectToAction("Index");
            dbcomment.Name = comment.Name;
            dbcomment.Desc = comment.Desc;
            
            if(comment.File!=null&&comment.File.Length<Consts.CommentImgSizeKb && comment.File.ContentType.Contains("image"))
            {
                if (System.IO.File.Exists(Path.Combine(Consts.CommentImgPath, dbcomment.Img)))
                    System.IO.File.Delete(Path.Combine(Consts.CommentImgPath, dbcomment.Img));
                string filename = Guid.NewGuid().ToString() + comment.Name;
                if (filename.Length > Consts.CommentImgNameLength)
                    filename = filename.Substring(filename.Length - Consts.CommentImgNameLength, Consts.CommentImgNameLength);
                using (FileStream fs = new FileStream(Path.Combine(Consts.CommentImgPath, filename), FileMode.Create))
                    await comment.File.CopyToAsync(fs);

                dbcomment.Img = filename;

            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
