using CSG.Data;
using CSG.Extensions;
using CSG.Models;
using CSG.Models.Entities;
using CSG.Models.Entities.Enums;
using CSG.Models.Identity;
using CSG.Repository;
using CSG.Services;
using CSG.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CSG.Controllers
{
    public class OperatorController : Controller
    {
        private readonly GizemContext _gizemContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly RequestRepo _requestRepo;
       
        public OperatorController(UserManager<ApplicationUser> userManager,
                                  RequestRepo requestRepo,
                                  GizemContext gizemContext,
                                  IEmailSender emailSender)
        {
            _gizemContext = gizemContext;
            _userManager = userManager;
            _requestRepo = requestRepo;
            _emailSender = emailSender;
        }

        public UserManager<ApplicationUser> Get_userManager()
        {
            return _userManager;
        }

            var query = from aur in _gizemContext.ApplicationUserRequests
                        join u in _gizemContext.Users on aur.ApplicationUserId equals u.Id
                        join r in _gizemContext.Requests on aur.RequestId equals r.Id
                        join ur in _gizemContext.UserRoles on u.Id equals ur.UserId
                        join rol in _gizemContext.Roles on ur.RoleId equals rol.Id
                        where rol.Name == RoleNames.Customer || rol.Name == RoleNames.Admin || rol.Name == RoleNames.Passive
                        select new OperatorRequestViewModel
                        {
                            requestid = r.Id,
                            username = u.UserName,
                            apartmentdetails = r.ApartmentDetails,
                            problem = r.Problem,
                            requesttype1 = r.RequestType1.ToString(),
                            requesttype2 = r.RequestType2.ToString(),
                            requeststatus = r.RequestStatus.ToString()
                        };
            return query;
        }

            var technicianagain = await _userManager.FindByIdAsync(technicianid);
            var technicianrequestcountagain = technicianagain.ApplicationUserRequests.Count;
            return Ok();
        }

        #endregion


        //public IActionResult GetTechnician()
        //{
        //    var query = _requestRepo.Get();
        //    var query2 = (from aur in _gizemContext.UserRoles
        //                  join u in _gizemContext.Users on aur.UserId equals u.Id
        //                  join r in _gizemContext.Roles on aur.RoleId equals r.Id
        //                  select new
        //                  {
        //                      UserId = u.Id,
        //                      UserName = u.UserName,
        //                      RoleId = r.Id,
        //                      UserRole = r.Name,
        //                  });
        //    var query3 = (from aur in _gizemContext.ApplicationUserRequests
        //                  join u in query2 on aur.ApplicationUserId equals u.UserId
        //                  join r in _gizemContext.Requests on aur.RequestId equals r.Id
        //                  select new
        //                  {
        //                      userId = u.UserId,
        //                      userName = u.UserName,
        //                      roleId = r.Id,
        //                      userRole = u.UserRole,
        //                      requestId = aur.RequestId,
        //                      request1Name = aur.Request.RequestType1.ToString(),
        //                      request2Name = aur.Request.RequestType2.ToString(),
        //                      requeststatus=aur.Request.RequestStatus.ToString(),
        //                  });
        //    var DataSource = query3.ToList();
        //    int count = DataSource.Count();

        //    return Json(new { result = DataSource, count = count });

        //}
    }
}
