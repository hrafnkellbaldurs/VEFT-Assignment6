using System.Collections.Generic;
using System.Linq;
using CoursesAPI.Models;
using CoursesAPI.Services.Exceptions;
using CoursesAPI.Services.Models.Entities;
using CoursesAPI.Services.Services;
using CoursesAPI.Tests.MockObjects;
using CoursesAPI.Tests.TestExtensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoursesAPI.Tests.Services
{
	[TestClass]
	public class CourseServicesTests
	{
		private MockUnitOfWork<MockDataContext> _mockUnitOfWork;
		private CoursesServiceProvider _service;
		private List<TeacherRegistration> _teacherRegistrations;

        private const string SSN_DABS    = "1203735289";
		private const string SSN_GUNNA   = "1234567890";
        private const string SSN_MARCEL  = "1234567891";
        private const string INVALID_SSN = "9876543210";

	    private const string NAME_DABS = "Daníel B. Sigurgeirsson";
        private const string NAME_GUNNA  = "Guðrún Guðmundsdóttir";
	    private const string NAME_MARCEL = "Marcel Kyas";

		private const int COURSEID_VEFT_20153 = 1337;
		private const int COURSEID_VEFT_20163 = 1338;
        private const int COURSEID_TSAM_20153 = 1339;
        private const int INVALID_COURSEID    = 9999;

        [TestInitialize]
        public void Setup()
		{
			_mockUnitOfWork = new MockUnitOfWork<MockDataContext>();

			#region Persons
			var persons = new List<Person>
			{
				// Of course I'm the first person,
				// did you expect anything else?
				new Person
				{
					ID    = 1,
					Name  = NAME_DABS,
					SSN   = SSN_DABS,
					Email = "dabs@ru.is"
				},
				new Person
				{
					ID    = 2,
					Name  = NAME_GUNNA,
					SSN   = SSN_GUNNA,
					Email = "gunna@ru.is"
				},
                new Person
                {
                    ID    = 3,
                    Name  = NAME_MARCEL,
                    SSN   = SSN_MARCEL,
                    Email = "marcel@ru.is"
                }
            };
			#endregion

			#region Course templates

			var courseTemplates = new List<CourseTemplate>
			{
				new CourseTemplate
				{
					CourseID    = "T-514-VEFT",
					Description = "Í þessum áfanga verður fjallað um vefþj...",
					Name        = "Vefþjónustur"
				},
                new CourseTemplate
                {
                    CourseID    = "T-409-TSAM",
                    Description = "Í þessum áfanga verður fjallað um samskipti...",
                    Name        = "Tölvusamskipti"
                }
            };
			#endregion

			#region Courses
			var courses = new List<CourseInstance>
			{
				new CourseInstance
				{
					ID         = COURSEID_VEFT_20153,
					CourseID   = "T-514-VEFT",
					SemesterID = "20153"
				},
				new CourseInstance
				{
					ID         = COURSEID_VEFT_20163,
					CourseID   = "T-514-VEFT",
					SemesterID = "20163"
				},
                new CourseInstance
                {
                    ID         = COURSEID_TSAM_20153,
                    CourseID   = "T-409-TSAM",
                    SemesterID = "20153"
                }
            };
			#endregion

			#region Teacher registrations
			_teacherRegistrations = new List<TeacherRegistration>
			{
				new TeacherRegistration
				{
					ID               = 101,
					CourseInstanceID = COURSEID_VEFT_20153,
					SSN              = SSN_DABS,
					Type             = TeacherType.MainTeacher
				}
            };
			#endregion

			_mockUnitOfWork.SetRepositoryData(persons);
			_mockUnitOfWork.SetRepositoryData(courseTemplates);
			_mockUnitOfWork.SetRepositoryData(courses);
			_mockUnitOfWork.SetRepositoryData(_teacherRegistrations);

			_service = new CoursesServiceProvider(_mockUnitOfWork);
		}

		#region GetCoursesBySemester
		/// <summary>
		/// The function should return an empty list if no 
		/// courses are defined for the semester
		/// </summary>
		[TestMethod]
		public void GetCoursesBySemester_ReturnsEmptyListWhenNoDataDefined()
		{
			// Arrange:
		    // Act:
		    var DTOList = _service.GetCourseInstancesBySemester(null ,"20173", 1).Items;
            // Note: the method uses test data defined in [TestInitialize]

            // Assert:
            Assert.AreEqual(0, DTOList.Count);
		}

        /// <summary>
		/// The function should return all courses on a given semester
		/// (no more, no less).
		/// </summary>
		[TestMethod]
        public void GetCoursesBySemester_ReturnsAllCoursesOnAGivenSemester()
        {
            // Arrange:
            // Act:
            var DTOList1 = _service.GetCourseInstancesBySemester(null, "20153", 1);
            var DTOList2 = _service.GetCourseInstancesBySemester(null, "20163", 1);

            // Assert:
            Assert.AreEqual(2, DTOList1.Items.Count);
            Assert.AreEqual(1, DTOList2.Items.Count);

            // Testing if data is being set up right
            var veft20163DTO = DTOList2.Items[0];
            Assert.AreEqual(COURSEID_VEFT_20163, veft20163DTO.CourseInstanceID);
            Assert.AreEqual("Vefþjónustur", veft20163DTO.Name );
            Assert.AreEqual("T-514-VEFT", veft20163DTO.TemplateID);
            Assert.AreEqual("", veft20163DTO.MainTeacher);
            // Note: the method uses test data defined in [TestInitialize]
        }

        /// <summary>
        /// The function should return courses tought on
        /// the semester 20153 if no semester is given.
        /// </summary>
        [TestMethod]
        public void GetCoursesBySemester_ReturnsCoursesForCurrentSemesterIfNoSemester()
        {
            // Arrange:
            // Act:
            var DTOList = _service.GetCourseInstancesBySemester().Items;

            // Assert:
            Assert.AreEqual(2, DTOList.Count);
            // Note: the method uses test data defined in [TestInitialize]
        }

        /// <summary>
        /// For each course returned, the name of the main teacher
        /// of the course should be included.
        /// </summary>
        [TestMethod]
        public void GetCoursesBySemester_ForEachCourseReturnedMainTeacherNameIsIncluded()
        {
            // Arrange:
            // Act:
            var DTOList = _service.GetCourseInstancesBySemester(null,"20153", 1).Items;

            // Assert:
            foreach (var ciDTO in DTOList)
            {
                Assert.AreNotEqual(null, ciDTO.MainTeacher);
            }
            // Note: the method uses test data defined in [TestInitialize]
        }

        /// <summary>
        /// If the main teacher hasn't been defined, the name of the main
        /// teacher should be returned as an empty string.
        /// </summary>
        [TestMethod]
        public void GetCoursesBySemester_MainTeacherNameIsEmptyStringIfNoMainTeacherDefined()
        {
            // Arrange:
            // Act:
            var DTOList = _service.GetCourseInstancesBySemester(null, "20153", 1).Items;

            // Assert:
            foreach (var ciDTO in DTOList)
            {
                if (ciDTO.CourseInstanceID == COURSEID_TSAM_20153)
                {
                    Assert.AreEqual("", ciDTO.MainTeacher);
                }
            }
            // Note: the method uses test data defined in [TestInitialize]
        }



        /// <summary>
        /// If the main teacher has been defined, the name of the main
        /// teacher should be returned.
        /// </summary>
        [TestMethod]
        public void GetCoursesBySemester_ReturnNameOfMainTeacherWhenItIsDefined()
        {
            // Arrange:
            // Act:
            var DTOList = _service.GetCourseInstancesBySemester(null, "20153", 1).Items;

            // Assert:
            foreach (var ciDTO in DTOList)
            {
                if (ciDTO.CourseInstanceID == COURSEID_VEFT_20153)
                {
                    Assert.AreEqual(NAME_DABS, ciDTO.MainTeacher);
                }
            }
            // Note: the method uses test data defined in [TestInitialize]
        }

        #endregion

        #region AddTeacher

        /// <summary>
        /// Adds a main teacher to a course which doesn't have a
        /// main teacher defined already (see test data defined above).
        /// </summary>
        [TestMethod]
		public void AddTeacher_WithValidTeacherAndCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			var prevCount = _teacherRegistrations.Count;
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			var dto = _service.AddTeacherToCourse(COURSEID_VEFT_20163, model);

			// Assert:

			// Check that the dto object is correctly populated:
			Assert.AreEqual(SSN_GUNNA, dto.SSN);
			Assert.AreEqual(NAME_GUNNA, dto.Name);

			// Ensure that a new entity object has been created:
			var currentCount = _teacherRegistrations.Count;
			Assert.AreEqual(prevCount + 1, currentCount);

			// Get access to the entity object and assert that
			// the properties have been set:
			var newEntity = _teacherRegistrations.Last();
			Assert.AreEqual(COURSEID_VEFT_20163, newEntity.CourseInstanceID);
			Assert.AreEqual(SSN_GUNNA, newEntity.SSN);
			Assert.AreEqual(TeacherType.MainTeacher, newEntity.Type);

			// Ensure that the Unit Of Work object has been instructed
			// to save the new entity object:
			Assert.IsTrue(_mockUnitOfWork.GetSaveCallCount() > 0);
		}

		[TestMethod]
		[ExpectedException(typeof(AppObjectNotFoundException))]
		public void AddTeacher_InvalidCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.AssistantTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			_service.AddTeacherToCourse(INVALID_COURSEID, model);
		}

		/// <summary>
		/// Ensure it is not possible to add a person as a teacher
		/// when that person is not registered in the system.
		/// </summary>
		[TestMethod]
		[ExpectedException(typeof (AppObjectNotFoundException))]
		public void AddTeacher_InvalidTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = INVALID_SSN,
				Type = TeacherType.MainTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			_service.AddTeacherToCourse(COURSEID_VEFT_20153, model);
		}

		/// <summary>
		/// In this test, we test that it is not possible to
		/// add another main teacher to a course, if one is already
		/// defined.
		/// </summary>
		[TestMethod]
		[ExpectedExceptionWithMessage(typeof (AppValidationException), "COURSE_ALREADY_HAS_A_MAIN_TEACHER")]
		public void AddTeacher_AlreadyWithMainTeacher()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_GUNNA,
				Type = TeacherType.MainTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			_service.AddTeacherToCourse(COURSEID_VEFT_20153, model);
		}

		/// <summary>
		/// In this test, we ensure that a person cannot be added as a
		/// teacher in a course, if that person is already registered
		/// as a teacher in the given course.
		/// </summary>
		[TestMethod]
		[ExpectedExceptionWithMessage(typeof (AppValidationException), "PERSON_ALREADY_REGISTERED_TEACHER_IN_COURSE")]
		public void AddTeacher_PersonAlreadyRegisteredAsTeacherInCourse()
		{
			// Arrange:
			var model = new AddTeacherViewModel
			{
				SSN  = SSN_DABS,
				Type = TeacherType.AssistantTeacher
			};
			// Note: the method uses test data defined in [TestInitialize]

			// Act:
			_service.AddTeacherToCourse(COURSEID_VEFT_20153, model);
		}

		#endregion
	}
}
