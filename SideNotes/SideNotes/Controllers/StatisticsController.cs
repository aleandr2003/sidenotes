using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SideNotes.Models;
using System.Transactions;

namespace SideNotes.Controllers
{
    public class StatisticsController : Controller
    {
        public ActionResult ArchiveDaily()
        {
            if (!Request.ServerVariables["REMOTE_HOST"].StartsWith("10."))
                throw new UnauthorizedAccessException("Only server can access statistics actions");
            StoreCampaigns();
            StoreDistinctVisitors();
            return View("OKView");
        }

        private void StoreCampaigns()
        {
            using (var context = new SideNotesEntities())
            {
                using (var scope = new TransactionScope())
                {
                    var campaigns = context.DailyHits.ToList();
                    foreach (var campaign in campaigns)
                    {
                        var archive = new DailyHitsArchive()
                        {
                            CampaignId = campaign.CampaignId,
                            hits = campaign.hits,
                            date = DateTime.Now
                        };
                        context.DailyHitsArchives.AddObject(archive);
                        campaign.hits = 0;
                    }
                    context.SaveChanges();
                    scope.Complete();
                }
            }
        }

        private void StoreDistinctVisitors()
        {
            using (var context = new SideNotesEntities())
            {
                var yesterday = DateTime.Now.AddDays(-1);
                var now = DateTime.Now;
                var count = context.RequestLogs
                    .Where(l => l.RequestDate > yesterday && l.RequestDate < now)
                    .Select(l => l.Remote_addr).Distinct().Count();
                var archive = new DailyHitsArchive()
                {
                    CampaignId = "UniqueVisitors",
                    hits = count,
                    date = DateTime.Now
                };
                context.DailyHitsArchives.AddObject(archive);
                context.SaveChanges();
            }
        }
    }
}
