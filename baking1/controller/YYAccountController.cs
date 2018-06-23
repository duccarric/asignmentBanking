using System;
using System.Collections.Generic;
using System.IO;
using baking1.entity;
using baking1.errors;
using baking1.model;
using baking1.utility;
using MySql.Data.MySqlClient;


namespace baking1.controller
{
    public class YYAccountController
    {
        private YYAccountModel model = new YYAccountModel();

        public bool Register()
        {
            YYAccount yyAccount = GetAccountInformation();
            Dictionary<string, string> errors = yyAccount.CheckValidate();
            if (errors.Count > 0)
            {
                Console.WriteLine("Please fix errros below and try again.");
                foreach (var error in errors)
                {
                    Console.WriteLine(error);
                }

                return false;
            }
            else
            {
                // Lưu vào database.
                yyAccount.EncryptPassword();
                model.Save(yyAccount);
                return true;
            }
        }

        /** Xử lý đăng nhập người dùng.
         *  1. Yêu cầu người dùng nhập thông tin đăng nhập.
         *  2. Kiểm tra thông tin username người dùng vừa nhập vào.
         *  3.
        **/

        public bool Login()
        {
            // Yêu cầu nhập thông tin đăng nhập.
            Console.WriteLine("----------------LOGIN INFORMATION----------------");
            Console.WriteLine("Username: ");
            var username = Console.ReadLine();
            Console.WriteLine("Password: ");
            var password = Console.ReadLine();
            // Tìm trong database thông tin tài khoản với username vừa nhập vào.
            // Trả về null nếu không tồn tại tài khoản trùng với username trên.
            // Trong trường hợp tồn tại bản ghi thì trả về thông tin account của
            // bản ghi đấy.
            YYAccount existingAccount = model.GetByUsername(username);
            // Nếu trả về null thì hàm login trả về false.
            if (existingAccount == null)
            {
                return false;
            }

            // Nếu chạy đến đây rồi thì `existingAccount` chắc chắn khác null.
            // Tiếp tục kiểm tra thông tin password.
            // Mã hoá password người dùng vừa nhập vào kèm theo muối lấy trong database
            // của bản ghi và so sánh với password đã mã hoá trong database.
            if (!existingAccount.CheckEncryptedPassword(password))
            {
                // Nếu không trùng thì trả về false, đăng nhập thất bại.
                return false;
            }

            // Trong trường hợp chạy đến đây thì thông tin tài khoản chắc chắn khác null
            // và password đã trùng nhau. Đăng nhập thành công.
            // Lưu thông tin vừa lấy ra trong database vào biến
            // `currentLoggedInYyAccount` của lớp Program.
            Program.currentLoggedInYyAccount = existingAccount;
            // Hàm trả về true, login thành công.
            return true;
        }

        private YYAccount GetAccountInformation()
        {
            Console.WriteLine("----------------REGISTER INFORMATION----------------");
            Console.WriteLine("Username: ");
            var username = Console.ReadLine();
            Console.WriteLine("Password: ");
            var password = Console.ReadLine();
            Console.WriteLine("Confirm Password: ");
            var cpassword = Console.ReadLine();
            Console.WriteLine("Balance: ");
            var balance = Utility.GetDecimalNumber();
            Console.WriteLine("Identity Card: ");
            var identityCard = Console.ReadLine();
            Console.WriteLine("Full Name: ");
            var fullName = Console.ReadLine();
            Console.WriteLine("Birthday: ");
            var birthday = Console.ReadLine();
            Console.WriteLine("Gender (1. Male |2. Female| 3.Others): ");
            var gender = Utility.GetInt32Number();
            Console.WriteLine("Email: ");
            var email = Console.ReadLine();
            Console.WriteLine("Phone Number: ");
            var phoneNumber = Console.ReadLine();
            Console.WriteLine("Address: ");
            var address = Console.ReadLine();

            var yyAccount = new YYAccount
            {
                Username = username,
                Password = password,
                Cpassword = cpassword,
                IdentityCard = identityCard,
                Gender = gender,
                Balance = balance,
                Address = address,
                Dob = birthday,
                FullName = fullName,
                Email = email,
                PhoneNumber = phoneNumber,
            };
            return yyAccount;
        }

        public void ShowAccountInfomation()
        {
            var currentAcount = model.GetByUsername(Program.currentLoggedInYyAccount.Username);
            if (currentAcount == null)
            {
                Program.currentLoggedInYyAccount = null;
                Console.WriteLine("Sai thong tin tai khoan hoac tai khoan hoac da xoa");
                return;
            }

            Console.WriteLine("So du tai khoan: ");
            Console.WriteLine(Program.currentLoggedInYyAccount.AccountNumber);
            Console.WriteLine("So du hien tai (VND): ");
            Console.WriteLine(Program.currentLoggedInYyAccount.Balance);
        }

        public void Deposit()
        {
            Console.WriteLine("Deposit.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter amount to deposit: ");
            var amount = Utility.GetUnsignDecimalNumber();
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
//            Program.currentLoggedIn = model.GetAccountByUserName(Program.currentLoggedIn.Username);
            var historyTransaction = new YYTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.WITHDRAW,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.UpdateBalance(Program.currentLoggedInYyAccount, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }
            Program.currentLoggedInYyAccount = model.GetByUsername(Program.currentLoggedInYyAccount.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }

        public void Transfer()
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            Console.WriteLine("Transfer.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter ReceiverAccountNumber: ");
            var accountNumber = Console.ReadLine();
            // 1. Lấy thông tin số dư mới nhất của tài khoản.
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from `account` where accountNumber = @accountNumber";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountNumber", accountNumber);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            if (!isExist)
            {
                throw new SpringHeroTransactionException("Invalid accountNumber");
            }
            DbConnection.Instance().CloseConnection();
            Console.WriteLine("Please enter amount to transfer: ");
            var amount = Utility.GetUnsignDecimalNumber();
            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
            var historyTransaction = new YYTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.TRANSFER,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = accountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.YTransaction(accountNumber, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }

            Program.currentLoggedInYyAccount = model.GetByUsername(Program.currentLoggedInYyAccount.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }

        public void transactionHistory()
        {
            Console.WriteLine("Nhap ngay bat dau: (yyyy-mm-dd)");
            string fromDate = Console.ReadLine();
            Console.WriteLine("Nhap ngay ket thuc: (yyyy-mm-dd)");
            string toDate = Console.ReadLine();
            List<YYTransaction> list = null;
            YYTransactionModel transactionModel = new YYTransactionModel();
            list = transactionModel.GetTransactionsByDate(fromDate, toDate);
            foreach (var transaction in list)
            {
                Console.WriteLine("Amount:" + transaction.Amount + " content: " + transaction.Content +
                                  " ReceiverAccountNumber:"
                                  + transaction.ReceiverAccountNumber + " SenderAccountNumber: " +
                                  transaction.SenderAccountNumber + " Created_date:"
                                  + transaction.CreatedAt + "\n");
            }

            Console.WriteLine("Do you want to print(y)");
            var ans = Console.ReadLine();
            if (ans.Equals("y"))
            {
                using (StreamWriter writetext = new StreamWriter("write.txt"))
                {
                    foreach (var transaction in list)
                    {
                        writetext.WriteLine("Amount:" + transaction.Amount + "content: " + transaction.Content +
                                            "receiverAccountNumber:"
                                            + transaction.ReceiverAccountNumber + "senderAccountNumber: " +
                                            transaction.SenderAccountNumber + "created_date:"
                                            + transaction.CreatedAt);
                    }
                }
            }
        }

        public void Withdraw()
        {
            Console.WriteLine("Withdraw.");
            Console.WriteLine("---------------------------------");
            Console.WriteLine("Please enter amount to transfer: ");
            var amount = Utility.GetUnsignDecimalNumber();
            var currentBlance = Program.currentLoggedInYyAccount.Balance;
            // 2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.
            if (amount > currentBlance)
            {
                throw new SpringHeroTransactionException("Not enough money!");
            }

            Console.WriteLine("Please enter message content: ");
            var content = Console.ReadLine();
//            Program.currentLoggedIn = model.GetAccountByUserName(Program.currentLoggedIn.Username);
            var historyTransaction = new YYTransaction
            {
                Id = Guid.NewGuid().ToString(),
                Type = YYTransaction.TransactionType.DEPOSIT,
                Amount = amount,
                Content = content,
                SenderAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                ReceiverAccountNumber = Program.currentLoggedInYyAccount.AccountNumber,
                Status = YYTransaction.ActiveStatus.DONE
            };
            if (model.UpdateBalance(Program.currentLoggedInYyAccount, historyTransaction))
            {
                Console.WriteLine("Transaction success!");
            }
            else
            {
                Console.WriteLine("Transaction fails, please try again!");
            }

            Program.currentLoggedInYyAccount = model.GetByUsername(Program.currentLoggedInYyAccount.Username);
            Console.WriteLine("Current balance: " + Program.currentLoggedInYyAccount.Balance);
            Console.WriteLine("Press enter to continue!");
            Console.ReadLine();
        }
    }
}