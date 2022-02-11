using CSG.Models.Entities.Enums;

namespace CSG.ViewModels
{
    public class RequestViewModel
    {
        public RequestType1 RequestType1 { get; set; }
        public RequestType2 RequestType2 { get; set; }
        public double LocationX { get; set; }
        public double LocationY  { get; set; }
        public string ApartmentDetails { get; set; }
        public string Problem { get; set; }


    }
}
