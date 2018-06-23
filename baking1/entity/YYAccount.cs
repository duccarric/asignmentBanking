using System;
using System.Collections.Generic;
using baking1.model;
using baking1.utility;


namespace baking1.entity
{
    public class YYAccount
    {
        private string _accountNumber; // id
        private string _username; // unique
        private string _password;
        private string _cpassword;
        private string _salt;
        private decimal _balance;
        private string _identityCard; // unique
        private string _fullName;
        private string _email; // unique
        private string _phoneNumber; // unique
        private string _address;
        private string _dob;
        private int _gender; // 1. male | 2. female | 3. rather not say.
        private string _createdAt;
        private string _updatedAt;
        private int _status; // 1. active | 2. locked | 0. inactive.

        public YYAccount()
        {
            GenerateAccountNumber();
            GenerateSalt();
        }

        // Tham số là chuỗi password chưa mã hoá mà người dùng nhập vào.
        public bool CheckEncryptedPassword(string password) 
        {         
            // Tiến hành mã hoá password người dùng nhập vào kèm theo muối được lấy từ db.
            // Trả về một chuỗi password đã mã hoá.            
            var checkPassword = Hash.EncryptedString(password, _salt);
            // So sánh hai chuỗi password đã mã hoá. Nếu trùng nhau thì trả về true.
            // Nếu không trùng nhau thì trả về false.
            return (checkPassword == _password);
        }        
        public void EncryptPassword()
        {
            if (string.IsNullOrEmpty(_password))
            {
                throw new ArgumentNullException("Password is null or empyt.");
            }
            _password = Hash.EncryptedString(_password, _salt);
        }

        private void GenerateAccountNumber()
        {
            _accountNumber = Guid.NewGuid().ToString(); // unique
        }

        private void GenerateSalt()
        {
            _salt = Guid.NewGuid().ToString().Substring(0, 7); // !unique
        }

        public string AccountNumber
        {
            get { return _accountNumber; }
            set { _accountNumber = value; }
        }

        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        public string Cpassword
        {
            get { return _cpassword; }
            set { _cpassword = value; }
        }

        public string Salt
        {
            get { return _salt; }
            set { _salt = value; }
        }

        public decimal Balance
        {
            get { return _balance; }
            set { _balance = value; }
        }

        public string IdentityCard
        {
            get { return _identityCard; }
            set { _identityCard = value; }
        }

        public string FullName
        {
            get { return _fullName; }
            set { _fullName = value; }
        }

        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; }
        }

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public string Dob
        {
            get { return _dob; }
            set { _dob = value; }
        }

        public int Gender
        {
            get { return _gender; }
            set { _gender = value; }
        }

        public string CreatedAt
        {
            get { return _createdAt; }
            set { _createdAt = value; }
        }

        public string UpdatedAt
        {
            get { return _updatedAt; }
            set { _updatedAt = value; }
        }

        public int Status
        {
            get { return _status; }
            set { _status = value; }
        }

        // Làm nhiệm vụ validate account, trả về một dictionary các lỗi.
        public Dictionary<string, string> CheckValidate()
        {
            YYAccountModel model = new YYAccountModel();
            Dictionary<string, string> errors = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(this._username))
            {
                errors.Add("username", "Username can not be null or empty.");
            } else if (this._username.Length < 6)
            {
                errors.Add("username", "Username is too short. At least 6 characters.");
            }else if (model.CheckExistUsername(this._username))
            {
                // Check trùng username.
                errors.Add("username", "Username is exist. Please try another one.");
            }
            if (_cpassword != _password)
            {
                errors.Add("password", "Confirm password does not match.");
            }

            // if else if else ...
            return errors;
        }
    }
}