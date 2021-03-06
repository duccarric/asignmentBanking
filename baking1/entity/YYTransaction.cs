﻿namespace baking1.entity
{
    public class YYTransaction
    { 
        public enum ActiveStatus
        {
            PROCESSING = 1,
            DONE = 2,
            REJECT = 0,
            DELETED = -1,
        }

        public enum TransactionType
        {
            DEPOSIT = 1,
            WITHDRAW = 2,
            TRANSFER = 3
        }
        private string _id;
        private string _createdAt;
        private string _updatedAt;
        private TransactionType _type;
        private decimal _amount;
        private string _content;
        private string _senderAccountNumber;
        private string _receiverAccountNumber;
        private ActiveStatus _status;

        public YYTransaction()
        {
        }

        public YYTransaction(string id, string createdAt, string updatedAt, TransactionType type, decimal amount, string content, string senderAccountNumber, string receiverAccountNumber, ActiveStatus status)
        {
            _id = id;
            _createdAt = createdAt;
            _updatedAt = updatedAt;
            _type = type;
            _amount = amount;
            _content = content;
            _senderAccountNumber = senderAccountNumber;
            _receiverAccountNumber = receiverAccountNumber;
            _status = status;
        }

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public string UpdatedAt
        {
            get { return _updatedAt; }
        }

        public TransactionType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public decimal Amount
        {
            get { return _amount; }
            set { _amount = value; }
        }

        public string Content
        {
            get { return _content; }
            set { _content = value; }
        }

        public string SenderAccountNumber
        {
            get { return _senderAccountNumber; }
            set { _senderAccountNumber = value; }
        }

        public string ReceiverAccountNumber
        {
            get { return _receiverAccountNumber; }
            set { _receiverAccountNumber = value; }
        }

        public ActiveStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }
    }
}