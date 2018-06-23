using System;

namespace baking1.utility
{
    public class Utility
    {
        // Yêu cầu người dùng nhập vào một số nguyên,
        // trong trường hợp nhập sai thì yêu cầu nhập lại.    
        public static decimal GetUnsignDecimalNumber()
        {
            decimal choice;
            while (true)
            {
                try
                {
                    var strChoice = Console.ReadLine();
                    choice = Decimal.Parse(strChoice);
                    if (choice <= 0)
                    {
                        throw new FormatException();
                    }
                    else
                    {
                        break;
                    }
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please enter a unsign number.");
                }
            }

            return choice;
        }

        public static int GetInt32Number()
        {
            var number = 0;
            while (true)
            {
                try
                {
                    number = Int32.Parse(Console.ReadLine());
                    break;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please enter a number.");
                }
            }

            return number;
        }

        public static decimal GetDecimalNumber()
        {
            decimal number = 0;
            while (true)
            {
                try
                {
                    number = Decimal.Parse(Console.ReadLine());
                    break;
                }
                catch (FormatException e)
                {
                    Console.WriteLine("Please enter a number.");
                }
            }

            return number;
        }
    }
}