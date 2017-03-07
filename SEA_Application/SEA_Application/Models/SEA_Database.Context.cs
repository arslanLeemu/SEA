﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SEA_Application.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class SEA_DatabaseEntities : DbContext
    {
        public SEA_DatabaseEntities()
            : base("name=SEA_DatabaseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AspNetAnnouncement> AspNetAnnouncements { get; set; }
        public virtual DbSet<AspNetAnnouncement_Subject> AspNetAnnouncement_Subject { get; set; }
        public virtual DbSet<AspNetAssignment> AspNetAssignments { get; set; }
        public virtual DbSet<AspNetAssignment_Topic> AspNetAssignment_Topic { get; set; }
        public virtual DbSet<AspNetAttendance> AspNetAttendances { get; set; }
        public virtual DbSet<AspNetClass> AspNetClasses { get; set; }
        public virtual DbSet<AspNetExam> AspNetExams { get; set; }
        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetStudent> AspNetStudents { get; set; }
        public virtual DbSet<AspNetStudent_Announcement> AspNetStudent_Announcement { get; set; }
        public virtual DbSet<AspNetStudent_Exam> AspNetStudent_Exam { get; set; }
        public virtual DbSet<AspNetStudent_Subject> AspNetStudent_Subject { get; set; }
        public virtual DbSet<AspNetStudent_Test> AspNetStudent_Test { get; set; }
        public virtual DbSet<AspNetSubject> AspNetSubjects { get; set; }
        public virtual DbSet<AspNetTeacher> AspNetTeachers { get; set; }
        public virtual DbSet<AspNetTest> AspNetTests { get; set; }
        public virtual DbSet<AspNetTest_Topic> AspNetTest_Topic { get; set; }
        public virtual DbSet<AspNetTopic> AspNetTopics { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Student_Assignment> Student_Assignment { get; set; }
    }
}