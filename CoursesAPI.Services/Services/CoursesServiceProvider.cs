using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.AccessControl;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;

namespace CoursesAPI.Services.Services
{
	public class CoursesServiceProvider
	{
		private readonly IUnitOfWork _uow;

		private readonly IRepository<CourseInstance> _courseInstances;
		private readonly IRepository<TeacherRegistration> _teacherRegistrations;
		private readonly IRepository<CourseTemplate> _courseTemplates; 
		private readonly IRepository<Person> _persons;

		public CoursesServiceProvider(IUnitOfWork uow)
		{
			_uow = uow;

			_courseInstances      = _uow.GetRepository<CourseInstance>();
			_courseTemplates      = _uow.GetRepository<CourseTemplate>();
			_teacherRegistrations = _uow.GetRepository<TeacherRegistration>();
			_persons              = _uow.GetRepository<Person>();
		}

		/// <summary>
		/// You should implement this function, such that all tests will pass.
		/// </summary>
		/// <param name="courseInstanceID">The ID of the course instance which the teacher will be registered to.</param>
		/// <param name="model">The data which indicates which person should be added as a teacher, and in what role.</param>
		/// <returns>Should return basic information about the person.</returns>
		public PersonDTO AddTeacherToCourse(int courseInstanceID, AddTeacherViewModel model)
		{
			// Check if courseID is correct
		    var courseInstance = (from c in _courseInstances.All()
		        where c.ID == courseInstanceID
		        select c).SingleOrDefault();

		    if (courseInstance == null){
		        throw new AppObjectNotFoundException();
		    }

            // Check if teacher SSN is in the system
		    var teacher = (from p in _persons.All()
		        where p.SSN == model.SSN
		        select p).SingleOrDefault();

		    if (teacher == null){
		        throw new AppObjectNotFoundException();
		    }

            // Get the list of teachers assigned for this course
            var teachersForCourse = (from tr in _teacherRegistrations.All()
                                     where tr.CourseInstanceID == courseInstanceID
                                     select tr).ToList();

		    // If the teacher being registered is a main teacher
            // and the course already has a main teacher registered;
            // throw an exception
            if (model.Type == TeacherType.MainTeacher && 
                teachersForCourse.Any(t => t.Type == TeacherType.MainTeacher))
            {
                throw new AppValidationException("COURSE_ALREADY_HAS_A_MAIN_TEACHER");
            }

            // If the teacher being registered is already registered as a
            // teacher in the course; throw an exception
		    if (teachersForCourse.Any(t => t.SSN == model.SSN))
		    {
		        throw new AppValidationException("PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE");
		    }

            // If all seems to be OK, register the teacher to the course
		    var teacherRegistration = new TeacherRegistration
		    {
                SSN = model.SSN,
                CourseInstanceID = courseInstanceID,
                Type = model.Type
		    };

            _teacherRegistrations.Add(teacherRegistration);
            _uow.Save();

            // Display the teacher that was added to the course
		    var personDTO = new PersonDTO
		    {
                Name = teacher.Name,
                SSN = teacher.SSN
		    };

			return personDTO;
		}

        /// <summary>
        /// Finds CourseInstances taught on the given semester.
        /// If no language is specified, the language "is" is used instead.
        /// If no semester is given, the current semester "20153" is used instead.
        /// If no page number is given, the first page is used.
        /// If the page number exceeds the total number of pages, an empty list is returned.
        /// </summary>
        /// <param name="language">The language the user has specified for the name of each course.</param>
        /// <param name="semester">The semester to get courses from</param>
        /// <param name="page">The number of the page to get from the list of course instances</param>
        /// <returns>A List of CourseInstanceDTOs taught on the given semester</returns>
        public Envelope<List<CourseInstanceDTO>> GetCourseInstancesBySemester(string language = null, string semester = null, int page = 1)
        {
            const string langIS = "is";  

            const int ITEMS_PER_PAGE = 10;

            // Assign a default semester if no semester is given
			if (string.IsNullOrEmpty(semester))
			{
				semester = "20153";
			}

            // Assign a default language if no language is specified
            if (string.IsNullOrEmpty(language))
            {
                language = langIS;
            }
         
            // Construct the list of courses tought in the given semester 
            // with the course name language set to the users preference
            var courses = (from c in _courseInstances.All()
                           join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
                           where c.SemesterID == semester
                           select new CourseInstanceDTO
                           {
                               Name = language == langIS? ct.Name : ct.NameEN,
                               TemplateID = ct.CourseID,
                               CourseInstanceID = c.ID,
                               MainTeacher = ""
                           }).OrderBy(dto => dto.CourseInstanceID).Skip((page - 1) * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE).ToList();


            // Find main teacher name
            foreach (var ciDTO in courses)
		    {
		        var mainTeacherRegistration = (from tr in _teacherRegistrations.All()
		            where tr.CourseInstanceID == ciDTO.CourseInstanceID
		            where tr.Type == TeacherType.MainTeacher
		            select tr).SingleOrDefault();

		        if (mainTeacherRegistration != null)
		        {
		            var mainTeacher = (from p in _persons.All()
                                       where p.SSN == mainTeacherRegistration.SSN
                                       select p).SingleOrDefault();

                    if (mainTeacher != null)
                    {
                        ciDTO.MainTeacher = mainTeacher.Name;
                    }
		        } 
		    }

            // Total number of courses tought in the given semester
            var coursesTotalCount = (from c in _courseInstances.All()
                                     join ct in _courseTemplates.All() on c.CourseID equals ct.CourseID
                                     where c.SemesterID == semester
                                     select new object()).Count();

            // Get the total number of pages in the collection
            var pageCount = (int) (Math.Ceiling(((double)coursesTotalCount) / ((double)ITEMS_PER_PAGE)));

            // Construct the envelope to return
            var envelope = new Envelope<List<CourseInstanceDTO>>
            {
                Items = courses,
                Paging = new Envelope<List<CourseInstanceDTO>>.PagingInfo
                {
                    PageCount = pageCount,
                    PageNumber = page,
                    PageSize = ITEMS_PER_PAGE,
                    TotalNumberOfItems = coursesTotalCount
                }
            };


			return envelope;
		}
	}
}
