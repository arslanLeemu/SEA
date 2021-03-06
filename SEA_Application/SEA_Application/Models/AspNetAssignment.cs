//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class AspNetAssignment
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public AspNetAssignment()
        {
            this.AspNetAssignment_Topic = new HashSet<AspNetAssignment_Topic>();
            this.Student_Assignment = new HashSet<Student_Assignment>();
        }
    
        public int Id { get; set; }
        public Nullable<int> SubjectID { get; set; }
        public Nullable<int> ClassID { get; set; }
        public Nullable<System.DateTime> PublishDate { get; set; }
        public Nullable<System.DateTime> DueDate { get; set; }
        public string Description { get; set; }
        public Nullable<int> TotalMarks { get; set; }
        public Nullable<int> Weightage { get; set; }
        public string Title { get; set; }
        public string FileName { get; set; }
        public string TeacherID { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<AspNetAssignment_Topic> AspNetAssignment_Topic { get; set; }
        public virtual AspNetClass AspNetClass { get; set; }
        public virtual AspNetSubject AspNetSubject { get; set; }
        public virtual AspNetUser AspNetUser { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Student_Assignment> Student_Assignment { get; set; }
    }
}
