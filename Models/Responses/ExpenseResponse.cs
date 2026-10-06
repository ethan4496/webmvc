using WebMVC.Entities;
using WebMVC.Ultilities.Enums;

namespace WebMVC.Models.Responses
{
    public class ExpenseResponse : Expense
    {
        public string Username { get; set; }

        public string TypeName
        {
            get
            {
                return EExpenseTypeName.GetTypeName(Type);
            }
        }

        public decimal TotalAfterExchange
        {
            get
            {
                return Total * ExchangeRate;
            }
        }
    }
}
