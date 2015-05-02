using Newtonsoft.Json;
using RedisCacheDemo.Helpers;
using RedisCacheDemo.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace RedisCacheDemo.Controllers
{
    public class ProductsController : Controller
    {
        private DemoContext db = new DemoContext();

        public ActionResult Index()
        {
            return View(db.Products.ToList());
        }

        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Product product = FindProductInCache(id.Value);

            if (product == null)
            {
                product = db.Products.Find(id);

                if (product == null)
                {
                    return HttpNotFound();
                }

                StoreProductInCache(product);
                product.Message = "Returned by Database.";
            }
            else
            {
                product.Message = "Returned by Redis.";
            }
            return View(product);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Price,Message")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Price,Message")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();

                RemoveProductFromCache(product.Id);
                return RedirectToAction("Index");
            }
            return View(product);
        }

        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();

            RemoveProductFromCache(id);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private Product FindProductInCache(Guid guid)
        {
            var db = RedisHelper.GetDatabase();

            var product = db.StringGet(guid.ToString());

            if (product.IsNullOrEmpty)
            {
                return null;
            }

            return JsonConvert.DeserializeObject<Product>(product);
        }

        private void StoreProductInCache(Product product)
        {
            var db = RedisHelper.GetDatabase();
            db.StringSet(product.Id.ToString(), JsonConvert.SerializeObject(product));
        }

        private void RemoveProductFromCache(Guid guid)
        {
            var db = RedisHelper.GetDatabase();
            db.KeyDelete(guid.ToString());
        }
    }
}