using System;
using System.Collections.Generic;
using baking1.entity;
using MySql.Data.MySqlClient;

namespace baking1.model
{
    public class YYTransactionModel
    {
        public List<YYTransaction> GetTransactionsByDate(string fromDate, string toDate)
        {
            List<YYTransaction> list = new List<YYTransaction>();
            YYTransaction yyTransaction = null;
            DbConnection.Instance().OpenConnection();
            var queryString = "select * from `transactions` where created_date < @toDate and created_date > @fromDate";
            var cmd = new MySqlCommand(queryString, DbConnection.Instance().Connection);
            cmd.Parameters.AddWithValue("@fromDate", fromDate);
            cmd.Parameters.AddWithValue("@toDate", toDate);
            var reader = cmd.ExecuteReader();
            var isExist = reader.Read();
            if (!isExist)
            {
                Console.WriteLine("Not record is finded");
            }

            while (reader.Read())
            {
                yyTransaction = new YYTransaction();
                yyTransaction.Id = reader.GetString("id");
                yyTransaction.Type = (YYTransaction.TransactionType) reader.GetInt32("type");
                yyTransaction.Amount = reader.GetDecimal("amount");
                yyTransaction.Content = reader.GetString("content");
                yyTransaction.ReceiverAccountNumber = reader.GetString("receiverAccountNumber");
                yyTransaction.SenderAccountNumber = reader.GetString("senderAccountNumber");
                DateTime time = reader.GetDateTime("created_date");
                string date = time.ToLongDateString();
                yyTransaction.CreatedAt = date;
                list.Add(yyTransaction);
            }

            DbConnection.Instance().CloseConnection();
            return list;
        }
    }
}