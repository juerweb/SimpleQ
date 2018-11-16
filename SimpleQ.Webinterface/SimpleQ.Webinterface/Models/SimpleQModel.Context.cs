﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SimpleQ.Webinterface.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class SimpleQDBEntities : DbContext
    {
        public SimpleQDBEntities()
            : base("name=SimpleQDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AnswerOption> AnswerOptions { get; set; }
        public virtual DbSet<AnswerType> AnswerTypes { get; set; }
        public virtual DbSet<BaseQuestionType> BaseQuestionTypes { get; set; }
        public virtual DbSet<Bill> Bills { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<DsgvoConstraint> DsgvoConstraints { get; set; }
        public virtual DbSet<FaqEntry> FaqEntries { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<PredefinedAnswerOption> PredefinedAnswerOptions { get; set; }
        public virtual DbSet<Survey> Surveys { get; set; }
        public virtual DbSet<SurveyCategory> SurveyCategories { get; set; }
        public virtual DbSet<Vote> Votes { get; set; }
    
        public virtual ObjectResult<Nullable<int>> sp_CheckExceededSurveyData()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<int>>("sp_CheckExceededSurveyData");
        }
    }
}
