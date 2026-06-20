using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class AccountResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SaleName { get; set; }
        public int RoleId { get; set; }
        public string RoleName
        {
            get
            {
                return ERoleIdName.GetRoleName(RoleId);
            }
        }
        public DateTime Created { get; set; }

        public List<PricingResponse> PricingResponses { get; set; } = new List<PricingResponse>();

        public List<AccountSalesResponse> AccountSalesResponses { get; set; } = new List<AccountSalesResponse>();
    }

    public class AccountSalesResponse
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string ShipName { get; set; }
    }
}
