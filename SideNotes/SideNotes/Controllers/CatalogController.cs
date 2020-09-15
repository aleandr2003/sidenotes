using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Services.Abstract;
using SideNotes.Services;
using SideNotes.Models;
using SideNotes.Controllers.Abstract;
using SideNotes.ActionResults;
using SideNotes.ViewModels;
using SideNotes.Extensions;
using System.Web.Routing;

namespace SideNotes.Controllers
{
    public class CatalogController : SidenotesController
    {
        private IAuthorizationService authz;
        public CatalogController(IAuthorizationService authz)
        {
            this.authz = authz;
            ViewData["SelectedTab"] = HeaderTabs.Catalog;
        }

        public ActionResult Index(int? Id)
        {
            ViewBag.SelectedCategory = Id;
            return View();
        }

        public ActionResult Category(int? Id)
        {
            ViewBag.SelectedCategory = Id;
            return View("Index");
        }

        [HttpPost]
        public JsonResult GetCategoriesJson()
        {
            using (var context = new SideNotesEntities())
            {
                var categories = context.Categories.ToList();
                var catalog = new Catalog(categories);
                return new SidenotesJsonResult() { Data = catalog.RootCategories };
            }
        }

        [HttpPost]
        public JsonResult GetBooksJson(int? Id)
        {
            using (var context = new SideNotesEntities())
            {
                var books = new List<Book>();
                var descendantBooks = new List<Book>();
                if (Id != null)
                {
                    var category = context.Categories.FirstOrDefault(c => c.Id == Id);
                    if (category == null) return Json(new { });
                    books.AddRange(category.Books.ToList());

                    var categories = context.Categories.ToList();
                    var catalog = new Catalog(categories);

                    var descendants = catalog.GetDescendants(category.Id);
                    descendantBooks.AddRange(descendants.SelectMany(c => c.Books));
                    descendantBooks.ForEach(b => { var p = b.Avatar != null ? b.Avatar.Small : null; });//подгружаем аватары
                }
                else
                {
                    books.AddRange(context.Books.ToList());
                }

                var bookModels = books.Select(b => new CatalogBookModel()
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author,
                    AvatarUrl = b.Avatar != null ? VirtualPathUtility.ToAbsolute(b.Avatar.Medium.Url) : BookAvatarService.NoAvatarMedium,
                    ReadUrl = Url.Action("Start", "Book", new { Id = b.Id }, true),
                    AnnotationUrl = Url.Action("Annotation", "Book", new { Id = b.Id }, true)
                }).ToList();

                var descendantBookModels = descendantBooks.Select(b => new CatalogBookModel()
                {
                    Id = b.Id,
                    Title = b.Title,
                    AuthorName = b.Author,
                    AvatarUrl = b.Avatar != null ? VirtualPathUtility.ToAbsolute(b.Avatar.Medium.Url) : BookAvatarService.NoAvatarMedium,
                    ReadUrl = Url.Action("Start", "Book", new { Id = b.Id }, true),
                    AnnotationUrl = Url.Action("Annotation", "Book", new { Id = b.Id }, true)
                }).ToList();
                return Json(new { books = bookModels, subfolderBooks = descendantBookModels });
            }
        }

        [HttpPost]    
        public JsonResult AddCategory(string name, int? parentId)
        {
            if (!authz.Authorize(Operation.EditCatalog)) 
                throw new UnauthorizedAccessException(Resources.Catalog.ControllerCantEditCatalog);
            using(var context = new SideNotesEntities()){
                if (context.Categories.Any(c => c.ParentId == parentId && c.Name == name))
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerCategoryAlreadyExists });
                if (parentId != null && !context.Categories.Any(c => c.Id == parentId))
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerParentCategoryNotFound });
                var category = new Category()
                {
                    ParentId = parentId,
                    Name = name
                };
                context.Categories.AddObject(category);
                context.SaveChanges();
                return Json(new { });
            }
        }

        [HttpPost]
        public JsonResult EditCategory(int Id, string name, int? parentId)
        {
            if (!authz.Authorize(Operation.EditCatalog))
                throw new UnauthorizedAccessException(Resources.Catalog.ControllerCantEditCatalog);
            using (var context = new SideNotesEntities())
            {
                if (parentId != null && !context.Categories.Any(c => c.Id == parentId))
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerParentCategoryNotFound });
                var category = context.Categories.FirstOrDefault(c => c.Id == Id);
                if (category == null)
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerCategoryNotFound });
                category.ParentId = parentId;
                category.Name = name;

                context.SaveChanges();
                return Json(new { });
            }
        }

        [HttpPost]
        public JsonResult DeleteCategory(int Id)
        {
            if (!authz.Authorize(Operation.EditCatalog))
                throw new UnauthorizedAccessException(Resources.Catalog.ControllerCantEditCatalog);
            using (var context = new SideNotesEntities())
            {
                var category = context.Categories.FirstOrDefault(c => c.Id == Id);
                if (category == null)
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerCategoryNotFoundMaybeDeleted });

                category.Books.Clear();
                context.Categories.Where(c => c.ParentId == Id).ToList().ForEach(c => c.ParentId = null);
                context.Categories.DeleteObject(category);
                context.SaveChanges();
                return Json(new { });
            }
        }

        [HttpPost]
        public JsonResult AddBookCategory(int BookId, int parentId)
        {
            if (!authz.Authorize(Operation.EditCatalog))
                throw new UnauthorizedAccessException(Resources.Catalog.ControllerCantEditCatalog);
            using (var context = new SideNotesEntities())
            {
                var category = context.Categories.FirstOrDefault(c => c.Id == parentId);
                if (category == null)
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerCategoryNotFound });
                var book = context.Books.FirstOrDefault(b => b.Id == BookId);
                if (book == null)
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerBookNotFound});
                if (!book.Categories.Contains(category))
                    book.Categories.Add(category);
                context.SaveChanges();
                return Json(new { });
            }
        }

        [HttpPost]
        public JsonResult RemoveBookCategory(int BookId, int CatId)
        {
            if (!authz.Authorize(Operation.EditCatalog))
                throw new UnauthorizedAccessException(Resources.Catalog.ControllerCantEditCatalog);
            using (var context = new SideNotesEntities())
            {
                var category = context.Categories.FirstOrDefault(c => c.Id == CatId);
                if (category == null)
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerCategoryNotFound });
                var book = context.Books.FirstOrDefault(b => b.Id == BookId);
                if (book == null)
                    return Json(new { ErrorMessage = Resources.Catalog.ControllerBookNotFound });
                if (book.Categories.Contains(category))
                    book.Categories.Remove(category);
                context.SaveChanges();
                return Json(new { });
            }
        }

        public ActionResult Manage()
        {
            if (!authz.Authorize(Operation.EditCatalog))
                throw new UnauthorizedAccessException(Resources.Catalog.ControllerCantEditCatalog);
            using (var context = new SideNotesEntities())
            {
                var categories = context.Categories.ToList();
                var catalog = new Catalog(categories);
                return View(catalog);
            }
        }

        public ActionResult ManageBookCategories(int BookId)
        {
            if (!authz.Authorize(Operation.EditCatalog))
                throw new UnauthorizedAccessException(Resources.Catalog.ControllerCantEditCatalog);
            using (var context = new SideNotesEntities())
            {
                var book = context.Books.Include("Categories").FirstOrDefault(b => b.Id == BookId);
                if (book == null)
                {
                    throw new ArgumentException(Resources.Catalog.ControllerBookNotFound);
                }

                var categories = context.Categories.ToList();
                var catalog = new Catalog(categories);

                return View(Tuple.Create(book, catalog));
            }
        }

        public ActionResult ManageCategoryContent(int Id)
        {
            if (!authz.Authorize(Operation.EditCatalog))
                throw new UnauthorizedAccessException(Resources.Catalog.ControllerCantEditCatalog);
            using (var context = new SideNotesEntities())
            {
                var category = context.Categories.Include("Books.Avatar.Small").FirstOrDefault(c => c.Id == Id);
                if (category == null)
                {
                    throw new ArgumentException(Resources.Catalog.ControllerCategoryNotFound);
                }

                var categories = context.Categories.ToList();
                var catalog = new Catalog(categories);

                var descendants = catalog.GetDescendants(category.Id);
                var descendantBooks = descendants.SelectMany(c => c.Books).ToList();
                descendantBooks.ForEach(b => { var p = b.Avatar != null ? b.Avatar.Small : null; });//подгружаем аватары

                return View(Tuple.Create(category, descendantBooks));
            }
        }

    }

    public class CatalogBookModel
    {
        public int Id;
        public string AvatarUrl;
        public string AnnotationUrl;
        public string ReadUrl;
        public string AuthorName;
        public string Title;
    }
}
