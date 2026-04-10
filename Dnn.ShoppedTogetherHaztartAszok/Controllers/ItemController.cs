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

using DotNetNuke.Entities.Users;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using ShoppedTogetherHaztartasok.Dnn.Models;
using ShoppedTogetherHaztartasok.Dnn.Services;
using ShoppedTogetherHaztartAszok.Dnn.Dnn.ShoppedTogetherHaztartAszok.Components;
using ShoppedTogetherHaztartAszok.Dnn.Dnn.ShoppedTogetherHaztartAszok.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ActionResult Index()
        {
            var runSync = Request.QueryString["runSync"];

            if (runSync == "1")
            {
                var service = new ShoppedTogetherHaztartasok.Dnn.Services.ShoppedTogetherService();
                service.RunIncrementalSync();
                ViewBag.Message = "Sync lefutott.";
            }

            var recommendationService = new ShoppedTogetherHaztartasok.Dnn.Services.ShoppedTogetherService();

            var recommendedIds = recommendationService.GetTopRecommendedProductIds(
                new List<int> { 39, 31, 34 },
                5);

            ViewBag.RecommendedIds = recommendedIds;

            var model = new List<Item>();
            return View(model);
        }
    }
}
