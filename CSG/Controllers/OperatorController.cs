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
        private readonly RequestRepo _requestRepo;
        private readonly IEmailSender _emailSender;
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

        public IActionResult Index()
        {
            var query1 = from aur in _gizemContext.ApplicationUserRequests
                         join u in _gizemContext.Users on aur.ApplicationUserId equals u.Id
                         join r in _gizemContext.Requests on aur.RequestId equals r.Id
                         join ur in _gizemContext.UserRoles on u.Id equals ur.UserId
                         join rol in _gizemContext.Roles on ur.RoleId equals rol.Id
                         where rol.Name == RoleNames.Customer || rol.Name == RoleNames.Admin || rol.Name == RoleNames.Passive
                         where r.RequestStatus == RequestStatus.Delivered
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
            var DataSource1 = query1.ToList();
            //NAUR = Not Assigned User Requests
            ViewBag.DataSourceNAUR = DataSource1;

            var query2 = (from aur in _gizemContext.UserRoles
                          join u in _gizemContext.Users on aur.UserId equals u.Id
                          join r in _gizemContext.Roles on aur.RoleId equals r.Id
                          where r.Name == nameof(RoleNames.Technician)
                          select new TechnicianAndRequestsViewModel
                          {
                              technicianid = u.Id,
                              technicianname = u.UserName,
                              technicianrequestcount = u.ApplicationUserRequests.Count
                          });
            var DataSource2 = query2.ToList();
            ViewBag.DataSourceTechList = DataSource2;

            return View();
        }

        #region GetAllRequests
        public IActionResult GetRequests()
        {
            //var data = _requestRepo.Get(x => x.RequestStatus == RequestStatus.Delivered).ToList();
            var data = _requestRepo.Get().ToList();
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
            var DataSource = query.ToList();
            int count = DataSource.Cast<OperatorRequestViewModel>().Count();


            return Json(new { result = DataSource, count = count });
        }
        #endregion

        #region AJAXCalls

        [HttpPost]
        public async Task<ActionResult> Save(string technicianid, string requestid)
        {
            var technician = await _userManager.FindByIdAsync(technicianid);
            var technicianrequestcount = technician.ApplicationUserRequests.Count;
            // eğer iki parametreden biri boş gelirse hata mesajı
            if (technicianid == null || requestid == null)
                return BadRequest(new
                {
                    Message = "Bad req."
                });
            var aur = new ApplicationUserRequest()
            {
                RequestId = new Guid(requestid),
                ApplicationUserId = technicianid
            };

            _gizemContext.ApplicationUserRequests.Add(aur);
            var result = _gizemContext.SaveChanges();
            if (result == 1)
            {
                var request = _requestRepo.GetById(new Guid(requestid));
                request.RequestStatus = RequestStatus.Solving;
                _requestRepo.Update(request);


                //email doğrulama
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(technician);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Action("Index", "Home", new { userId = technicianid, code = code },protocol: Request.Scheme);

                var emailMessage = new EmailMessage()
                {
                    Contacts = new string[] { technician.Email },
                    Body =
                        $"There is a fault request assigned to you by the operator.  <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.",
                    Subject = "Fault Request Information"
                };

                await _emailSender.SendAsync(emailMessage);
            }


            var technicianagain = await _userManager.FindByIdAsync(technicianid);
            var technicianrequestcountagain = technicianagain.ApplicationUserRequests.Count;
            return Ok();
        }

        #endregion

    }
}