using CSG.Models;
using System.Threading.Tasks;

namespace CSG.Services
{
    public interface IEmailSender
    {
        Task SendAsync(EmailMessage message);
    }
}
