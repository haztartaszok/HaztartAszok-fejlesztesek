/*
' Copyright (c) 2026 HaztartAszok
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using Dnn.ShoppedTogetherHaztartAszok.Services;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using Hotcakes.Commerce;
using Hotcakes.Commerce.Orders;
using ShoppedTogetherHaztartasok.Dnn.Models;
using ShoppedTogetherHaztartasok.Dnn.Services;
using ShoppedTogetherHaztartAszok.Dnn.Dnn.ShoppedTogetherHaztartAszok.Components;
using ShoppedTogetherHaztartAszok.Dnn.Dnn.ShoppedTogetherHaztartAszok.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppedTogetherHaztartAszok.Dnn.Dnn.ShoppedTogetherHaztartAszok.Controllers
{
    [DnnHandleError]
    public class ItemController : DnnController
    {


        public ActionResult Delete(int itemId)
        {
            ItemManager.Instance.DeleteItem(itemId, ModuleContext.ModuleId);
            return RedirectToDefaultRoute();
        }

        public ActionResult Edit(int itemId = -1)
        {
            DotNetNuke.Framework.JavaScriptLibraries.JavaScript.RequestRegistration(CommonJs.DnnPlugins);

            var userlist = UserController.GetUsers(PortalSettings.PortalId);
            var users = from user in userlist.Cast<UserInfo>().ToList()
                        select new SelectListItem { Text = user.DisplayName, Value = user.UserID.ToString() };

            ViewBag.Users = users;

            var item = (itemId == -1)
                 ? new Item { ModuleId = ModuleContext.ModuleId }
                 : ItemManager.Instance.GetItem(itemId, ModuleContext.ModuleId);

            return View(item);
        }

        [HttpPost]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Edit(Item item)
        {
            if (item.ItemId == -1)
            {
                item.CreatedByUserId = User.UserID;
                item.CreatedOnDate = DateTime.UtcNow;
                item.LastModifiedByUserId = User.UserID;
                item.LastModifiedOnDate = DateTime.UtcNow;

                ItemManager.Instance.CreateItem(item);
            }
            else
            {
                var existingItem = ItemManager.Instance.GetItem(item.ItemId, item.ModuleId);
                existingItem.LastModifiedByUserId = User.UserID;
                existingItem.LastModifiedOnDate = DateTime.UtcNow;
                existingItem.ItemName = item.ItemName;
                existingItem.ItemDescription = item.ItemDescription;
                existingItem.AssignedUserId = item.AssignedUserId;

                ItemManager.Instance.UpdateItem(existingItem);
            }

            return RedirectToDefaultRoute();
        }

        [ModuleAction(ControlKey = "Edit", TitleKey = "AddItem")]


        private IEnumerable<ShoppedTogetherProductPair> GetTopPairs(int productId)
        {
            var service = new ShoppedTogetherService();

            return service.GetPairsForProduct(productId)
                .OrderByDescending(p => p.TogetherCount)
                .Take(5);
        }

        public ActionResult RunSync()
        {
            var service = new ShoppedTogetherHaztartasok.Dnn.Services.ShoppedTogetherService();
            service.RunIncrementalSync();

            return Content("Sync lefutott");
        }

        public ActionResult AdminStats()
        {
            return Content("ADMINSTATS ACTION FUT");
        }


        private static string CsvEscape(string value)
        {
            if (value == null)
                return "";

            value = value.Replace("\"", "\"\"");
            value = value.Replace("\r", " ").Replace("\n", " ");

            return "\"" + value + "\"";
        }

        public ActionResult Index(int? productId = null)
        {
            var limitParam = Request.QueryString["limit"];
            int limit = 200;

            if (!string.IsNullOrEmpty(limitParam))
            {
                int parsedLimit;
                if (int.TryParse(limitParam, out parsedLimit) && parsedLimit > 0)
                {
                    limit = parsedLimit;
                }
            }


            if (Request.QueryString["exportCsv"] == "1" && productId.HasValue)
            {
                var connString = DotNetNuke.Data.DataProvider.Instance().ConnectionString;
                var repo = new StatisticsRepository(connString);

                var products = repo.GetProducts();
                var selectedProductName = products.FirstOrDefault(p => p.Id == productId.Value)?.ProductName ?? "Ismeretlen termék";
                var relatedProducts = repo.GetTopRelatedProducts(productId.Value);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("sep=;");
                sb.AppendLine("ValasztottTermek;EgyuttVasaroltTermek;TogetherCount");

                foreach (var item in relatedProducts)
                {
                    var selected = CsvEscape(selectedProductName);
                    var related = CsvEscape(item.ProductName ?? "");

                    sb.AppendLine($"{selected};{related};{item.TogetherCount}");
                }

                var enc = System.Text.Encoding.GetEncoding(1250);
                var bytes = enc.GetBytes(sb.ToString());

                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.Buffer = true;
                Response.ContentType = "text/csv; charset=windows-1250";
                Response.ContentEncoding = System.Text.Encoding.GetEncoding(1250);
                Response.AddHeader(
                    "Content-Disposition",
                    $"attachment; filename=egyutt-vasarolt-termekek-{productId.Value}.csv"
                );

                Response.BinaryWrite(bytes);
                Response.Flush();

                Response.SuppressContent = true;
                HttpContext.ApplicationInstance.CompleteRequest();

                return new EmptyResult();
            }

            if (Request.QueryString["exportCsv"] == "1" && !productId.HasValue)
            {

                var connString = DotNetNuke.Data.DataProvider.Instance().ConnectionString;
                var repo = new StatisticsRepository(connString);

                var pairs = repo.GetTopProductPairs(limit);

                var sb = new System.Text.StringBuilder();
                sb.AppendLine("TermekA;TermekB;TogetherCount");

                foreach (var item in pairs)
                {
                    var productA = (item.ProductAName ?? "").Replace(";", ",");
                    var productB = (item.ProductBName ?? "").Replace(";", ",");

                    sb.AppendLine(
                        System.Web.HttpUtility.HtmlDecode(productA) + ";" +
                        System.Web.HttpUtility.HtmlDecode(productB) + ";" +
                        item.TogetherCount
                    );
                }

                Response.Clear();
                Response.ClearHeaders();
                Response.ClearContent();
                Response.Buffer = true;
                Response.ContentEncoding = System.Text.Encoding.UTF8;
                Response.ContentType = "text/csv";
                Response.AddHeader("Content-Disposition", "attachment; filename=top-egyuttvasarlas.csv");
                Response.Write(sb.ToString());
                Response.Flush();
                Response.SuppressContent = true;
                HttpContext.ApplicationInstance.CompleteRequest();

                return new EmptyResult();
            }


            var runSync = Request.QueryString["runSync"];

            if (runSync == "1")
            {
                var service = new ShoppedTogetherHaztartasok.Dnn.Services.ShoppedTogetherService();
                service.RunIncrementalSync();
                ViewBag.Message = "Sync lefutott.";
            }

            var app = Hotcakes.Commerce.HotcakesApplication.Current;
            var cart = app.OrderServices.CurrentShoppingCart();

            var cartProductIds = new List<int>();

            if (cart != null)
            {
                var productRepo = DotNetNuke.Data.DataContext.Instance()
                    .GetRepository<ShoppedTogetherHaztartasok.Dnn.Models.SourceProduct>();

                var allProducts = productRepo.Get().ToList();

                var productBvinToId = allProducts
                    .Where(p => !string.IsNullOrEmpty(p.bvin))
                    .GroupBy(p => p.bvin)
                    .ToDictionary(g => g.Key, g => g.First().Id);

                cartProductIds = cart.Items
                    .Select(i => i.ProductId)
                    .Where(productBvin => !string.IsNullOrEmpty(productBvin) && productBvinToId.ContainsKey(productBvin))
                    .Select(productBvin => productBvinToId[productBvin])
                    .Distinct()
                    .ToList();
            }

            var recommendationService = new ShoppedTogetherHaztartasok.Dnn.Services.ShoppedTogetherService();

            List<int> recommendedIds;

            if (cartProductIds.Any())
            {
                var relatedIds = recommendationService
                    .GetTopRecommendedProductIds(cartProductIds, 5)
                    .ToList();

                var fallbackIds = recommendationService
                    .GetTopSellingProductIds(20)
                    .Where(id => !cartProductIds.Contains(id))
                    .Where(id => !relatedIds.Contains(id))
                    .ToList();

                recommendedIds = relatedIds
                    .Concat(fallbackIds)
                    .Distinct()
                    .Take(5)
                    .ToList();
            }
            else
            {
                recommendedIds = recommendationService
                    .GetTopSellingProductIds(5)
                    .ToList();
            }

            var recommendedProducts = recommendationService
                .GetRecommendedProductsByIds(recommendedIds)
                .ToList();

            ViewBag.RecommendedProducts = recommendedProducts;

            var model = new List<Item>();

            if (PortalSettings.ActiveTab.TabName == "Admin Statisztika")
            {
                var connString = DotNetNuke.Data.DataProvider.Instance().ConnectionString;
                var repo = new StatisticsRepository(connString);

                var products = repo.GetProducts();

                var vm = new AdminStatsViewModel
                {
                    Products = products,
                    SelectedProductId = productId,
                    SelectedProductName = productId.HasValue
                        ? products.FirstOrDefault(p => p.Id == productId.Value)?.ProductName
                        : null,
                    RelatedProducts = productId.HasValue
                        ? repo.GetTopRelatedProducts(productId.Value)
                        : new List<RelatedProductStat>(),
                    TopProductPairs = productId.HasValue
                        ? new List<ProductPairStat>()
                        : repo.GetTopProductPairs(limit)
                }
            ;

                return View("AdminStats", vm);
            }

            return View(model);
        }


    }
}

