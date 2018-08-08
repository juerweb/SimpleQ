﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the CodeValidationModel for the xyPage
    /// </summary>
    public class CodeValidationModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public CodeValidationModel(bool isValid, string companyName, string departmentName, int code): this()
        {
            this.isValid = isValid;
            this.companyName = companyName;
            this.departmentName = departmentName;
            this.code = code;
        }

        public CodeValidationModel()
        {

        }
        #endregion

        #region Fields
        private Boolean isValid;
        private String companyName;
        private String departmentName;
        private int code;
        #endregion

        #region Properties + Getter/Setter Methods
        public bool IsValid { get => isValid; set => isValid = value; }
        public string CompanyName { get => companyName; set => companyName = value; }
        public string DepartmentName { get => departmentName; set => departmentName = value; }
        public int Code { get => code; set => code = value; }
        #endregion

        #region Methods
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}