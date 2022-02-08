using CSG.Data;
using CSG.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CSG.Components
{
    public class FooterViewComponent:ViewComponent
    {
        private readonly GizemContext _dbContext;
        public FooterViewComponent(GizemContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IViewComponentResult Invoke(RegisterLoginViewModel registerLoginViewModel)
        {
            return View(registerLoginViewModel);
        }
    }
}
