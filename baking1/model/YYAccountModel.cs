using System;
using System.Collections.Generic;
using baking1.entity;
using baking1.errors;
using MySql.Data.MySqlClient;

namespace baking1.model
{
    public class YYAccountModel
    {
        public Boolean Save(YYAccount account)
        {
            DbConnection.Instance().OpenConnection();
            string queryString = "insert into account" +
                                 "(accountNumber, username, password, balance, identityCard, fullName, " +
                                 "email, phoneNumber, address, dob, gender, salt)" +
                                 " values " +
                                 "(@accountNumber, @username, @password, @balance, @identityCard, @fullName, " +
                                 "@email, @phoneNumber, @address, @dob, @gender, @salt)";
            MySqlCommand cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@accountNumber", account.AccountNumber);
            cmd.Parameters.AddWithValue("@username", account.Username);
            cmd.Parameters.AddWithValue("@password", account.Password);
            cmd.Parameters.AddWithValue("@balance", account.Balance);
            cmd.Parameters.AddWithValue("@identityCard", account.IdentityCard);
            cmd.Parameters.AddWithValue("@fullName", account.FullName);
            cmd.Parameters.AddWithValue("@email", account.Email);
            cmd.Parameters.AddWithValue("@phoneNumber", account.PhoneNumber);
            cmd.Parameters.AddWithValue("@address", account.Address);
            cmd.Parameters.AddWithValue("@dob", account.Dob);
            cmd.Parameters.AddWithValue("@gender", account.Gender);
            cmd.Parameters.AddWithValue("@salt", account.Salt);

            cmd.ExecuteNonQuery();
            DbConnection.Instance().CloseConnection();
            return true;
        }

        public Boolean CheckExistUsername(string username)
        {
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from `account` where username = @username";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", username);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            DbConnection.Instance().CloseConnection();
            return isExist;
        }

        public bool YTransaction(String trecieverAccount, YYTransaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            var transaction = DbConnection.Instance().Connection.BeginTransaction(); // Khởi tạo transaction.
            try
            {
                //4.2 Trừ tiền người gửi.

                //4.2.1. Lấy thông tin số dư mới nhất của tài khoản.
                var queryBalance = "select balance from `account` where accountNumber = @accountNumber and status = 1";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@accountNumber",
                    Program.currentLoggedInYyAccount.AccountNumber);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                // Không tìm thấy tài khoản tương ứng, throw lỗi.
                if (!balanceReader.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SpringHeroTransactionException("Invalid username");
                }

                // Đảm bảo sẽ có bản ghi.
                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();
                //4.2.2 Kiểm tra số tiền chuyen di.
                if (historyTransaction.Amount > currentBalance)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }

                //4.3. Update số dư vào tài khoản.
                //4.3.1. Tính toán lại số tiền trong tài khoản.
                currentBalance -= historyTransaction.Amount;
                //4.3.2. Update số dư vào database.
                var updateAccountResult = 0;
                var queryUpdateAccountBalance =
                    "update `account` set balance = @balance where accountNumber = @accountNumber and status = 1";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@accountNumber",
                    Program.currentLoggedInYyAccount.AccountNumber);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();
                //4.4 Cong tien nguoi nhan
                //4.4.1  Lấy thông tin tài khoản nhận, đảm bảo tài khoản không bị khoá hoặc inactive.
                var queryString = "select * from `account` where accountNumber = @accountNumberR";
                var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
                cmd.Parameters.AddWithValue("@accountNumberR", trecieverAccount);
                var reader = cmd.ExecuteReader();
                var isExist = reader.Read();
                if (!isExist)
                {
                    throw new SpringHeroTransactionException("Invalid username");
                }

                var fullname = reader.GetString("fullName");
                Console.WriteLine(fullname);
                var accountReciverBalance = reader.GetDecimal("balance");
                accountReciverBalance += historyTransaction.Amount;
                reader.Close();
                //4.4.2 Update số dư vào database.
                var updateAccountReciver = 0;
                var queryUpdateAccountBalancReciveiver =
                    "update `account` set balance = @balanceR where accountNumber = @accountR and status = 1";
                var cmdUpdateAccountBalanceRecieve =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@accountR", trecieverAccount);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balanceR", accountReciverBalance);
                updateAccountReciver = cmdUpdateAccountBalanceRecieve.ExecuteNonQuery();
                // 4.5 Lưu thông tin transaction vào bảng transaction.
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transactions` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction = new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && updateAccountReciver == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (SpringHeroTransactionException e)
            {
                transaction.Rollback();
                return false;
            }

            DbConnection.Instance().CloseConnection();
            return false;
        }

        public bool UpdateBalance(YYAccount account, YYTransaction historyTransaction)
        {
            DbConnection.Instance().OpenConnection(); // đảm bảo rằng đã kết nối đến db thành công.
            var transaction = DbConnection.Instance().Connection.BeginTransaction(); // Khởi tạo transaction.

            try
            {
                /**
                 * 1. Lấy thông tin số dư mới nhất của tài khoản.
                 * 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw.
                 *     2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.                 
                 * 3. Update số dư vào tài khoản.
                 *     3.1. Tính toán lại số tiền trong tài khoản.
                 *     3.2. Update số tiền vào database.
                 * 4. Lưu thông tin transaction vào bảng transaction.
                 */

                // 1. Lấy thông tin số dư mới nhất của tài khoản.
                var queryBalance = "select balance from `account` where username = @username and status = 1";
                MySqlCommand queryBalanceCommand = new MySqlCommand(queryBalance, DbConnection.Instance().Connection);
                queryBalanceCommand.Parameters.AddWithValue("@username", account.Username);
                // queryBalanceCommand.Parameters.AddWithValue("@status", account.Status);
                var balanceReader = queryBalanceCommand.ExecuteReader();
                // Không tìm thấy tài khoản tương ứng, throw lỗi.
                if (!balanceReader.Read())
                {
                    // Không tồn tại bản ghi tương ứng, lập tức rollback transaction, trả về false.
                    // Hàm dừng tại đây.
                    throw new SpringHeroTransactionException("Invalid username");
                }

                // Đảm bảo sẽ có bản ghi.
                var currentBalance = balanceReader.GetDecimal("balance");
                balanceReader.Close();

                // 2. Kiểm tra kiểu transaction. Chỉ chấp nhận deposit và withdraw. 
                if (historyTransaction.Type != YYTransaction.TransactionType.DEPOSIT
                    && historyTransaction.Type != YYTransaction.TransactionType.WITHDRAW)
                {
                    throw new SpringHeroTransactionException("Invalid transaction type!");
                }

                // 2.1. Kiểm tra số tiền rút nếu kiểu transaction là withdraw.
                if (historyTransaction.Type == YYTransaction.TransactionType.WITHDRAW &&
                    historyTransaction.Amount > currentBalance)
                {
                    throw new SpringHeroTransactionException("Not enough money!");
                }

                // 3. Update số dư vào tài khoản.
                // 3.1. Tính toán lại số tiền trong tài khoản.
                if (historyTransaction.Type == YYTransaction.TransactionType.DEPOSIT)
                {
                    currentBalance += historyTransaction.Amount;
                }
                else
                {
                    currentBalance -= historyTransaction.Amount;
                }

                // 3.2. Update số dư vào database.
                var updateAccountResult = 0;
                var queryUpdateAccountBalance =
                    "update `account` set balance = @balance where username = @username and status = 1";
                var cmdUpdateAccountBalance =
                    new MySqlCommand(queryUpdateAccountBalance, DbConnection.Instance().Connection);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@username", account.Username);
                cmdUpdateAccountBalance.Parameters.AddWithValue("@balance", currentBalance);
                updateAccountResult = cmdUpdateAccountBalance.ExecuteNonQuery();

                // 4. Lưu thông tin transaction vào bảng transaction.
                var insertTransactionResult = 0;
                var queryInsertTransaction = "insert into `transactions` " +
                                             "(id, type, amount, content, senderAccountNumber, receiverAccountNumber, status) " +
                                             "values (@id, @type, @amount, @content, @senderAccountNumber, @receiverAccountNumber, @status)";
                var cmdInsertTransaction =
                    new MySqlCommand(queryInsertTransaction, DbConnection.Instance().Connection);
                cmdInsertTransaction.Parameters.AddWithValue("@id", historyTransaction.Id);
                cmdInsertTransaction.Parameters.AddWithValue("@type", historyTransaction.Type);
                cmdInsertTransaction.Parameters.AddWithValue("@amount", historyTransaction.Amount);
                cmdInsertTransaction.Parameters.AddWithValue("@content", historyTransaction.Content);
                cmdInsertTransaction.Parameters.AddWithValue("@senderAccountNumber",
                    historyTransaction.SenderAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@receiverAccountNumber",
                    historyTransaction.ReceiverAccountNumber);
                cmdInsertTransaction.Parameters.AddWithValue("@status", historyTransaction.Status);
                insertTransactionResult = cmdInsertTransaction.ExecuteNonQuery();

                if (updateAccountResult == 1 && insertTransactionResult == 1)
                {
                    transaction.Commit();
                    return true;
                }
            }
            catch (SpringHeroTransactionException e)
            {
                transaction.Rollback();
                return false;
            }

            DbConnection.Instance().CloseConnection();
            return false;
        }

        public YYAccount GetByUsername(string username)
        {
            YYAccount account = null;
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from `account` where username = @username";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@username", username);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            if (isExist)
            {
                account = new YYAccount
                {
                    AccountNumber = reader.GetString("accountNumber"),
                    Username = reader.GetString("username"),
                    Password = reader.GetString("password"),
                    Salt = reader.GetString("salt"),
                    FullName = reader.GetString("fullName"),
                    Balance = reader.GetInt32("balance")
                };
            }

            DbConnection.Instance().CloseConnection();
            return account;
        }
    }
}