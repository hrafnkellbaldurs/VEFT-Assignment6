using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using CoursesAPI.Models;
using CoursesAPI.Services.DataAccess;
using CoursesAPI.Services.Services;


namespace CoursesAPI.Controllers
{
    [RoutePrefix("api/courses")]
    public class CoursesController : ApiController
    {
        private readonly CoursesServiceProvider _service;

        public CoursesController()
        {
            _service = new CoursesServiceProvider(new UnitOfWork<AppDataContext>());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="semester"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetCoursesBySemester(string semester = null)
        {
            var language = Request.Headers.AcceptLanguage.First().ToString();
            return Ok(_service.GetCourseInstancesBySemester(language, semester));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("/{id}")]
        public IHttpActionResult GetCoursesById(int id)
        {
            return Ok();
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IHttpActionResult AddTeacher(int id, AddTeacherViewModel model)
        {
            return StatusCode(HttpStatusCode.Created);
        }
    }
}
