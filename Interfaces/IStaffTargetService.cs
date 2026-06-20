using WebMVC.Entities;
using WebMVC.Models.Responses;

namespace WebMVC.Interfaces
{
    public interface IStaffTargetService
    {
        Task<List<StaffTargetResponse>> GetListStaffTarget(DateTime dataDate, int? staffId = null);
        Task<bool> UpdateStaffImage(int id, IFormFile image);
        Task<bool> UpdateStaffTarget(int id, StaffTarget request);
    }
}
