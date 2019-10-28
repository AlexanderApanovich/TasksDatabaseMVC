﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TasksDatabase.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }


        public int CourseId { get; set; }
        public int TaskTypeId { get; set; }
        public int? DeveloperId { get; set; }
        public int? ReviewerId { get; set; }
        public Course Course { get; set; }
        public TaskType TaskType { get; set; }
        public User Developer { get; set; }
        public User Reviewer { get; set; }

    }
}