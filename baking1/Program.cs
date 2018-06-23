using baking1.entity;
using baking1.view;
using baking1.view;

namespace baking1
{
    internal class Program
    {

        public static YYAccount currentLoggedInYyAccount;

        static void Main(string[] args)
        {
            ApplicationView view = new ApplicationView();
            while (true)
            {
                if (Program.currentLoggedInYyAccount != null)
                {
                    view.GenerateCustomerMenu();
                }
                else
                {
                    view.GenerateDefaultMenu();
                }
            }
        }
    }
}